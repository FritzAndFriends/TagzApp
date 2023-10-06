using Azure;
using Azure.AI.ContentSafety;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Text.RegularExpressions;
using System.Text;
using System.Web;
using TagzApp.Web.Services;

namespace TagzApp.Storage.Postgres;

public class AzureSafetyModeration : INotifyNewMessages
{
	private INotifyNewMessages _NotifyNewMessages;
	private readonly IServiceProvider _ServiceProvider;
	private readonly IConfiguration _Configuration;
	private readonly ILogger<AzureSafetyModeration> _AzureSafetyLogger;
	private readonly bool _Enabled;
	private readonly string? _ContentSafetyKey;
	private readonly string? _ContentSafetyEndpoint;

	public AzureSafetyModeration(
		INotifyNewMessages notifyNewMessages,
		IServiceProvider serviceProvider,
		IConfiguration configuration,
		ILogger<AzureSafetyModeration> azureSafetyLogger)
	{

		_NotifyNewMessages = notifyNewMessages;
		_ServiceProvider = serviceProvider;
		_Configuration = configuration;
		_AzureSafetyLogger = azureSafetyLogger;

		_Enabled = _Configuration["AzureContentSafety:Key"] != null && _Configuration.GetValue<bool>("AzureContentSafety:Enabled", true);

		_ContentSafetyKey = _Configuration["AzureContentSafety:Key"];
		_ContentSafetyEndpoint = _Configuration["AzureContentSafety:Endpoint"];

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
		var request = new AnalyzeTextOptions(clearText);

		Response<AnalyzeTextResult> response;
		try
		{
			response = client.AnalyzeText(request);
		}
		catch (RequestFailedException ex)
		{
			_AzureSafetyLogger.LogError(ex, "Analyze text failed.\nStatus code: {0}, Error code: {1}, Error message: {2}", ex.Status, ex.ErrorCode, ex.Message);
			throw;
		}

		if (response != null && (response.Value.SexualResult.Severity > 0 || response.Value.HateResult.Severity > 0 || response.Value.SelfHarmResult.Severity > 0 || response.Value.ViolenceResult.Severity > 0))
		{

			string reason = "";
			if (response.Value.SexualResult.Severity > 0) reason += $"Sexual: {response.Value.SexualResult.Severity}. ";
			if (response.Value.HateResult.Severity > 0) reason += $"Hate: {response.Value.HateResult.Severity}. ";
			if (response.Value.SelfHarmResult.Severity > 0) reason += $"SelfHarm: {response.Value.SelfHarmResult.Severity}. ";
			if (response.Value.ViolenceResult.Severity > 0) reason += $"Violence: {response.Value.ViolenceResult.Severity}. ";

			// Add moderation from Azure Content Safety
			using var scope = _ServiceProvider.CreateScope();
			var moderationRepository = scope.ServiceProvider.GetRequiredService<IModerationRepository>();
			moderationRepository.ModerateWithReason("AZURE-CONTENTSAFETY", content.Provider, content.ProviderId, ModerationState.Rejected, reason).GetAwaiter().GetResult();

			_NotifyNewMessages.NotifyNewContent(hashtag, content);
			_NotifyNewMessages.NotifyRejectedContent(hashtag, content, new ModerationAction {
				Provider = content.Provider,
				ProviderId = content.ProviderId,
				State = ModerationState.Rejected,
				Timestamp = DateTimeOffset.UtcNow,
				Moderator = "AZURE-CONTENTSAFETY",
				Reason = reason
			});

		} else {

			_NotifyNewMessages.NotifyNewContent(hashtag, content);

		}


	}


	/// <summary>
	/// Sanitation code from Stackoverflow:  https://stackoverflow.com/a/19524290
	/// </summary>
	private class HtmlCleaner
	{

		private static readonly Regex _tags_ = new Regex(@"<[^>]+?>", RegexOptions.Multiline | RegexOptions.Compiled);

		//add characters that are should not be removed to this regex
		private static readonly Regex _notOkCharacter_ = new Regex(@"[^\w;&#@.:/\\?=|%!() -]", RegexOptions.Compiled);

		public static String UnHtml(String html)
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

		private static String RemoveTag(String html, String startTag, String endTag)
		{
			Boolean bAgain;
			do
			{
				bAgain = false;
				Int32 startTagPos = html.IndexOf(startTag, 0, StringComparison.CurrentCultureIgnoreCase);
				if (startTagPos < 0)
					continue;
				Int32 endTagPos = html.IndexOf(endTag, startTagPos + 1, StringComparison.CurrentCultureIgnoreCase);
				if (endTagPos <= startTagPos)
					continue;
				html = html.Remove(startTagPos, endTagPos - startTagPos + endTag.Length);
				bAgain = true;
			} while (bAgain);
			return html;
		}

		private static String SingleSpacedTrim(String inString)
		{
			StringBuilder sb = new StringBuilder();
			Boolean inBlanks = false;
			foreach (Char c in inString)
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
