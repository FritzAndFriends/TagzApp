using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Web;
using TagzApp.Common.Telemetry;
using TagzApp.Common.Configuration;

namespace TagzApp.Providers.TwitchChat;

public class TwitchChatProvider : ISocialMediaProvider, IDisposable
{
	private bool _DisposedValue;
	private IChatClient? _Client;
	private readonly IOptionsMonitor<TwitchChatConfiguration> _ConfigMonitor;
	private readonly IDisposable? _ConfigChangeSubscription;

	public string Id => "TWITCH";
	public string DisplayName => "TwitchChat";
	internal const string AppSettingsSection = "provider-twitch";
	public TimeSpan NewContentRetrievalFrequency => TimeSpan.FromSeconds(1);
	public string Description { get; init; } = "Twitch is where millions of people come together live every day to chat, interact, and make their own entertainment together.";
	public bool Enabled => _ConfigMonitor.CurrentValue.Enabled;

	private SocialMediaStatus _Status = SocialMediaStatus.Unhealthy;
	private string _StatusMessage = "Not started";

	private static readonly ConcurrentQueue<Content> _Contents = new();
	private static readonly CancellationTokenSource _CancellationTokenSource = new();
	private readonly ILogger<TwitchChatProvider> _Logger;
	private readonly TwitchProfileRepository _ProfileRepository;
	private readonly ProviderInstrumentation? _Instrumentation;

	public TwitchChatProvider(ILogger<TwitchChatProvider> logger, IConfiguration configuration, HttpClient client, IOptionsMonitor<TwitchChatConfiguration> configMonitor, ProviderInstrumentation? instrumentation = null)
	{
		_ConfigMonitor = configMonitor;
		_Logger = logger;
		_ProfileRepository = new TwitchProfileRepository(configuration, client);
		_Instrumentation = instrumentation;

		// Subscribe to configuration changes
		_ConfigChangeSubscription = _ConfigMonitor.OnChange(async (config, name) =>
		{
			await HandleConfigurationChange(config);
		});

		var currentConfig = _ConfigMonitor.CurrentValue;
		if (!string.IsNullOrWhiteSpace(currentConfig.Description))
		{
			Description = currentConfig.Description;
		}
	}

	internal TwitchChatProvider(IOptions<TwitchChatConfiguration> settings, ILogger<TwitchChatProvider> logger, IChatClient chatClient)
	{
		// For static scenarios (testing, development) - create a static options monitor wrapper that returns the settings value
		_ConfigMonitor = settings.ToStaticMonitor();
		_ConfigChangeSubscription = null; // No change subscription for static configurations
		_Logger = logger;
		_ProfileRepository = null!; // Will be null for testing
		ListenForMessages(chatClient);
	}

	private async Task HandleConfigurationChange(TwitchChatConfiguration newConfig)
	{
		var previousConfig = _ConfigMonitor.CurrentValue;

		// Handle channel name change
		if (_Client != null && _Client.IsRunning && previousConfig.ChannelName != newConfig.ChannelName)
		{
			_Client.ListenToNewChannel(newConfig.ChannelName);
		}

		// Handle enabled state change
		if (previousConfig.Enabled != newConfig.Enabled)
		{
			if (newConfig.Enabled)
			{
				await StartAsync();
			}
			else
			{
				await StopAsync();
			}
		}
	}

	private Task ListenForMessages(IChatClient? chatClient = null)
	{
		var currentConfig = _ConfigMonitor.CurrentValue;

		_Status = SocialMediaStatus.Degraded;
		_StatusMessage = "Starting TwitchChat client";

		_Client = chatClient ?? new ChatClient(currentConfig.ChannelName, currentConfig.ChatBotName, currentConfig.OAuthToken, _Logger);

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
				SourceUri = new Uri($"https://twitch.tv/{_ConfigMonitor.CurrentValue.ChannelName}"),
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
			_Logger.LogError(ex, "TwitchChat: Failed to initialize client");
			_Status = SocialMediaStatus.Unhealthy;
			_StatusMessage = $"Failed to initialize TwitchChat client: '{ex.Message}'";
			_Instrumentation?.RecordConnectionStatusChange(Id, SocialMediaStatus.Unhealthy);
			return Task.CompletedTask;
		}

