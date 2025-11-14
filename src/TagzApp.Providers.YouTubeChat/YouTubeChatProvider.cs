using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TagzApp.Common.Telemetry;

namespace TagzApp.Providers.YouTubeChat;

public class YouTubeChatProvider : ISocialMediaProvider, IDisposable
{
	private readonly YouTubeChatConfiguration _ChatConfig;
	public const string ProviderName = "YouTubeChat";

	public const string ProviderId = "YOUTUBE-CHAT";

	public string Id => ProviderId;
	public string DisplayName => ProviderName;
	public string Description { get; init; }
	public TimeSpan NewContentRetrievalFrequency { get; set; } = TimeSpan.FromSeconds(15);

	public string NewestId { get; set; }
	public string RefreshToken { get; set; }
	public string YouTubeEmailId { get; set; }
	public bool Enabled { get; }

	private readonly HttpClient _HttpClient;
	private readonly ILogger<YouTubeChatProvider> _Logger;
	private readonly ProviderInstrumentation? _Instrumentation;
	private string _GoogleException = string.Empty;

	private CancellationTokenSource _TokenSource = new();
	private YouTubeService _Service;
	private bool _DisposedValue;
	private string _NextPageToken;

	private SocialMediaStatus _Status = SocialMediaStatus.Unhealthy;
	private string _StatusMessage = "Not started";

	// YouTube API quota tracking
	private long _QuotaUsed = 0;
	private const int QUOTA_LIVECHAT_LIST = 5;
	private const int QUOTA_SEARCH_LIST = 100;
	private const int QUOTA_VIDEO_LIST = 1;

	public YouTubeChatProvider(YouTubeChatConfiguration config, IConfiguration configuration, HttpClient httpClient, ILogger<YouTubeChatProvider> logger, ProviderInstrumentation? instrumentation = null)
	{
		_ChatConfig = config;
		Enabled = true; // config.Enabled;
		_HttpClient = httpClient;
		_Logger = logger;
		_Instrumentation = instrumentation;

	}

	public async Task<IEnumerable<Content>> GetContentForHashtag(Hashtag tag, DateTimeOffset since)
	{

		if (string.IsNullOrEmpty(_ChatConfig.LiveChatId) || (!string.IsNullOrEmpty(_GoogleException) && _GoogleException.StartsWith(_ChatConfig.LiveChatId))) return Enumerable.Empty<Content>();
		var liveChatListRequest = new LiveChatMessagesResource.ListRequest(_Service, _ChatConfig.LiveChatId, new(new[] { "id", "snippet", "authorDetails" }));
		liveChatListRequest.MaxResults = 2000;
		liveChatListRequest.ProfileImageSize = 36;

		if (!string.IsNullOrEmpty(_NextPageToken)) liveChatListRequest.PageToken = _NextPageToken;

		LiveChatMessageListResponse contents;
		try
		{
			contents = await liveChatListRequest.ExecuteAsync();
			_NextPageToken = contents.NextPageToken;
			NewContentRetrievalFrequency = contents.PollingIntervalMillis.HasValue ? TimeSpan.FromMilliseconds(contents.PollingIntervalMillis.Value * 10) : TimeSpan.FromSeconds(6);

			// Track quota usage for LiveChatMessages.list API call
			_QuotaUsed += QUOTA_LIVECHAT_LIST;
			_Logger.LogInformation("YouTube API call: LiveChatMessages.list - Quota cost: {QuotaCost}, Total quota used: {QuotaUsed}, Messages retrieved: {MessageCount}",
				QUOTA_LIVECHAT_LIST, _QuotaUsed, contents.Items.Count);

			// Emit telemetry for API usage
			_Instrumentation?.MessagesReceivedCounter.Add(QUOTA_LIVECHAT_LIST,
				new KeyValuePair<string, object?>("provider", Id),
				new KeyValuePair<string, object?>("api_call", "LiveChatMessages.list"),
				new KeyValuePair<string, object?>("quota_used", _QuotaUsed));

		}
		catch (Exception ex)
		{
			_Logger.LogError(ex, "Exception while fetching YouTubeChat: {Message}", ex.Message);
			if (ex.Message.Contains("live chat is no longer live"))
			{
				_GoogleException = $"{_ChatConfig.LiveChatId}:{ex.Message}";
				_ChatConfig.LiveChatId = string.Empty;
			}

			_Status = SocialMediaStatus.Unhealthy;
			_StatusMessage = $"Exception while fetching YouTubeChat: {ex.Message}";

			return Enumerable.Empty<Content>();
		}

		_Status = SocialMediaStatus.Healthy;
		_StatusMessage = $"OK -- adding ({contents.Items.Count}) messages for chatid '{_ChatConfig.LiveChatId}' at {DateTimeOffset.UtcNow}";

		try
		{
			var outItems = contents.Items.Select(i => new Content
			{
				Author = new Creator
				{
					DisplayName = i.AuthorDetails.DisplayName,
					ProfileImageUri = new Uri(i.AuthorDetails.ProfileImageUrl),
					ProfileUri = new Uri($"https://www.youtube.com/channel/{i.AuthorDetails.ChannelId}")
				},
				Provider = Id,
				ProviderId = i.Id,
				Text = string.IsNullOrEmpty(i.Snippet.DisplayMessage) ? "- REMOVED MESSAGE -" : i.Snippet.DisplayMessage,
				SourceUri = new Uri($"https://youtube.com/livechat/{_ChatConfig.LiveChatId}"),
				Timestamp = DateTimeOffset.Parse(i.Snippet.PublishedAtRaw),
				Type = ContentType.Message,
				HashtagSought = tag?.Text ?? ""
			}).ToArray();

			// Track message authors in instrumentation
			if (_Instrumentation is not null)
			{
				foreach (var item in contents.Items)
				{
					if (!string.IsNullOrEmpty(item.AuthorDetails.DisplayName))
					{
						_Instrumentation.AddMessage("youtubechat", item.AuthorDetails.DisplayName);
					}
				}
			}

			return outItems;

		}
		catch (Exception ex)
		{

			_Logger.LogError(ex, "Exception while parsing YouTubeChat: {Message}", ex.Message);

			_Status = SocialMediaStatus.Unhealthy;
			_StatusMessage = $"Exception while parsing YouTubeChat: {ex.Message}";

			return Enumerable.Empty<Content>();

		}


	}

