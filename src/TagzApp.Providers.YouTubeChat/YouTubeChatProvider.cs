using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Microsoft.Extensions.Options;

namespace TagzApp.Providers.YouTubeChat;

public class YouTubeChatProvider : ISocialMediaProvider, IDisposable
{
	private readonly YouTubeChatConfiguration _ChatConfig;

	public string Id => "YOUTUBE-CHAT";
	public string DisplayName => "YouTube Chat";
	public TimeSpan NewContentRetrievalFrequency => TimeSpan.FromSeconds(10);

	public string NewestId { get; set; }

	private CancellationTokenSource _TokenSource = new();
	private string _LiveChatId = string.Empty;
	private YouTubeService _Service;
	private bool _DisposedValue;
	private Stream _ChatStream;

	public YouTubeChatProvider(IOptions<YouTubeChatConfiguration> chatConfig)
	{
		_ChatConfig = chatConfig.Value;
	}

	public async Task<IEnumerable<Content>> GetContentForHashtag(Hashtag tag, DateTimeOffset since)
	{

		var content = new List<Content>();

		// liveChatListRequest.

		return content;


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

		if (string.IsNullOrEmpty(_LiveChatId)) return;
		var liveChatListRequest = new LiveChatMessagesResource.ListRequest(_Service, _LiveChatId, new(new[] { "id", "snippet", "authorDetails" }));
		_ChatStream = await liveChatListRequest.ExecuteAsStreamAsync(_TokenSource.Token);

	}

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

	public string GetChannelForUser(string accessToken)
	{

		var credential = GoogleCredential.FromAccessToken(accessToken);
		var service = new YouTubeService(new BaseClientService.Initializer
		{
			HttpClientInitializer = credential
		});

		var channelRequest = service.Channels.List("snippet");
		channelRequest.Mine = true;
		var channels = channelRequest.Execute();

		return channels.Items.First().Snippet.Title;

	}

	public IEnumerable<YouTubeBroadcast> GetBroadcastsForUser(string accessToken)
	{

		var credential = GoogleCredential.FromAccessToken(accessToken);
		var service = new YouTubeService(new BaseClientService.Initializer
		{
			HttpClientInitializer = credential
		});

		var listRequest = service.LiveBroadcasts.List("snippet");
		listRequest.Mine = true;
		listRequest.BroadcastType = LiveBroadcastsResource.ListRequest.BroadcastTypeEnum.Event__;
		var broadcasts = listRequest.Execute();


		//var credential = GoogleWebAuthorizationBroker.AuthorizeAsync()
		return broadcasts.Items
			.Select(i => new YouTubeBroadcast(i.Id, i.Snippet.Title, i.Snippet.ActualStartTimeDateTimeOffset ?? i.Snippet.ScheduledStartTimeDateTimeOffset, i.Snippet.LiveChatId))
			.OrderBy(b => b.BroadcastTime);

	}


}

public record YouTubeBroadcast(string Id, string Title, DateTimeOffset? BroadcastTime, string LiveChatId);
