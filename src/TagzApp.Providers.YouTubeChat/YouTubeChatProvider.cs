using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using Microsoft.Extensions.Configuration;

namespace TagzApp.Providers.YouTubeChat;

public class YouTubeChatProvider : ISocialMediaProvider, IDisposable
{
	private readonly YouTubeChatConfiguration _ChatConfig;
	public const string ProviderName = "YouTubeChat";

	private const string YouTubeApiKey = "AIzaSyD-zImnv2v_RMqUhYb4YJnzO6vKfIqaUR8";
	private const string DotNetChannelId = "UCvtT19MZW8dq5Wwfu6B0oxw";

	public string Id => "YOUTUBE-CHAT";
	public string DisplayName => ProviderName;
	public string Description { get; init; }
	public TimeSpan NewContentRetrievalFrequency { get; set; } = TimeSpan.FromSeconds(15);

	public string NewestId { get; set; }
	public string LiveChatId { get; set; }
	public string RefreshToken { get; set; }
	public string YouTubeEmailId { get; set; }
	public bool Enabled { get; }

	private string _GoogleException = string.Empty;

	private CancellationTokenSource _TokenSource = new();
	private YouTubeService _Service;
	private bool _DisposedValue;
	private string _NextPageToken;

	private SocialMediaStatus _Status = SocialMediaStatus.Unhealthy;
	private string _StatusMessage = "Not started";

	public YouTubeChatProvider(YouTubeChatConfiguration config, IConfiguration configuration)
	{
		_ChatConfig = config;
		Enabled = true; // config.Enabled;

		LiveChatId = config.LiveChatId;

	}

	public async Task<IEnumerable<Content>> GetContentForHashtag(Hashtag tag, DateTimeOffset since)
	{

		if (string.IsNullOrEmpty(LiveChatId) || (!string.IsNullOrEmpty(_GoogleException) && _GoogleException.StartsWith(LiveChatId))) return Enumerable.Empty<Content>();
		var liveChatListRequest = new LiveChatMessagesResource.ListRequest(_Service, LiveChatId, new(new[] { "id", "snippet", "authorDetails" }));
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
				_GoogleException = $"{LiveChatId}:{ex.Message}";
				LiveChatId = string.Empty;
			}

			_Status = SocialMediaStatus.Unhealthy;
			_StatusMessage = $"Exception while fetching YouTubeChat: {ex.Message}";

			return Enumerable.Empty<Content>();
		}

		_Status = SocialMediaStatus.Healthy;
		_StatusMessage = $"OK -- adding ({contents.Items.Count}) messages for chatid '{LiveChatId}' at {DateTimeOffset.UtcNow}";

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
				SourceUri = new Uri($"https://youtube.com/livechat/{LiveChatId}"),
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
			ApiKey = YouTubeApiKey,
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
		var broadcasts = GetBroadcastsForUser();
		if (broadcasts.Any())
		{
			LiveChatId = broadcasts.First().LiveChatId;
		}

	}

	public async Task<string> GetChannelForUserAsync()
	{

		var service = await GetGoogleService();

		var channelRequest = service.Search.List("snippet");
		//channelRequest.Mine = true;
		channelRequest.ChannelId = DotNetChannelId;
		var channels = channelRequest.Execute();

		// Not sure if this is needed, can't replicate "fisrt" error. (https://github.com/FritzAndFriends/TagzApp/issues/241)
		return channels.Items?.First().Snippet.Title ?? "Unknown Channel Title";

	}

	public IEnumerable<YouTubeBroadcast> GetBroadcastsForUser()
	{

		var service = GetGoogleService().GetAwaiter().GetResult();

		var listRequest = service.Search.List("snippet");
		listRequest.Q = ".NET Conf 2024 - Day 2";
		listRequest.ChannelId = DotNetChannelId;
		//listRequest.EventType = SearchResource.ListRequest.EventTypeEnum.Live ;
		listRequest.Type = "live";
		listRequest.MaxResults = 500;
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
		//outBroadcasts.AddRange(broadcasts.Items
		//	.Select(i => new YouTubeBroadcast(i.Id.VideoId, i.Snippet.Title, i.Snippet.PublishedAtDateTimeOffset);

		var first = true;
		while (!string.IsNullOrEmpty(broadcasts.NextPageToken) && outBroadcasts.Count < 20)
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

				if (!broadcast.Snippet.Title.StartsWith(".NET Conf 2024")) continue;

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
						broadcast.Snippet.PublishedAtDateTimeOffset,
						liveChatId
					));

			}


		}

		return outBroadcasts.OrderBy(b => b.BroadcastTime);

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


