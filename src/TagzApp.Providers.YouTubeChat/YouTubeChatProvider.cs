using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using Microsoft.Extensions.Configuration;

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
	private string _GoogleException = string.Empty;

	private CancellationTokenSource _TokenSource = new();
	private YouTubeService _Service;
	private bool _DisposedValue;
	private string _NextPageToken;

	private SocialMediaStatus _Status = SocialMediaStatus.Unhealthy;
	private string _StatusMessage = "Not started";

	public YouTubeChatProvider(YouTubeChatConfiguration config, IConfiguration configuration, HttpClient httpClient)
	{
		_ChatConfig = config;
		Enabled = true; // config.Enabled;
		_HttpClient = httpClient;

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

		}
		catch (Exception ex)
		{
			Console.WriteLine($"Exception while fetching YouTubeChat: {ex.Message}");
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
			return outItems;

		}
		catch (Exception ex)
		{

			Console.WriteLine($"Exception while parsing YouTubeChat: {ex.Message}");

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
		}
		catch (Google.GoogleApiException ex)
		{
			// GoogleApiException: The service youtube has thrown an exception. HttpStatusCode is Forbidden. The user is not enabled for live streaming.
			Console.WriteLine($"Exception while fetching YouTube broadcasts: {ex.Message}");

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
		}
		catch (Google.GoogleApiException ex)
		{
			// GoogleApiException: The service youtube has thrown an exception. HttpStatusCode is Forbidden. The user is not enabled for live streaming.
			Console.WriteLine($"Exception while fetching YouTube broadcasts: {ex.Message}");

			_Status = SocialMediaStatus.Unhealthy;
			_StatusMessage = $"Exception while fetching YouTube broadcasts: {ex.Message}";

			return Enumerable.Empty<YouTubeBroadcast>();
		}
		ConvertToYouTubeBroadcasts(service, listRequest, broadcasts, outBroadcasts);


		return outBroadcasts.OrderBy(b => b.BroadcastTime);

	}

	private static SearchListResponse ConvertToYouTubeBroadcasts(YouTubeService service, SearchResource.ListRequest listRequest, SearchListResponse broadcasts, List<YouTubeBroadcast> outBroadcasts)
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
			}

			foreach (var broadcast in broadcasts.Items)
			{

				var videoRequest = service.Videos.List("liveStreamingDetails");
				videoRequest.Id = broadcast.Id.VideoId;
				var videoResponse = videoRequest.Execute();

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

	public Task<(SocialMediaStatus Status, string Message)> GetHealth() => Task.FromResult((_Status, _StatusMessage));

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