	private async Task<YouTubeService> GetGoogleService()
	{

		if (_Service is not null) return _Service;

		_Service = new YouTubeService(new BaseClientService.Initializer
		{
			ApiKey = _ChatConfig.YouTubeApiKey,
			ApplicationName = "TagzApp"
		});

		_Status = SocialMediaStatus.Degraded;
		_StatusMessage = "Starting YouTubeChat client";

		return _Service;
	}


	public async Task StartAsync()
	{

		// if (string.IsNullOrEmpty(LiveChatId) || string.IsNullOrEmpty(RefreshToken)) return;

		_Service = await GetGoogleService();
		await YouTubeEmoteTranslator.LoadEmotes(_HttpClient, 10);

		if (!_ChatConfig.Enabled)
		{
			_Status = SocialMediaStatus.Disabled;
			_StatusMessage = "YouTubeChat client is disabled";
			return;
		}

	}

	public async Task<string> GetChannelForUserAsync()
	{

		var service = await GetGoogleService();

		var channelRequest = service.Search.List("snippet");
		//channelRequest.Mine = true;
		channelRequest.ChannelId = _ChatConfig.ChannelId;
		var channels = channelRequest.Execute();

		// Track quota usage for Search.list API call
		_QuotaUsed += QUOTA_SEARCH_LIST;
		_Logger.LogInformation("YouTube API call: Search.list (channel) - Quota cost: {QuotaCost}, Total quota used: {QuotaUsed}",
			QUOTA_SEARCH_LIST, _QuotaUsed);

		_Instrumentation?.MessagesReceivedCounter.Add(QUOTA_SEARCH_LIST,
			new KeyValuePair<string, object?>("provider", Id),
			new KeyValuePair<string, object?>("api_call", "Search.list"),
			new KeyValuePair<string, object?>("quota_used", _QuotaUsed));

		// Not sure if this is needed, can't replicate "fisrt" error. (https://github.com/FritzAndFriends/TagzApp/issues/241)
		return channels.Items?.First().Snippet.Title ?? "Unknown Channel Title";

	}

