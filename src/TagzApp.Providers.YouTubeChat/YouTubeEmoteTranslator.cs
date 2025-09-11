using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Net.Http.Json;

namespace TagzApp.Providers.YouTubeChat;

public class YouTubeEmoteTranslator
{

	private static readonly List<EmoteData> _emotes = new();

	private static FrozenSet<string> _emoteShortcuts = FrozenSet.Create<string>();

	public static async Task LoadEmotes(HttpClient httpClient, short maxValue = 10)
	{

		if (_emotes.Any()) return;

		for (short i = 0; i <= maxValue; i++)
		{

			var emotes = await httpClient.GetFromJsonAsync<List<EmoteData>>($"https://www.gstatic.com/youtube/img/emojis/emojis-svg-{i}.json");
			if (emotes is not null && emotes.Any()) _emotes.AddRange(emotes);

		}

		_emoteShortcuts = _emotes.SelectMany(e => e.Shortcuts).ToFrozenSet(StringComparer.InvariantCultureIgnoreCase);

	}

	internal static bool TryIdentifyEmotes(ReadOnlySpan<char> text, out List<Emote> emotes)
	{

		emotes = new List<Emote>();

		foreach (var shortcut in _emoteShortcuts)
		{

			if (text.Contains(shortcut, StringComparison.InvariantCultureIgnoreCase))
			{

				// var newEmote = new Emote()

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
