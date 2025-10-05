using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using TagzApp.Common.Telemetry;

namespace TagzApp.Providers.Discord;

/// <summary>
/// Discord provider for TagzApp that monitors Discord channels for messages
/// </summary>
public class DiscordProvider : ISocialMediaProvider, IDisposable
{
	private bool _DisposedValue;
	private DiscordGatewayService? _GatewayService;
	private readonly IOptionsMonitor<DiscordConfiguration> _ConfigMonitor;
	private readonly IDisposable? _ConfigChangeSubscription;

	public string Id => "DISCORD";
	public string DisplayName => "Discord";
	internal const string AppSettingsSection = "provider-discord";
	public TimeSpan NewContentRetrievalFrequency => TimeSpan.FromSeconds(1);
	public string Description { get; init; } = "Monitor Discord channels for live messages and community interactions.";
	public bool Enabled => _ConfigMonitor.CurrentValue.Enabled;

	private SocialMediaStatus _Status = SocialMediaStatus.Unhealthy;
	private string _StatusMessage = "Not started";

	private static readonly ConcurrentQueue<Content> _Contents = new();
	private static readonly ConcurrentQueue<DiscordMessage> _MessageQueue = new();
	private static readonly CancellationTokenSource _CancellationTokenSource = new();
	private readonly ILogger<DiscordProvider> _Logger;
	private readonly HttpClient _HttpClient;
	private readonly ProviderInstrumentation? _Instrumentation;