	public IEnumerable<YouTubeBroadcast> GetBroadcastsForUser()
	{
		return GetBroadcastsForUser(_ChatConfig.YouTubeApiKey, _ChatConfig.ChannelId);
	}

	public IEnumerable<YouTubeBroadcast> GetBroadcastsForUser(string youTubeApiKey, string channelId)
	{
		if (string.IsNullOrEmpty(youTubeApiKey) || string.IsNullOrEmpty(channelId))
		{
			return Enumerable.Empty<YouTubeBroadcast>();
		}

		var service = new YouTubeService(new BaseClientService.Initializer
		{
			ApiKey = youTubeApiKey,
			ApplicationName = "TagzApp"
		});

		var listRequest = service.Search.List("snippet");
		//listRequest.Q = searchString;
		listRequest.ChannelId = channelId;
		listRequest.EventType = SearchResource.ListRequest.EventTypeEnum.Upcoming;
		listRequest.Type = "video";
		listRequest.MaxResults = 500;
		listRequest.Order = SearchResource.ListRequest.OrderEnum.Date;
		SearchListResponse broadcasts;
		try
		{
			broadcasts = listRequest.Execute();

			// Track quota usage for Search.list API call
			_QuotaUsed += QUOTA_SEARCH_LIST;
			_Logger.LogInformation("YouTube API call: Search.list (Upcoming) - Quota cost: {QuotaCost}, Total quota used: {QuotaUsed}, Results: {ResultCount}",
				QUOTA_SEARCH_LIST, _QuotaUsed, broadcasts.Items?.Count ?? 0);

			_Instrumentation?.MessagesReceivedCounter.Add(QUOTA_SEARCH_LIST,
				new KeyValuePair<string, object?>("provider", Id),
				new KeyValuePair<string, object?>("api_call", "Search.list"),
				new KeyValuePair<string, object?>("quota_used", _QuotaUsed));
		}
		catch (Google.GoogleApiException ex)
		{
			// GoogleApiException: The service youtube has thrown an exception. HttpStatusCode is Forbidden. The user is not enabled for live streaming.
			_Logger.LogError(ex, "Exception while fetching YouTube broadcasts: {Message}", ex.Message);

			_Status = SocialMediaStatus.Unhealthy;
			_StatusMessage = $"Exception while fetching YouTube broadcasts: {ex.Message}";

			return Enumerable.Empty<YouTubeBroadcast>();
		}

		var outBroadcasts = new List<YouTubeBroadcast>();

		broadcasts = ConvertToYouTubeBroadcasts(service, listRequest, broadcasts, outBroadcasts);

		listRequest = service.Search.List("snippet");
		//listRequest.Q = searchString;
		listRequest.ChannelId = channelId;
		listRequest.EventType = SearchResource.ListRequest.EventTypeEnum.Live;
		listRequest.Type = "video";
		listRequest.MaxResults = 5;
		listRequest.Order = SearchResource.ListRequest.OrderEnum.Date;
		try
		{
			broadcasts = listRequest.Execute();

			// Track quota usage for Search.list API call
			_QuotaUsed += QUOTA_SEARCH_LIST;
			_Logger.LogInformation("YouTube API call: Search.list (Live) - Quota cost: {QuotaCost}, Total quota used: {QuotaUsed}, Results: {ResultCount}",
				QUOTA_SEARCH_LIST, _QuotaUsed, broadcasts.Items?.Count ?? 0);

			_Instrumentation?.MessagesReceivedCounter.Add(QUOTA_SEARCH_LIST,
				new KeyValuePair<string, object?>("provider", Id),
				new KeyValuePair<string, object?>("api_call", "Search.list"),
				new KeyValuePair<string, object?>("quota_used", _QuotaUsed));
		}
		catch (Google.GoogleApiException ex)
		{
			// GoogleApiException: The service youtube has thrown an exception. HttpStatusCode is Forbidden. The user is not enabled for live streaming.
			_Logger.LogError(ex, "Exception while fetching YouTube broadcasts: {Message}", ex.Message);

			_Status = SocialMediaStatus.Unhealthy;
			_StatusMessage = $"Exception while fetching YouTube broadcasts: {ex.Message}";

			return Enumerable.Empty<YouTubeBroadcast>();
		}
		ConvertToYouTubeBroadcasts(service, listRequest, broadcasts, outBroadcasts);


		return outBroadcasts.OrderBy(b => b.BroadcastTime);

	}

