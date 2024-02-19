using System.Text.Json;
using System.Text.Json.Serialization;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using Microsoft.Extensions.Configuration;

namespace TagzApp.Providers.YouTubeChat;

public class YouTubeChatProvider : ISocialMediaProvider, IDisposable
{
	private readonly YouTubeChatConfiguration _ChatConfig;
	public const string ProviderName = "YouTubeChat";

	public string Id => "YOUTUBE-CHAT";
	public string DisplayName => ProviderName;
	public string Description { get; init; }
	public TimeSpan NewContentRetrievalFrequency { get; set; } = TimeSpan.FromMinutes(1);

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
		Enabled = config.Enabled;

		var rawConfig = configuration["ApplicationConfiguration:YouTubeChatConfiguration"];

		if (rawConfig == null || rawConfig == "{}") return;

		var youtubeConfig = JsonSerializer.Deserialize<YouTubeChatApplicationConfiguration>(rawConfig);
		RefreshToken = youtubeConfig.RefreshToken;
		LiveChatId = youtubeConfig.LiveChatId;
		YouTubeEmailId = youtubeConfig.ChannelEmail;

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
				Text = i.Snippet.DisplayMessage,
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

		var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
		{
			ClientSecrets = new ClientSecrets
			{
				ClientId = _ChatConfig.ClientId,
				ClientSecret = _ChatConfig.ClientSecret
			},
			Scopes = new[] { YouTubeChatConfiguration.Scope_YouTube }
		});

		TokenResponse token;
		try
		{
			token = await flow.RefreshTokenAsync(YouTubeEmailId, RefreshToken, CancellationToken.None);
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Exception while refreshing token: {ex.Message}");

			_Status = SocialMediaStatus.Unhealthy;
			_StatusMessage = $"Exception while refreshing token: {ex.Message}";

			throw;
		}
		var credential = new UserCredential(flow, "me", token);

		_Service = new YouTubeService(new BaseClientService.Initializer
		{
			HttpClientInitializer = credential
		});

		_Status = SocialMediaStatus.Degraded;
		_StatusMessage = "Starting YouTubeChat client";

		return _Service;
	}

	public async Task StartAsync()
	{

		if (string.IsNullOrEmpty(LiveChatId) || string.IsNullOrEmpty(RefreshToken)) return;

		_Service = await GetGoogleService();

	}

	public async Task<string> GetChannelForUserAsync()
	{

		var service = await GetGoogleService();

		var channelRequest = service.Channels.List("snippet");
		channelRequest.Mine = true;
		var channels = channelRequest.Execute();

		// Not sure if this is needed, can't replicate "fisrt" error. (https://github.com/FritzAndFriends/TagzApp/issues/241)
		return channels.Items?.First().Snippet.Title ?? "Unknown Channel Title";

	}

	public async Task<IEnumerable<YouTubeBroadcast>> GetBroadcastsForUser()
	{

		var service = await GetGoogleService();

		var listRequest = service.LiveBroadcasts.List("snippet");
		listRequest.Mine = true;
		listRequest.BroadcastType = LiveBroadcastsResource.ListRequest.BroadcastTypeEnum.Event__;
		LiveBroadcastListResponse broadcasts;
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

[JsonSerializable(typeof(YouTubeBroadcast))]
public record YouTubeBroadcast(string Id, string Title, DateTimeOffset? BroadcastTime, string LiveChatId);


