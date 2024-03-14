using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Web;

namespace TagzApp.Providers.TwitchChat;

public class TwitchChatProvider : ISocialMediaProvider, IDisposable
{
	private bool _DisposedValue;
	private IChatClient? _Client;

	public string Id => "TWITCH";
	public string DisplayName => "TwitchChat";
	public TimeSpan NewContentRetrievalFrequency => TimeSpan.FromSeconds(1);
	public string Description { get; init; } = "Twitch is where millions of people come together live every day to chat, interact, and make their own entertainment together.";
	public bool Enabled { get; }

	private SocialMediaStatus _Status = SocialMediaStatus.Unhealthy;
	private string _StatusMessage = "Not started";

	private static readonly ConcurrentQueue<Content> _Contents = new();
	private static readonly CancellationTokenSource _CancellationTokenSource = new();
	private readonly TwitchChatConfiguration _Settings;
	private readonly ILogger<TwitchChatProvider> _Logger;
	private readonly TwitchProfileRepository _ProfileRepository;

	public TwitchChatProvider(ILogger<TwitchChatProvider> logger, IConfiguration configuration, HttpClient client)
	{
		_Settings = ConfigureTagzAppFactory.Current.GetConfigurationById<TwitchChatConfiguration>(Id).GetAwaiter().GetResult();
		_Logger = logger;
		_ProfileRepository = new TwitchProfileRepository(configuration, client);
		Enabled = _Settings.Enabled;

		if (!string.IsNullOrWhiteSpace(_Settings.Description))
		{
			Description = _Settings.Description;
		}
	}

	internal TwitchChatProvider(IOptions<TwitchChatConfiguration> settings, ILogger<TwitchChatProvider> logger, IChatClient chatClient)
	{
		_Settings = settings.Value;
		_Logger = logger;
		ListenForMessages(chatClient);
	}

	private async Task ListenForMessages(IChatClient chatClient = null)
	{

		_Status = SocialMediaStatus.Degraded;
		_StatusMessage = "Starting TwitchChat client";

		var token = _CancellationTokenSource.Token;
		_Client = chatClient ?? new ChatClient(_Settings.ChannelName, _Settings.ChatBotName, _Settings.OAuthToken, _Logger);

		_Client.NewMessage += async (sender, args) =>
		{

			string profileUrl = string.Empty;
			try
			{
				profileUrl = await IdentifyProfilePic(args.UserName);
			}
			catch (Exception ex)
			{
				_Logger.LogError(ex, "Failed to identify profile pic for {UserName}", args.UserName);
				profileUrl = "about:blank";
			}

			_Contents.Enqueue(new Content
			{
				Provider = Id,
				ProviderId = args.MessageId,
				SourceUri = new Uri($"https://twitch.tv/{_Settings.ChannelName}"),
				Author = new Creator
				{
					ProfileUri = new Uri($"https://twitch.tv/{args.UserName}"),
					ProfileImageUri = new Uri(profileUrl),
					DisplayName = args.DisplayName,
					UserName = $"@{args.DisplayName}"
				},
				Text = HttpUtility.HtmlEncode(args.Message),
				Type = ContentType.Chat,
				Timestamp = args.Timestamp,
				Emotes = args.Emotes
			});
		};

		try
		{
			_Client.Init();
		}
		catch (Exception ex)
		{
			_Logger.LogError(ex, "Failed to initialize TwitchChat client");
			_Status = SocialMediaStatus.Unhealthy;
			_StatusMessage = $"Failed to initialize TwitchChat client: '{ex.Message}'";
			return;
		}

		_Status = SocialMediaStatus.Healthy;
		_StatusMessage = "OK";

	}

	private async Task<string> IdentifyProfilePic(string userName)
	{
		return await _ProfileRepository.GetProfilePic(userName);
	}

	public Task<IEnumerable<Content>> GetContentForHashtag(Hashtag tag, DateTimeOffset since)
	{

		if (!_Client?.IsRunning ?? true)
		{

			// mark status as unhealthy and return empty list
			_Status = SocialMediaStatus.Unhealthy;
			_StatusMessage = "TwitchChat client is not running";

			return Task.FromResult(Enumerable.Empty<Content>());

		}

		if (!_Client.IsConnected)
		{

			// mark status as unhealthy and return empty list
			_Status = SocialMediaStatus.Unhealthy;
			_StatusMessage = "TwitchChat client is not logged in - check credentials";

			return Task.FromResult(Enumerable.Empty<Content>());

		}
		else
		{
			_Status = SocialMediaStatus.Healthy;
			_StatusMessage = "OK";
		}

		var messages = _Contents.ToList();
		if (messages.Count() == 0) return Task.FromResult(Enumerable.Empty<Content>());

		var messageCount = messages.Count();
		for (var i = 0; i < messageCount; i++)
		{
			_Contents.TryDequeue(out _);
		}

		_Status = SocialMediaStatus.Healthy;
		_StatusMessage = "OK";

		messages.ForEach(m => m.HashtagSought = tag.Text);

		return Task.FromResult(messages.AsEnumerable());

	}

	protected virtual void Dispose(bool disposing)
	{
		if (!_DisposedValue)
		{
			if (disposing)
			{
				_Client?.Dispose();
			}

			// TODO: free unmanaged resources (unmanaged objects) and override finalizer
			// TODO: set large fields to null
			_DisposedValue = true;
		}
	}

	// TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
	~TwitchChatProvider()
	{
		// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
		Dispose(disposing: false);
	}

	public void Dispose()
	{
		// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	public Task StartAsync()
	{

		if (string.IsNullOrEmpty(_Settings.ChannelName) || string.IsNullOrEmpty(_Settings.OAuthToken))
		{
			_Status = SocialMediaStatus.Unhealthy;
			_StatusMessage = "TwitchChat client is not configured";
			return Task.CompletedTask;
		}

		ListenForMessages();
		return Task.CompletedTask;
	}

	public Task<(SocialMediaStatus Status, string Message)> GetHealth()
	{
		return Task.FromResult((_Status, _StatusMessage));
	}

	public Task StopAsync()
	{
		return Task.CompletedTask;
	}

	public async Task<IProviderConfiguration> GetConfiguration(IConfigureTagzApp configure)
	{
		return await configure.GetConfigurationById<TwitchChatConfiguration>(Id);
	}

	public async Task SaveConfiguration(IConfigureTagzApp configure, IProviderConfiguration providerConfiguration)
	{
		await configure.SetConfigurationById(Id, (TwitchChatConfiguration)providerConfiguration);
	}
}
