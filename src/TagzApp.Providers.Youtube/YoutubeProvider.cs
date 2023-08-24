using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Microsoft.Extensions.Options;
using TagzApp.Providers.Youtube.Configuration;

namespace TagzApp.Providers.Youtube;

internal class YoutubeProvider : ISocialMediaProvider
{
	private readonly YoutubeConfiguration _Configuration;

	public string Id => "YOUTUBE";
	public string DisplayName => "Youtube";

	public TimeSpan NewContentRetrievalFrequency => TimeSpan.FromSeconds(30);

	public YoutubeProvider(IOptions<YoutubeConfiguration> options)
	{
		_Configuration = options.Value;
	}

	public async Task<IEnumerable<Content>> GetContentForHashtag(Hashtag tag, DateTimeOffset since)
	{
		var youtubeService = new YouTubeService(new BaseClientService.Initializer()
		{
			ApiKey = _Configuration.ApiKey,
			ApplicationName = "MyTagg"
		});

		var searchListRequest = youtubeService.Search.List("snippet");
		searchListRequest.Q = tag.Text;
		searchListRequest.Order = SearchResource.ListRequest.OrderEnum.Relevance;
		searchListRequest.SafeSearch = _Configuration.SafeSearch;
		searchListRequest.MaxResults = _Configuration.MaxResults;

		var searchListResponse = await searchListRequest.ExecuteAsync();

		if (searchListResponse.Items == null || (!searchListResponse.Items?.Any() ?? true))
		{
			return Enumerable.Empty<Content>();
		}

		return searchListResponse.Items!.Select(m => new Content
		{
			Provider = Id,
			ProviderId = m.Id.VideoId, // TODO: Validate this is what we want here
			Type = ContentType.Message,
			Timestamp = m.Snippet.PublishedAtDateTimeOffset ?? DateTimeOffset.Now,
			SourceUri = new Uri($"https://www.youtube.com/watch?v={m.Id.VideoId}"),
			Author = new Creator
			{
				DisplayName = m.Snippet.ChannelTitle,
				UserName = m.Snippet.ChannelTitle,
				ProviderId = Id,
				ProfileImageUri = new Uri(m.Snippet.Thumbnails.Default__.Url),
				ProfileUri = new Uri($"https://www.youtube.com/channel/{m.Snippet.ChannelId}")
			},
			Text = m.Snippet.Title
		});
	}
}
