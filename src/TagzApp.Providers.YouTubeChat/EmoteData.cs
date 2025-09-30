namespace TagzApp.Providers.YouTubeChat;

public class EmoteData
{
	public string EmojiId { get; set; } = string.Empty;
	public EmoteImage Image { get; set; } = new();
	public string[] SearchTerms { get; set; } = Array.Empty<string>();
	public string[] Shortcuts { get; set; } = Array.Empty<string>();
}

public class EmoteImage
{
	public EmoteThumbnail[] Thumbnails { get; set; } = Array.Empty<EmoteThumbnail>();
}

public class EmoteThumbnail
{
	public string Url { get; set; } = string.Empty;
}