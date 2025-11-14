using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Microsoft.Extensions.Logging;
using TagzApp.Common.Telemetry;
using TagzApp.Providers.Youtube.Configuration;

namespace TagzApp.Providers.Youtube;

internal class YoutubeProvider : ISocialMediaProvider
{
	private readonly YoutubeConfiguration _Configuration;
	private readonly ILogger<YoutubeProvider>? _Logger;
	private readonly ProviderInstrumentation? _Instrumentation;

	public string Id => "YOUTUBE";
	public string DisplayName => "Youtube";
	public string Description { get; init; }

	private SocialMediaStatus _Status = SocialMediaStatus.Unhealthy;
	private string _StatusMessage = "Not started";

	public TimeSpan NewContentRetrievalFrequency => TimeSpan.FromSeconds(30);

	public bool Enabled { get; }

	public YoutubeProvider(YoutubeConfiguration options, ILogger<YoutubeProvider>? logger = null, ProviderInstrumentation? instrumentation = null)
	{
		_Configuration = options;
		_Logger = logger;
		_Instrumentation = instrumentation;
		Enabled = options.Enabled;
	}

	public async Task<IEnumerable<Content>> GetContentForHashtag(Hashtag tag, DateTimeOffset since)
	{
		var youtubeService = new YouTubeService(new BaseClientService.Initializer()
		{
			ApiKey = _Configuration.ApiKey,
			ApplicationName = "TagzApp"
		});

		var searchListRequest = youtubeService.Search.List("snippet");
		searchListRequest.Q = tag.Text;
		searchListRequest.Order = SearchResource.ListRequest.OrderEnum.Relevance;
		searchListRequest.SafeSearch = _Configuration.SafeSearch;
		searchListRequest.MaxResults = _Configuration.MaxResults;

		var searchListResponse = await searchListRequest.ExecuteAsync();

		_Status = SocialMediaStatus.Healthy;
		_StatusMessage = "OK";
		_Instrumentation?.RecordConnectionStatusChange(Id, SocialMediaStatus.Healthy);

		if (searchListResponse.Items == null || (!searchListResponse.Items?.Any() ?? true))
		{
			return Enumerable.Empty<Content>();
		}

		var content = searchListResponse.Items!.Select(m => new Content
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

		if (_Instrumentation is not null && content.Any())
		{
			_Logger?.LogInformation("YouTube: Retrieved {Count} new videos", content.Count());
			foreach (var video in content)
			{
				if (!string.IsNullOrEmpty(video.Author?.UserName))
				{
					_Instrumentation.AddMessage(Id.ToLowerInvariant(), video.Author.UserName);
				}
			}
		}

		return content;
	}

	public Task StartAsync()
	{
		if (Enabled)
		{
			_Logger?.LogInformation("YouTube: Provider started");
			_Instrumentation?.RecordConnectionStatusChange(Id, SocialMediaStatus.Healthy);
		}
		else
		{
			_Logger?.LogInformation("YouTube: Provider is disabled");
			_Instrumentation?.RecordConnectionStatusChange(Id, SocialMediaStatus.Disabled);
		}
		return Task.CompletedTask;
	}

	public Task<(SocialMediaStatus Status, string Message)> GetHealth() => Task.FromResult((_Status, _StatusMessage));

	public Task StopAsync()
	{
		_Logger?.LogInformation("YouTube: Provider stopped");
		_Instrumentation?.RecordConnectionStatusChange(Id, SocialMediaStatus.Disabled);
		return Task.CompletedTask;
	}

	public void Dispose()
	{
		// do nothing
	}

	public async Task<IProviderConfiguration> GetConfiguration(IConfigureTagzApp configure)
	{
		return await configure.GetConfigurationById<YoutubeConfiguration>(Id);
	}

	public async Task SaveConfiguration(IConfigureTagzApp configure, IProviderConfiguration providerConfiguration)
	{
		await configure.SetConfigurationById(Id, (YoutubeConfiguration)providerConfiguration);
	}
}
