using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace TagzApp.Storage.Postgres.SafetyModeration;

public class WordFilterModeration : INotifyNewMessages
{
	private const string _KeyBlockedUsersCache = "blockedUsers";
	private readonly IMemoryCache _Cache;
	private INotifyNewMessages _NotifyNewMessages;
	private readonly IServiceProvider _ServiceProvider;
	private readonly ILogger<WordFilterModeration> _WordFilterLogger;
	private readonly bool _Enabled;
	private readonly string[] _BlockedWords;

	public WordFilterModeration(
		IMemoryCache cache,
		INotifyNewMessages notifyNewMessages,
		IServiceProvider serviceProvider,
		IConfigureTagzApp configureTagzApp,
		ILogger<WordFilterModeration> wordFilterLogger)
	{
		_Cache = cache;
		_NotifyNewMessages = notifyNewMessages;
		_ServiceProvider = serviceProvider;
		_WordFilterLogger = wordFilterLogger;

		var siteConfig = serviceProvider.GetRequiredService<IConfiguration>();

		var config = //configureTagzApp.GetConfigurationById<WordFilterConfiguration>(WordFilterConfiguration.ConfigurationKey).GetAwaiter().GetResult();
			new WordFilterConfiguration
			{
				Enabled = siteConfig.GetValue<bool>("wordfilter_enabled", false),
				BlockedWords = siteConfig.GetSection("wordfilter_blockedwords").Get<string[]>() ?? Array.Empty<string>()
			};

		_Enabled = config.Enabled;
		_BlockedWords = config.BlockedWords;

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
		// Check if this content is created by one of the blocked users listed in the cache
		var usersBlocked = _Cache.GetOrCreate(_KeyBlockedUsersCache, _ => new List<(string Provider, string UserName, BlockedUserCapabilities Capabilities)>());
		var isBlocked = usersBlocked
			.FirstOrDefault(a => a.Provider.Equals(content.Provider, StringComparison.InvariantCultureIgnoreCase)
				&& a.UserName.Equals('@' + content.Author.UserName.TrimStart('@'), StringComparison.InvariantCultureIgnoreCase));

		if (!string.IsNullOrEmpty(isBlocked.Provider))
		{
			using var scope = _ServiceProvider.CreateScope();
			var moderationRepository = scope.ServiceProvider.GetRequiredService<IModerationRepository>();
			moderationRepository.ModerateWithReason("BLOCKED-USER", content.Provider, content.ProviderId, ModerationState.Rejected, "Blocked User").GetAwaiter().GetResult();

			if (isBlocked.Capabilities != BlockedUserCapabilities.Hidden)
			{
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
			}
			return;
		}

		if (!_Enabled || _BlockedWords.Length == 0)
		{
			_NotifyNewMessages.NotifyNewContent(hashtag, content);
			return;
		}

		// Check if the content contains any blocked words or phrases
		var clearText = HtmlCleaner.UnHtml(content.Text);

		// Return now if there is no text to analyze
		if (string.IsNullOrEmpty(clearText))
		{
			_NotifyNewMessages.NotifyNewContent(hashtag, content);
			return;
		}

		var blockedWordsFound = CheckForBlockedWords(clearText);

		if (blockedWordsFound.Any())
		{
			string reason = $"Contains blocked words/phrases: {string.Join(", ", blockedWordsFound)}";

			// Add moderation from Word Filter
			using var scope = _ServiceProvider.CreateScope();
			var moderationRepository = scope.ServiceProvider.GetRequiredService<IModerationRepository>();
			moderationRepository.ModerateWithReason("WORD-FILTER", content.Provider, content.ProviderId, ModerationState.Rejected, reason).GetAwaiter().GetResult();

			_NotifyNewMessages.NotifyNewContent(hashtag, content);
			_NotifyNewMessages.NotifyRejectedContent(hashtag, content, new ModerationAction
			{
				Provider = content.Provider,
				ProviderId = content.ProviderId,
				State = ModerationState.Rejected,
				Timestamp = DateTimeOffset.UtcNow,
				Moderator = "WORD-FILTER",
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
		_WordFilterLogger.LogInformation($"Blocked user count: {blockedUsers.Count()}");
		(moderationRepository as PostgresModerationRepository)?.UpdateBlockedUsersCache(blockedUsers);
		return scope;
	}

	private List<string> CheckForBlockedWords(string text)
	{
		var foundWords = new List<string>();

		foreach (var blockedWord in _BlockedWords)
		{
			if (string.IsNullOrWhiteSpace(blockedWord)) continue;

			// Check if the blocked word/phrase exists in the text (case-insensitive)
			// Use word boundaries for single words, but allow phrase matching for multi-word entries
			if (blockedWord.Contains(' '))
			{
				// For phrases, do a simple case-insensitive contains check
				if (text.Contains(blockedWord, StringComparison.InvariantCultureIgnoreCase))
				{
					foundWords.Add(blockedWord);
				}
			}
			else
			{
				// For single words, use regex with word boundaries to avoid partial matches
				var pattern = $@"\b{Regex.Escape(blockedWord)}\b";
				if (Regex.IsMatch(text, pattern, RegexOptions.IgnoreCase))
				{
					foundWords.Add(blockedWord);
				}
			}
		}

		return foundWords;
	}

	/// <summary>
	/// Sanitation code from AzureSafetyModeration - reused for consistency
	/// </summary>
	private class HtmlCleaner
	{
		private static readonly Regex _tags_ = new(@"<[^>]+?>", RegexOptions.Multiline | RegexOptions.Compiled);

		//add characters that are should not be removed to this regex
		private static readonly Regex _notOkCharacter_ = new(@"[^\w;&#@.:/\\?=|%!() -]", RegexOptions.Compiled);

		public static string UnHtml(string html)
		{
			html = System.Web.HttpUtility.UrlDecode(html);
			html = System.Web.HttpUtility.HtmlDecode(html);

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
			var sb = new System.Text.StringBuilder();
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