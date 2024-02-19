using Azure;
using Azure.AI.ContentSafety;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;
using System.Text;
using System.Web;
using Microsoft.Extensions.Caching.Memory;

namespace TagzApp.Storage.Postgres.SafetyModeration;

public class AzureSafetyModeration : INotifyNewMessages
{
	private const string KEY_BLOCKEDUSERS_CACHE = "blockedUsers";
	private readonly IMemoryCache _Cache;
	private INotifyNewMessages _NotifyNewMessages;
	private readonly IServiceProvider _ServiceProvider;
	private readonly ILogger<AzureSafetyModeration> _AzureSafetyLogger;
	private readonly bool _Enabled;
	private readonly string? _ContentSafetyKey;
	private readonly string? _ContentSafetyEndpoint;

	public AzureSafetyModeration(
		IMemoryCache cache,
		INotifyNewMessages notifyNewMessages,
		IServiceProvider serviceProvider,
		IConfigureTagzApp configureTagzApp,
		ILogger<AzureSafetyModeration> azureSafetyLogger)
	{
		_Cache = cache;
		_NotifyNewMessages = notifyNewMessages;
		_ServiceProvider = serviceProvider;
		_AzureSafetyLogger = azureSafetyLogger;

		var config = configureTagzApp.GetConfigurationById<AzureSafetyConfiguration>(AzureSafetyConfiguration.ConfigurationKey).GetAwaiter().GetResult();

		_Enabled = config.Enabled;

		_ContentSafetyKey = config.Key;
		_ContentSafetyEndpoint = config.Endpoint;

		using IServiceScope scope = ReloadBlockedUserCache();

	}

	public void NotifyApprovedContent(string hashtag, Content content, ModerationAction action)
	{

		_NotifyNewMessages.NotifyApprovedContent(hashtag, content, action);
		return;

	}

	public void NotifyRejectedContent(string hashtag, Content content, ModerationAction action)
	{

		_NotifyNewMessages.NotifyRejectedContent(hashtag, content, action);
		return;

	}

	public void NotifyNewContent(string hashtag, Content content)
	{

		// TODO: Establish a notification pipeline that allows for multiple moderation providers

		// Check if this content is created by one of the blocked users listed in the cache
		var usersBlocked = _Cache.GetOrCreate(KEY_BLOCKEDUSERS_CACHE, _ => new List<(string Provider, string UserName)>());
		var isBlocked = usersBlocked
			.Any(a => a.Provider.Equals(content.Provider, StringComparison.InvariantCultureIgnoreCase)
				&& a.UserName.Equals('@' + content.Author.UserName.TrimStart('@'), StringComparison.InvariantCultureIgnoreCase));

		if (isBlocked)
		{
			using var scope = _ServiceProvider.CreateScope();
			var moderationRepository = scope.ServiceProvider.GetRequiredService<IModerationRepository>();
			moderationRepository.ModerateWithReason("BLOCKED-USER", content.Provider, content.ProviderId, ModerationState.Rejected, "Blocked User").GetAwaiter().GetResult();

			_NotifyNewMessages.NotifyNewContent(hashtag, content);
			_NotifyNewMessages.NotifyRejectedContent(hashtag, content, new ModerationAction
			{
				Provider = content.Provider,
				ProviderId = content.ProviderId,
				State = ModerationState.Rejected,
				Timestamp = DateTimeOffset.UtcNow,
				Moderator = "BLOCKED-USER",
				Reason = "Blocked User"
			});
			return;
		}

		if (!_Enabled)
		{
			_NotifyNewMessages.NotifyNewContent(hashtag, content);
			return;
		}

		// use Azure Content Safety API to check the content
		// if it's unsafe, add a moderation action that indicates Azure rejected it
		// and log the rejection

		var client = new ContentSafetyClient(new Uri(_ContentSafetyEndpoint), new AzureKeyCredential(_ContentSafetyKey));
		var clearText = HtmlCleaner.UnHtml(content.Text);

		// Return now if there is no text to analyze
		if (string.IsNullOrEmpty(clearText))
		{
			_NotifyNewMessages.NotifyNewContent(hashtag, content);
			return;
		}

		var request = new AnalyzeTextOptions(clearText);

		Response<AnalyzeTextResult> response;
		try
		{
			response = client.AnalyzeText(request);
		}
		catch (RequestFailedException ex)
		{
			_AzureSafetyLogger.LogError(ex, "Analyze text failed.\nStatus code: {0}, Error code: {1}, Error message: {2}", ex.Status, ex.ErrorCode, ex.Message);
			_NotifyNewMessages.NotifyNewContent(hashtag, content);
			return;
		}

		if (response != null && (response.Value.CategoryResult(TextCategory.Sexual) > 0 || response.Value.CategoryResult(TextCategory.Hate) > 0 || response.Value.CategoryResult(TextCategory.SelfHarm) > 0 || response.Value.CategoryResult(TextCategory.Violence) > 0))
		{

			string reason = "";
			// CategoriesAnalysis.FirstOrDefault(a => a.Category == TextCategory.Hate)
			if (response.Value.CategoryResult(TextCategory.Sexual) > 0) reason += $"Sexual: {response.Value.CategoryResult(TextCategory.Sexual)}. ";
			if (response.Value.CategoryResult(TextCategory.Hate) > 0) reason += $"Hate: {response.Value.CategoryResult(TextCategory.Hate)}. ";
			if (response.Value.CategoryResult(TextCategory.SelfHarm) > 0) reason += $"SelfHarm: {response.Value.CategoryResult(TextCategory.SelfHarm)}. ";
			if (response.Value.CategoryResult(TextCategory.Violence) > 0) reason += $"Violence: {response.Value.CategoryResult(TextCategory.Violence)}. ";

			// Add moderation from Azure Content Safety
			using var scope = _ServiceProvider.CreateScope();
			var moderationRepository = scope.ServiceProvider.GetRequiredService<IModerationRepository>();
			moderationRepository.ModerateWithReason("AZURE-CONTENTSAFETY", content.Provider, content.ProviderId, ModerationState.Rejected, reason).GetAwaiter().GetResult();

			_NotifyNewMessages.NotifyNewContent(hashtag, content);
			_NotifyNewMessages.NotifyRejectedContent(hashtag, content, new ModerationAction
			{
				Provider = content.Provider,
				ProviderId = content.ProviderId,
				State = ModerationState.Rejected,
				Timestamp = DateTimeOffset.UtcNow,
				Moderator = "AZURE-CONTENTSAFETY",
				Reason = reason
			});

		}
		else
		{

			_NotifyNewMessages.NotifyNewContent(hashtag, content);

		}


	}

