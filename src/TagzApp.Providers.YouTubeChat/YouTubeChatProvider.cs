using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;

namespace TagzApp.Providers.YouTubeChat;

public class YouTubeChatProvider : ISocialMediaProvider, IDisposable
{
	private readonly YouTubeChatConfiguration _ChatConfig;
	private readonly string? _ClientId;
	private readonly string? _ClientSecret;
	public const string ProviderName = "YouTubeChat";

	public string Id => "YOUTUBE-CHAT";
	public string DisplayName => ProviderName;
	public TimeSpan NewContentRetrievalFrequency { get; set; } = TimeSpan.FromMinutes(1);

	public string NewestId { get; set; }
	public string LiveChatId { get; set; }
	public string RefreshToken { get; set; }
	public string YouTubeEmailId { get; set; }

	private string _GoogleException = string.Empty;

	private CancellationTokenSource _TokenSource = new();
	private YouTubeService _Service;
	private bool _DisposedValue;
	private string _NextPageToken;

	public YouTubeChatProvider(YouTubeChatConfiguration config)
	{
		_ChatConfig = config;
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
			//NewContentRetrievalFrequency = TimeSpan.FromMilliseconds(contents.PollingIntervalMillis.HasValue ? contents.PollingIntervalMillis.Value : 45000);
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Exception while fetching YouTubeChat: {ex.Message}");
			if (ex.Message.Contains("live chat is no longer live"))
			{
				_GoogleException = $"{LiveChatId}:{ex.Message}";
				LiveChatId = string.Empty;
			}
			return Enumerable.Empty<Content>();
		}

		return contents.Items.Select(i => new Content
		{
			Author = new Creator
			{
				DisplayName = i.AuthorDetails.DisplayName,
				ProfileImageUri = new Uri(i.AuthorDetails.ProfileImageUrl),
				ProfileUri = new Uri($"https://www.youtube.com/channel/{i.AuthorDetails.ChannelId}")
			},
			Provider = Id,
			ProviderId = i.Id,
			Text = i.Snippet.DisplayMessage,
			SourceUri = new Uri($"https://youtube.com/livechat/{LiveChatId}"),
			Timestamp = DateTimeOffset.Parse(i.Snippet.PublishedAtRaw),
			Type = ContentType.Message,
			HashtagSought = tag?.Text ?? ""
		}).ToArray();

	}

	private async Task<YouTubeService> GetGoogleService()
	{

		if (_Service is not null) return _Service;

		var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
		{
			ClientSecrets = new ClientSecrets
			{
				ClientId = _ChatConfig.ClientId,
				ClientSecret = _ChatConfig.ClientSecret
			},
			Scopes = new[] { YouTubeChatConfiguration.Scope_YouTube }
		});
		var token = await flow.RefreshTokenAsync(YouTubeEmailId, RefreshToken, CancellationToken.None);
		var credential = new UserCredential(flow, "me", token);

		_Service = new YouTubeService(new BaseClientService.Initializer
		{
			HttpClientInitializer = credential
		});
		return _Service;
	}

	public async Task StartAsync()
	{

		//var initializer = new BaseClientService.Initializer()
		//{
		//	ApplicationName = "TagzApp",
		//	HttpClientInitializer = credential
		//};

		//_Service = new YouTubeService(initializer);
		//var list = new LiveBroadcastsResource(_Service);
		//var listRequest = list.List(new Google.Apis.Util.Repeatable<string>(new[] { "snippet" }));
		//listRequest.Id = _ChatConfig.BroadcastId;

		//try
		//{
		//	var response = listRequest.Execute();

		//	if (response is not null)
		//	{
		//		_LiveChatId = response.Items.FirstOrDefault()?.Snippet?.LiveChatId ?? string.Empty;
		//	}

		//}
		//catch (Exception ex)
		//{

		//	Console.WriteLine(ex);

		//}

	}

	public async Task<string> GetChannelForUserAsync()
	{

		var service = await GetGoogleService();

		var channelRequest = service.Channels.List("snippet");
		channelRequest.Mine = true;
		var channels = channelRequest.Execute();

		return channels.Items.First().Snippet.Title;

	}

	public async Task<IEnumerable<YouTubeBroadcast>> GetBroadcastsForUser()
	{

		var service = await GetGoogleService();

		var listRequest = service.LiveBroadcasts.List("snippet");
		listRequest.Mine = true;
		listRequest.BroadcastType = LiveBroadcastsResource.ListRequest.BroadcastTypeEnum.Event__;
		var broadcasts = listRequest.Execute();

		var outBroadcasts = new List<YouTubeBroadcast>();
		outBroadcasts.AddRange(broadcasts.Items
			.Select(i => new YouTubeBroadcast(i.Id, i.Snippet.Title, i.Snippet.ActualStartTimeDateTimeOffset ?? i.Snippet.ScheduledStartTimeDateTimeOffset, i.Snippet.LiveChatId)));

		while (!string.IsNullOrEmpty(broadcasts.NextPageToken) && outBroadcasts.Count < 20)
		{
			listRequest.PageToken = broadcasts.NextPageToken;
			broadcasts = listRequest.Execute();
			outBroadcasts.AddRange(broadcasts.Items
				.Select(i => new YouTubeBroadcast(i.Id, i.Snippet.Title, i.Snippet.ActualStartTimeDateTimeOffset ?? i.Snippet.ScheduledStartTimeDateTimeOffset, i.Snippet.LiveChatId)));
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

	#endregion

}

public record YouTubeBroadcast(string Id, string Title, DateTimeOffset? BroadcastTime, string LiveChatId);