	private SearchListResponse ConvertToYouTubeBroadcasts(YouTubeService service, SearchResource.ListRequest listRequest, SearchListResponse broadcasts, List<YouTubeBroadcast> outBroadcasts)
	{
		var first = true;
		while (first || !string.IsNullOrEmpty(broadcasts.NextPageToken))// && outBroadcasts.Count < 20)
		{

			if (first)
			{
				first = false;
			}
			else
			{
				listRequest.PageToken = broadcasts.NextPageToken;
				broadcasts = listRequest.Execute();

				// Track quota for paginated Search.list calls
				_QuotaUsed += QUOTA_SEARCH_LIST;
				_Logger.LogInformation("YouTube API call: Search.list (pagination) - Quota cost: {QuotaCost}, Total quota used: {QuotaUsed}",
					QUOTA_SEARCH_LIST, _QuotaUsed);

				_Instrumentation?.MessagesReceivedCounter.Add(QUOTA_SEARCH_LIST,
					new KeyValuePair<string, object?>("provider", Id),
					new KeyValuePair<string, object?>("api_call", "Search.list"),
					new KeyValuePair<string, object?>("quota_used", _QuotaUsed));
			}

			foreach (var broadcast in broadcasts.Items)
			{

				var videoRequest = service.Videos.List("liveStreamingDetails");
				videoRequest.Id = broadcast.Id.VideoId;
				var videoResponse = videoRequest.Execute();

				// Track quota for Videos.list calls
				_QuotaUsed += QUOTA_VIDEO_LIST;
				_Logger.LogDebug("YouTube API call: Videos.list - Quota cost: {QuotaCost}, Total quota used: {QuotaUsed}, VideoId: {VideoId}",
					QUOTA_VIDEO_LIST, _QuotaUsed, broadcast.Id.VideoId);

				_Instrumentation?.MessagesReceivedCounter.Add(QUOTA_VIDEO_LIST,
					new KeyValuePair<string, object?>("provider", Id),
					new KeyValuePair<string, object?>("api_call", "Videos.list"),
					new KeyValuePair<string, object?>("quota_used", _QuotaUsed));

				if (videoResponse.Items.First().LiveStreamingDetails is null) continue;

				var liveChatId = videoResponse.Items.First().LiveStreamingDetails.ActiveLiveChatId;
				if (string.IsNullOrEmpty(liveChatId)) continue;

				outBroadcasts.Add(
					new YouTubeBroadcast(
						broadcast.Id.VideoId,
						broadcast.Snippet.Title,
						videoResponse.Items.First().LiveStreamingDetails.ScheduledStartTimeDateTimeOffset,
						liveChatId
					));

			}


		}

		return broadcasts;
	}

	#region Dispose Pattern

	protected virtual void Dispose(bool disposing)
	{
		if (!_DisposedValue)
		{
			if (disposing)
			{
				_Service.Dispose();
				_TokenSource.Cancel();
			}

			// TODO: free unmanaged resources (unmanaged objects) and override finalizer
			// TODO: set large fields to null
			_DisposedValue = true;
		}
	}

	// // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
	// ~YouTubeChatProvider()
	// {
	//     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
	//     Dispose(disposing: false);
	// }

	public void Dispose()
	{
		// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	public Task<(SocialMediaStatus Status, string Message)> GetHealth()
	{
		var message = $"{_StatusMessage} | API Quota Used: {_QuotaUsed} units";
		return Task.FromResult((_Status, message));
	}

	public Task StopAsync()
	{
		return Task.CompletedTask;
	}

	public async Task<IProviderConfiguration> GetConfiguration(IConfigureTagzApp configure)
	{
		return await configure.GetConfigurationById<YouTubeChatConfiguration>(Id);
	}

	public async Task SaveConfiguration(IConfigureTagzApp configure, IProviderConfiguration providerConfiguration)
	{
		await configure.SetConfigurationById(Id, (YouTubeChatConfiguration)providerConfiguration);
	}

	#endregion

}