		_Status = SocialMediaStatus.Healthy;
		_StatusMessage = "OK";
		_Logger.LogInformation("TwitchChat: Successfully connected");
		_Instrumentation?.RecordConnectionStatusChange(Id, SocialMediaStatus.Healthy);

		return Task.CompletedTask;

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
			_Logger.LogWarning("TwitchChat: Client is not running");
			_Instrumentation?.RecordConnectionStatusChange(Id, SocialMediaStatus.Unhealthy);

			return Task.FromResult(Enumerable.Empty<Content>());

		}

		if (!_Client.IsConnected)
		{

			// mark status as unhealthy and return empty list
			_Status = SocialMediaStatus.Unhealthy;
			_StatusMessage = "TwitchChat client is not logged in - check credentials";
			_Logger.LogWarning("TwitchChat: Client is not connected - check credentials");
			_Instrumentation?.RecordConnectionStatusChange(Id, SocialMediaStatus.Unhealthy);

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

		if (_Instrumentation is not null)
		{
			_Logger.LogInformation("TwitchChat: Retrieved {Count} new messages", messages.Count);
			foreach (var username in messages?.Select(x => x.Author?.UserName)!)
			{
				if (!string.IsNullOrEmpty(username))
				{
					_Instrumentation.AddMessage("twitchchat", username);
				}

			}
		}

		messages.ForEach(m => m.HashtagSought = tag.Text);

		return Task.FromResult(messages.AsEnumerable());

	}

	protected virtual void Dispose(bool disposing)
	{
		if (!_DisposedValue)
		{
			if (disposing)
			{
				_ConfigChangeSubscription?.Dispose();
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

	public async Task StartAsync()
	{
		var currentConfig = _ConfigMonitor.CurrentValue;

		if (string.IsNullOrEmpty(currentConfig.ChannelName) || string.IsNullOrEmpty(currentConfig.OAuthToken))
		{
			_Status = SocialMediaStatus.Unhealthy;
			_StatusMessage = "TwitchChat client is not configured";
			return;
		}

		await ListenForMessages();
	}

	public Task<(SocialMediaStatus Status, string Message)> GetHealth()
	{
		return Task.FromResult((_Status, _StatusMessage));
	}

	public Task StopAsync()
	{

		_Client?.Stop();
		_Status = SocialMediaStatus.Disabled;
		_StatusMessage = "TwitchChat client is stopped";
		_Logger.LogInformation("TwitchChat: Provider stopped");
		_Instrumentation?.RecordConnectionStatusChange(Id, SocialMediaStatus.Disabled);

		return Task.CompletedTask;
	}

	public async Task<IProviderConfiguration> GetConfiguration(IConfigureTagzApp configure)
	{
		return await BaseProviderConfiguration<TwitchChatConfiguration>.CreateFromConfigurationAsync<TwitchChatConfiguration>(configure);
	}

	public async Task SaveConfiguration(IConfigureTagzApp configure, IProviderConfiguration providerConfiguration)
	{
		var twitchConfig = (TwitchChatConfiguration)providerConfiguration;
		await twitchConfig.SaveToConfigurationAsync(configure);

		var currentConfig = _ConfigMonitor.CurrentValue;

		// handle channelname change
		if (currentConfig.ChannelName != twitchConfig.ChannelName)
		{
			_Client?.ListenToNewChannel(twitchConfig.ChannelName);
		}

		// Handle enabled state change
		if (currentConfig.Enabled != providerConfiguration.Enabled && currentConfig.Enabled)
		{
			await StopAsync();
		}
		else if (currentConfig.Enabled != providerConfiguration.Enabled && !currentConfig.Enabled)
		{
			await StartAsync();
		}

		// The IOptionsMonitor will automatically pick up the changes from the saved configuration
		// No need to manually update since it's reactive
	}
}