	public void NotifyNewBlockedCount(int blockedCount)
	{
		// reload the blocked users list
		using IServiceScope scope = ReloadBlockedUserCache();
		_NotifyNewMessages.NotifyNewBlockedCount(blockedCount);

	}

	private IServiceScope ReloadBlockedUserCache()
	{
		var scope = _ServiceProvider.CreateScope();
		var moderationRepository = scope.ServiceProvider.GetRequiredService<IModerationRepository>();
		var blockedUsers = moderationRepository.GetBlockedUsers().GetAwaiter().GetResult();
		_AzureSafetyLogger.LogInformation($"Blocked user count: {blockedUsers.Count()}");
		_Cache.Set(KEY_BLOCKEDUSERS_CACHE, blockedUsers.Select(u => (u.Provider, u.UserName)).ToList());
		return scope;
	}

	/// <summary>
	/// Sanitation code from Stackoverflow:  https://stackoverflow.com/a/19524290
	/// </summary>
	private class HtmlCleaner
	{

		private static readonly Regex _tags_ = new(@"<[^>]+?>", RegexOptions.Multiline | RegexOptions.Compiled);

		//add characters that are should not be removed to this regex
		private static readonly Regex _notOkCharacter_ = new(@"[^\w;&#@.:/\\?=|%!() -]", RegexOptions.Compiled);

		public static string UnHtml(string html)
		{
			html = HttpUtility.UrlDecode(html);
			html = HttpUtility.HtmlDecode(html);

			html = RemoveTag(html, "<!--", "-->");
			html = RemoveTag(html, "<script", "</script>");
			html = RemoveTag(html, "<style", "</style>");

			//replace matches of these regexes with space
			html = _tags_.Replace(html, " ");
			html = _notOkCharacter_.Replace(html, " ");
			html = SingleSpacedTrim(html);

			return html;
		}

		private static string RemoveTag(string html, string startTag, string endTag)
		{
			bool bAgain;
			do
			{
				bAgain = false;
				int startTagPos = html.IndexOf(startTag, 0, StringComparison.CurrentCultureIgnoreCase);
				if (startTagPos < 0)
					continue;
				int endTagPos = html.IndexOf(endTag, startTagPos + 1, StringComparison.CurrentCultureIgnoreCase);
				if (endTagPos <= startTagPos)
					continue;
				html = html.Remove(startTagPos, endTagPos - startTagPos + endTag.Length);
				bAgain = true;
			} while (bAgain);
			return html;
		}

		private static string SingleSpacedTrim(string inString)
		{
			StringBuilder sb = new();
			bool inBlanks = false;
			foreach (char c in inString)
			{
				switch (c)
				{
					case '\r':
					case '\n':
					case '\t':
					case ' ':
						if (!inBlanks)
						{
							inBlanks = true;
							sb.Append(' ');
						}
						continue;
					default:
						inBlanks = false;
						sb.Append(c);
						break;
				}
			}
			return sb.ToString().Trim();
		}
	}

}
