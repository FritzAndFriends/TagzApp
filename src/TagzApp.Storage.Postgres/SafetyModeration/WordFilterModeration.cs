using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;
using TagzApp.Common.Models;

namespace TagzApp.Storage.Postgres.SafetyModeration;

public class WordFilterModeration : INotifyNewMessages
{
	private INotifyNewMessages _NotifyNewMessages;
	private readonly IServiceProvider _ServiceProvider;
	private readonly ILogger<WordFilterModeration> _WordFilterLogger;
	private readonly object _ConfigLock = new object();
	private bool _Enabled;
	private string[] _BlockedWords;

	public WordFilterModeration(
		INotifyNewMessages notifyNewMessages,
		IServiceProvider serviceProvider,
		IConfigureTagzApp configureTagzApp,
		ILogger<WordFilterModeration> wordFilterLogger)
	{
		_NotifyNewMessages = notifyNewMessages;
		_ServiceProvider = serviceProvider;
		_WordFilterLogger = wordFilterLogger;

		// Subscribe to configuration changes
		WordFilterConfiguration.ConfigurationChanged += OnConfigurationChanged;

		// Load initial configuration
		LoadConfiguration(configureTagzApp).GetAwaiter().GetResult();

		_WordFilterLogger.LogInformation("WordFilter moderation initialized - Enabled: {Enabled}, BlockedWords Count: {Count}", 
			_Enabled, _BlockedWords.Length);
	}

	private async Task LoadConfiguration(IConfigureTagzApp configureTagzApp)
	{
		try
		{
			var config = await configureTagzApp.GetConfigurationById<WordFilterConfiguration>(WordFilterConfiguration.ConfigurationKey);
			
			lock (_ConfigLock)
			{
				_Enabled = config.Enabled;
				_BlockedWords = config.BlockedWords ?? Array.Empty<string>();
			}

			_WordFilterLogger.LogInformation("WordFilter configuration loaded - Enabled: {Enabled}, BlockedWords Count: {Count}", 
				_Enabled, _BlockedWords.Length);
		}
		catch (Exception ex)
		{
			_WordFilterLogger.LogError(ex, "Failed to load WordFilter configuration - disabling word filtering");
			lock (_ConfigLock)
			{
				_Enabled = false;
				_BlockedWords = Array.Empty<string>();
			}
		}
	}

	private void OnConfigurationChanged(object? sender, WordFilterConfigurationChangedEventArgs e)
	{
		lock (_ConfigLock)
		{
			_Enabled = e.Configuration.Enabled;
			_BlockedWords = e.Configuration.BlockedWords ?? Array.Empty<string>();
		}

		_WordFilterLogger.LogInformation("WordFilter configuration updated via event - Enabled: {Enabled}, BlockedWords Count: {Count}", 
			_Enabled, _BlockedWords.Length);
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
		bool enabled;
		string[] blockedWords;

		// Get current configuration with thread safety
		lock (_ConfigLock)
		{
			enabled = _Enabled;
			blockedWords = _BlockedWords;
		}

		if (!enabled || blockedWords.Length == 0)
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

		var blockedWordsFound = CheckForBlockedWords(clearText, blockedWords);

		if (blockedWordsFound.Any())
		{
			string reason = $"Contains blocked words/phrases: {string.Join(", ", blockedWordsFound)}";

			_WordFilterLogger.LogInformation("Message from {Provider} rejected due to blocked words: {Words}", 
				content.Provider, string.Join(", ", blockedWordsFound));

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
		_NotifyNewMessages.NotifyNewBlockedCount(blockedCount);
	}

	private List<string> CheckForBlockedWords(string text, string[] blockedWords)
	{
		var foundWords = new List<string>();

		foreach (var blockedWord in blockedWords)
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
		private static readonly Regex _Tags = new(@"<[^>]+?>", RegexOptions.Multiline | RegexOptions.Compiled);

		//add characters that are should not be removed to this regex
		private static readonly Regex _NotOkCharacter = new(@"[^\w;&#@.:/\\?=|%!() -]", RegexOptions.Compiled);

		public static string UnHtml(string html)
		{
			html = System.Web.HttpUtility.UrlDecode(html);
			html = System.Web.HttpUtility.HtmlDecode(html);

			html = RemoveTag(html, "<!--", "-->");
			html = RemoveTag(html, "<script", "</script>");
			html = RemoveTag(html, "<style", "</style>");

			//replace matches of these regexes with space
			html = _Tags.Replace(html, " ");
			html = _NotOkCharacter.Replace(html, " ");
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