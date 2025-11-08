using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace TagzApp.Providers.Discord;

public class DiscordConfiguration : BaseProviderConfiguration<DiscordConfiguration>
{
	/// <summary>
	/// The configuration key used to store this configuration in the TagzApp configuration system
	/// </summary>
	protected override string ConfigurationKey => "provider-discord";

	[JsonPropertyOrder(1)]
	[Required]
	[Display(Name = "Bot Token", Description = "Discord bot token for API access")]
	public string BotToken { get; set; } = string.Empty;

	[JsonPropertyOrder(2)]
	[Required]
	[Display(Name = "Guild ID", Description = "Discord server (guild) ID")]
	public string GuildId { get; set; } = string.Empty;

	[JsonPropertyOrder(3)]
	[Required]
	[Display(Name = "Channel ID", Description = "Discord channel ID to monitor")]
	public string ChannelId { get; set; } = string.Empty;

	[JsonPropertyOrder(4)]
	[Display(Name = "Guild Name", Description = "Discord server name (auto-populated)")]
	public string GuildName { get; set; } = string.Empty;

	[JsonPropertyOrder(5)]
	[Display(Name = "Channel Name", Description = "Discord channel name (auto-populated)")]
	public string ChannelName { get; set; } = string.Empty;

	[JsonPropertyOrder(6)]
	[Display(Name = "Include Bot Messages", Description = "Show messages from other bots")]
	public bool IncludeBotMessages { get; set; } = false;

	[JsonPropertyOrder(7)]
	[Display(Name = "Include System Messages", Description = "Show join/leave and system messages")]
	public bool IncludeSystemMessages { get; set; } = false;

	[JsonPropertyOrder(8)]
	[Display(Name = "Minimum Message Length", Description = "Hide messages shorter than this")]
	public int MinMessageLength { get; set; } = 1;

	// NOTE: We have blocked users at the application level in TagzApp
	// [JsonPropertyOrder(9)]
	// [Display(Name = "Blocked Users", Description = "Comma-separated list of user IDs to block")]
	// public string BlockedUsers { get; set; } = string.Empty;

	[JsonPropertyOrder(10)]
	[Display(Name = "Max Queue Size", Description = "Maximum messages to keep in memory")]
	public int MaxQueueSize { get; set; } = 1000;

	[JsonPropertyOrder(11)]
	[Display(Name = "Reconnect Attempts", Description = "Max reconnection attempts")]
	public int MaxReconnectAttempts { get; set; } = 5;

	[JsonPropertyOrder(12)]
	[Display(Name = "Enable Rich Embeds", Description = "Include Discord embed content")]
	public bool EnableRichEmbeds { get; set; } = true;

	public static DiscordConfiguration Empty => new()
	{
		BotToken = string.Empty,
		GuildId = string.Empty,
		ChannelId = string.Empty,
		GuildName = string.Empty,
		ChannelName = string.Empty
	};

	[JsonIgnore]
	public override string Name => "Discord";

	[JsonIgnore]
	public override string Description => "Monitor Discord channels for live messages";

	public override bool Enabled { get; set; }

	[JsonIgnore]
	public override string[] Keys => [
		nameof(BotToken),
		nameof(GuildId),
		nameof(ChannelId),
		nameof(GuildName),
		nameof(ChannelName),
		nameof(IncludeBotMessages),
		nameof(IncludeSystemMessages),
		nameof(MinMessageLength),
		nameof(BlockedUsers),
		nameof(MaxQueueSize),
		nameof(MaxReconnectAttempts),
		nameof(EnableRichEmbeds)
	];

	/// <summary>
	/// Validation check for required configuration
	/// </summary>
	[JsonIgnore]
	public bool IsValid => 
		!string.IsNullOrEmpty(BotToken) && 
		!string.IsNullOrEmpty(GuildId) && 
		!string.IsNullOrEmpty(ChannelId) &&
		BotToken.Length > 50; // Discord tokens are ~70 characters

	public override string GetConfigurationByKey(string key)
	{
		return key switch
		{
			nameof(BotToken) => BotToken,
			nameof(GuildId) => GuildId,
			nameof(ChannelId) => ChannelId,
			nameof(GuildName) => GuildName,
			nameof(ChannelName) => ChannelName,
			nameof(IncludeBotMessages) => IncludeBotMessages.ToString(),
			nameof(IncludeSystemMessages) => IncludeSystemMessages.ToString(),
			nameof(MinMessageLength) => MinMessageLength.ToString(),
			nameof(BlockedUsers) => BlockedUsers,
			nameof(MaxQueueSize) => MaxQueueSize.ToString(),
			nameof(MaxReconnectAttempts) => MaxReconnectAttempts.ToString(),
			nameof(EnableRichEmbeds) => EnableRichEmbeds.ToString(),
			nameof(Enabled) => Enabled.ToString(),
			_ => string.Empty
		};
	}

	public override void SetConfigurationByKey(string key, string value)
	{
		switch (key)
		{
			case nameof(BotToken):
				BotToken = value;
				break;
			case nameof(GuildId):
				GuildId = value;
				break;
			case nameof(ChannelId):
				ChannelId = value;
				break;
			case nameof(GuildName):
				GuildName = value;
				break;
			case nameof(ChannelName):
				ChannelName = value;
				break;
			case nameof(IncludeBotMessages):
				IncludeBotMessages = bool.Parse(value);
				break;
			case nameof(IncludeSystemMessages):
				IncludeSystemMessages = bool.Parse(value);
				break;
			case nameof(MinMessageLength):
				MinMessageLength = int.Parse(value);
				break;
			case nameof(BlockedUsers):
				BlockedUsers = value;
				break;
			case nameof(MaxQueueSize):
				MaxQueueSize = int.Parse(value);
				break;
			case nameof(MaxReconnectAttempts):
				MaxReconnectAttempts = int.Parse(value);
				break;
			case nameof(EnableRichEmbeds):
				EnableRichEmbeds = bool.Parse(value);
				break;
			case nameof(Enabled):
				Enabled = bool.Parse(value);
				break;
			default:
				throw new NotImplementedException($"Unable to set value for key '{key}'");
		}
	}

	/// <summary>
	/// Updates this instance with values from another configuration instance
	/// </summary>
	/// <param name="source">The source configuration to copy from</param>
	protected override void UpdateFromConfiguration(DiscordConfiguration source)
	{
		BotToken = source.BotToken;
		GuildId = source.GuildId;
		ChannelId = source.ChannelId;
		GuildName = source.GuildName;
		ChannelName = source.ChannelName;
		IncludeBotMessages = source.IncludeBotMessages;
		IncludeSystemMessages = source.IncludeSystemMessages;
		MinMessageLength = source.MinMessageLength;
		BlockedUsers = source.BlockedUsers;
		MaxQueueSize = source.MaxQueueSize;
		MaxReconnectAttempts = source.MaxReconnectAttempts;
		EnableRichEmbeds = source.EnableRichEmbeds;
		Enabled = source.Enabled;
	}

	/// <summary>
	/// Get blocked users list as array
	/// </summary>
	/// <returns>Array of user IDs to block</returns>
	public string[] GetBlockedUsersList() => 
		BlockedUsers?.Split(',', StringSplitOptions.RemoveEmptyEntries)
			.Select(s => s.Trim())
			.Where(s => !string.IsNullOrEmpty(s))
			.ToArray() ?? Array.Empty<string>();
}