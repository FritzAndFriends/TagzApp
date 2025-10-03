using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Net.Http.Json;
using System.Text.RegularExpressions;

namespace TagzApp.Providers.YouTubeChat;

public class YouTubeEmoteTranslator
{

	private static readonly List<EmoteData> _emotes = new();

	private static FrozenSet<string> _emoteShortcuts = FrozenSet.Create<string>();

	// Matches either a run of entirely non-alphanumeric characters or a token that
	// starts and ends with a colon with alphanumeric characters or the underscore character
	// between, ensuring
	// the token is bounded by start/whitespace and whitespace/end so we can get
	// accurate positions in the original text via the capture group.
	private static readonly Regex s_emoteCandidateRegex = new(@"(?:^|\s)([^A-Za-z0-9]+|:[A-Za-z0-9_]+:)(?=\s|$)", RegexOptions.Compiled);

	public static async Task LoadEmotes(HttpClient httpClient, short maxValue = 10)
	{

		if (_emotes.Any()) return;

		for (short i = 0; i <= maxValue; i++)
		{

			try
			{

				var emotes = await httpClient.GetFromJsonAsync<List<EmoteData>>($"https://www.gstatic.com/youtube/img/emojis/emojis-svg-{i}.json");
				if (emotes is not null && emotes.Any()) _emotes.AddRange(emotes);
			}
			catch (Exception)
			{
				// Ignore errors, just continue to the next page.
				continue;
			}
		}

		_emoteShortcuts = _emotes.SelectMany(e => e.Shortcuts).ToFrozenSet(StringComparer.InvariantCultureIgnoreCase);

	}

	// Public overload so callers that have a string can easily call into the span-based API
	public static bool TryIdentifyEmotes(string text, out List<Emote> emotes)
	{

		emotes = new List<Emote>();

		if (_emoteShortcuts is null || !_emoteShortcuts.Any()) return false;

		var textStr = text.ToString();

		foreach (Match m in s_emoteCandidateRegex.Matches(textStr))
		{
			var g = m.Groups[1];
			var token = g.Value;

			// Check membership in the frozen set (case-insensitive as it was created)
			if (_emoteShortcuts.Contains(token))
			{
				// Create Emote record with the position and length from the capture group
				var imageUrl = TranslateEmote(token);
				emotes.Add(new Emote(g.Index, g.Length, imageUrl));
			}
		}

		return emotes.Any();

	}

	public static string TranslateEmote(string emojiId)
	{

		return _emotes.FirstOrDefault(
			e => e.Shortcuts.Contains(emojiId, StringComparer.InvariantCultureIgnoreCase)
		)?.Image.Thumbnails.FirstOrDefault()?.Url ?? emojiId;

	}
}
