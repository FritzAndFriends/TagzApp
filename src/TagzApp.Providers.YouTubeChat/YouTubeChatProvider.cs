﻿namespace TagzApp.Providers.YouTubeChat;

public class YouTubeChatProvider : ISocialMediaProvider
{
	public string Id => "YOUTUBE-CHAT";
	public string DisplayName => "YouTube Chat";
	public TimeSpan NewContentRetrievalFrequency { get; }

	public Task<IEnumerable<Content>> GetContentForHashtag(Hashtag tag, DateTimeOffset since)
	{
		throw new NotImplementedException();
	}

}