	public DiscordProvider(
		ILogger<DiscordProvider> logger, 
		IConfiguration configuration, 
		HttpClient client, 
		IOptionsMonitor<DiscordConfiguration> configMonitor, 
		ProviderInstrumentation? instrumentation = null)
	{
		_ConfigMonitor = configMonitor;
		_Logger = logger;
		_HttpClient = client;
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

	public SocialMediaStatus Status => _Status;
	public string StatusMessage => _StatusMessage;

	public async Task<IEnumerable<Content>> GetContentForHashtag(Hashtag tag, DateTimeOffset since)
	{
		var outContent = new List<Content>();

		// Process messages from the queue
		while (_MessageQueue.TryDequeue(out var message))
		{
			var content = MapDiscordMessageToContent(message, tag);
			if (content != null)
			{
				outContent.Add(content);
				_Contents.Enqueue(content);

				// Maintain queue size limit
				while (_Contents.Count > _ConfigMonitor.CurrentValue.MaxQueueSize && _Contents.TryDequeue(out _))
				{
					// Remove excess items
				}
			}
		}

		return outContent;
	}

	public async Task StartAsync()
	{
		var config = _ConfigMonitor.CurrentValue;
		
		if (!config.Enabled || !config.IsValid)
		{
			_Status = SocialMediaStatus.Disabled;
			_StatusMessage = config.Enabled ? "Invalid configuration" : "Disabled";
			_Logger.LogInformation("Discord provider is disabled or has invalid configuration");
			return;
		}

		try
		{
			_Status = SocialMediaStatus.Connecting;
			_StatusMessage = "Starting Discord Gateway connection...";

			_Logger.LogInformation("Starting Discord provider for guild {GuildId}, channel {ChannelId}", 
				config.GuildId, config.ChannelId);

			// Initialize Gateway service
			_GatewayService = new DiscordGatewayService(_Logger, config, _MessageQueue);
			_GatewayService.ConnectionStatusChanged += OnConnectionStatusChanged;
			_GatewayService.MessageReceived += OnMessageReceived;

			// Start the Gateway connection
			await _GatewayService.StartAsync(_CancellationTokenSource.Token);

			_Status = SocialMediaStatus.Healthy;
			_StatusMessage = "Connected and monitoring channel";
		}
		catch (Exception ex)
		{
			_Logger.LogError(ex, "Failed to start Discord provider");
			_Status = SocialMediaStatus.Unhealthy;
			_StatusMessage = $"Failed to start: {ex.Message}";
		}
	}

	public async Task StopAsync()
	{
		if (_GatewayService != null)
		{
			_GatewayService.ConnectionStatusChanged -= OnConnectionStatusChanged;
			_GatewayService.MessageReceived -= OnMessageReceived;
			await _GatewayService.StopAsync();
			_GatewayService.Dispose();
			_GatewayService = null;
		}

		_Status = SocialMediaStatus.Disabled;
		_StatusMessage = "Stopped";
		_Logger.LogInformation("Discord provider stopped");
	}

	private async Task HandleConfigurationChange(DiscordConfiguration newConfig)
	{
		_Logger.LogInformation("Discord provider configuration changed");

		// Restart the service with new configuration
		await StopAsync();
		
		if (newConfig.Enabled && newConfig.IsValid)
		{
			await StartAsync();
		}
	}

	private void OnConnectionStatusChanged(object? sender, string status)
	{
		_Logger.LogInformation("Discord Gateway status: {Status}", status);
		
		_Status = status switch
		{
			"Connected" or "Ready" => SocialMediaStatus.Healthy,
			"Connecting..." or "Reconnecting" => SocialMediaStatus.Connecting,
			var s when s.StartsWith("Failed") => SocialMediaStatus.Unhealthy,
			_ => SocialMediaStatus.Unknown
		};
		
		_StatusMessage = status;
	}

	private void OnMessageReceived(object? sender, DiscordMessage message)
	{
		_Logger.LogDebug("Received Discord message from {Author} in channel {ChannelId}", 
			message.Author.DisplayName, message.ChannelId);

		_Instrumentation?.IncrementContentCounter("DISCORD");
	}

	private Content? MapDiscordMessageToContent(DiscordMessage message, Hashtag tag)
	{
		try
		{
			var messageText = BuildMessageText(message);
			
			var content = new Content
			{
				Provider = "DISCORD",
				ProviderId = message.Id,
				Type = ContentType.Message,
				Timestamp = message.Timestamp,
				SourceUri = new Uri($"https://discord.com/channels/{message.GuildId}/{message.ChannelId}/{message.Id}"),
				Author = new Creator
				{
					DisplayName = message.Author.DisplayName,
					UserName = message.Author.FullUsername,
					ProfileUri = new Uri($"https://discord.com/users/{message.Author.Id}"),
					ProfileImageUri = new Uri(message.Author.AvatarUrl)
				},
				Text = messageText,
				HashtagSought = tag.Text.ToLowerInvariant(),
				
				// Discord-specific metadata
				ExtendedMetadata = new Dictionary<string, object>
				{
					["guildId"] = message.GuildId ?? string.Empty,
					["channelId"] = message.ChannelId,
					["messageType"] = message.Type.ToString(),
					["isBot"] = message.Author.Bot,
					["hasEmbeds"] = message.Embeds.Length > 0,
					["hasAttachments"] = message.Attachments.Length > 0,
					["isReply"] = message.MessageReference != null,
					["isEdited"] = message.EditedTimestamp.HasValue
				}
			};

			return content;
		}
		catch (Exception ex)
		{
			_Logger.LogWarning(ex, "Failed to map Discord message {MessageId} to content", message.Id);
			return null;
		}
	}

	private string BuildMessageText(DiscordMessage message)
	{
		var text = message.Content;

		// Add attachment information
		if (message.Attachments.Length > 0)
		{
			var attachmentInfo = string.Join(", ", message.Attachments.Select(a => 
				$"📎 {a.Filename}"));
			text += $"\n\n{attachmentInfo}";
		}

		// Add embed information if enabled
		if (_ConfigMonitor.CurrentValue.EnableRichEmbeds && message.Embeds.Length > 0)
		{
			foreach (var embed in message.Embeds)
			{
				if (!string.IsNullOrEmpty(embed.Title))
					text += $"\n\n**{embed.Title}**";
				if (!string.IsNullOrEmpty(embed.Description))
					text += $"\n{embed.Description}";
			}
		}

		return text;
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!_DisposedValue)
		{
			if (disposing)
			{
				_ConfigChangeSubscription?.Dispose();
				_GatewayService?.Dispose();
				_CancellationTokenSource?.Cancel();
				_CancellationTokenSource?.Dispose();
			}

			_DisposedValue = true;
		}
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}