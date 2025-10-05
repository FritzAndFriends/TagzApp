using System.Text.Json.Serialization;

namespace TagzApp.Providers.Discord;

/// <summary>
/// Represents a Discord message
/// </summary>
public class DiscordMessage
{
	[JsonPropertyName("id")]
	public string Id { get; set; } = string.Empty;

	[JsonPropertyName("channel_id")]
	public string ChannelId { get; set; } = string.Empty;

	[JsonPropertyName("guild_id")]
	public string? GuildId { get; set; }

	[JsonPropertyName("content")]
	public string Content { get; set; } = string.Empty;

	[JsonPropertyName("timestamp")]
	public DateTimeOffset Timestamp { get; set; }

	[JsonPropertyName("edited_timestamp")]
	public DateTimeOffset? EditedTimestamp { get; set; }

	[JsonPropertyName("author")]
	public DiscordUser Author { get; set; } = new();

	[JsonPropertyName("type")]
	public DiscordMessageType Type { get; set; }

	[JsonPropertyName("embeds")]
	public DiscordEmbed[] Embeds { get; set; } = Array.Empty<DiscordEmbed>();

	[JsonPropertyName("attachments")]
	public DiscordAttachment[] Attachments { get; set; } = Array.Empty<DiscordAttachment>();

	[JsonPropertyName("message_reference")]
	public DiscordMessageReference? MessageReference { get; set; }

	[JsonPropertyName("pinned")]
	public bool Pinned { get; set; }

	[JsonPropertyName("mention_everyone")]
	public bool MentionEveryone { get; set; }
}

/// <summary>
/// Represents a Discord user
/// </summary>
public class DiscordUser
{
	[JsonPropertyName("id")]
	public string Id { get; set; } = string.Empty;

	[JsonPropertyName("username")]
	public string Username { get; set; } = string.Empty;

	[JsonPropertyName("discriminator")]
	public string Discriminator { get; set; } = string.Empty;

	[JsonPropertyName("global_name")]
	public string? GlobalName { get; set; }

	[JsonPropertyName("avatar")]
	public string? Avatar { get; set; }

	[JsonPropertyName("bot")]
	public bool Bot { get; set; }

	[JsonPropertyName("system")]
	public bool System { get; set; }

	/// <summary>
	/// Display name (global name or username)
	/// </summary>
	public string DisplayName => GlobalName ?? Username;

	/// <summary>
	/// Full username with discriminator if not 0
	/// </summary>
	public string FullUsername => Discriminator == "0" ? Username : $"{Username}#{Discriminator}";

	/// <summary>
	/// Avatar URL with fallback to default
	/// </summary>
	public string AvatarUrl => !string.IsNullOrEmpty(Avatar) ? 
		$"https://cdn.discordapp.com/avatars/{Id}/{Avatar}.png?size=128" : 
		$"https://cdn.discordapp.com/embed/avatars/{(string.IsNullOrEmpty(Discriminator) || Discriminator == "0" ? 0 : int.Parse(Discriminator) % 5)}.png";
}

/// <summary>
/// Represents a Discord embed
/// </summary>
public class DiscordEmbed
{
	[JsonPropertyName("title")]
	public string? Title { get; set; }

	[JsonPropertyName("description")]
	public string? Description { get; set; }

	[JsonPropertyName("url")]
	public string? Url { get; set; }

	[JsonPropertyName("timestamp")]
	public DateTimeOffset? Timestamp { get; set; }

	[JsonPropertyName("color")]
	public int Color { get; set; }

	[JsonPropertyName("author")]
	public DiscordEmbedAuthor? Author { get; set; }

	[JsonPropertyName("fields")]
	public DiscordEmbedField[] Fields { get; set; } = Array.Empty<DiscordEmbedField>();
}

/// <summary>
/// Represents a Discord embed author
/// </summary>
public class DiscordEmbedAuthor
{
	[JsonPropertyName("name")]
	public string? Name { get; set; }

	[JsonPropertyName("url")]
	public string? Url { get; set; }

	[JsonPropertyName("icon_url")]
	public string? IconUrl { get; set; }
}

/// <summary>
/// Represents a Discord embed field
/// </summary>
public class DiscordEmbedField
{
	[JsonPropertyName("name")]
	public string Name { get; set; } = string.Empty;

	[JsonPropertyName("value")]
	public string Value { get; set; } = string.Empty;

	[JsonPropertyName("inline")]
	public bool Inline { get; set; }
}

/// <summary>
/// Represents a Discord attachment
/// </summary>
public class DiscordAttachment
{
	[JsonPropertyName("id")]
	public string Id { get; set; } = string.Empty;

	[JsonPropertyName("filename")]
	public string Filename { get; set; } = string.Empty;

	[JsonPropertyName("url")]
	public string Url { get; set; } = string.Empty;

	[JsonPropertyName("proxy_url")]
	public string ProxyUrl { get; set; } = string.Empty;

	[JsonPropertyName("size")]
	public int Size { get; set; }

	[JsonPropertyName("width")]
	public int? Width { get; set; }

	[JsonPropertyName("height")]
	public int? Height { get; set; }

	[JsonPropertyName("content_type")]
	public string? ContentType { get; set; }
}

/// <summary>
/// Represents a Discord message reference (reply)
/// </summary>
public class DiscordMessageReference
{
	[JsonPropertyName("message_id")]
	public string? MessageId { get; set; }

	[JsonPropertyName("channel_id")]
	public string? ChannelId { get; set; }

	[JsonPropertyName("guild_id")]
	public string? GuildId { get; set; }
}

/// <summary>
/// Discord message types
/// </summary>
public enum DiscordMessageType
{
	Default = 0,
	RecipientAdd = 1,
	RecipientRemove = 2,
	Call = 3,
	ChannelNameChange = 4,
	ChannelIconChange = 5,
	ChannelPinnedMessage = 6,
	GuildMemberJoin = 7,
	UserPremiumGuildSubscription = 8,
	UserPremiumGuildSubscriptionTier1 = 9,
	UserPremiumGuildSubscriptionTier2 = 10,
	UserPremiumGuildSubscriptionTier3 = 11,
	ChannelFollowAdd = 12,
	GuildDiscoveryDisqualified = 14,
	GuildDiscoveryRequalified = 15,
	GuildDiscoveryGracePeriodInitialWarning = 16,
	GuildDiscoveryGracePeriodFinalWarning = 17,
	ThreadCreated = 18,
	Reply = 19,
	ChatInputCommand = 20,
	ThreadStarterMessage = 21,
	GuildInviteReminder = 22,
	ContextMenuCommand = 23,
	AutoModerationAction = 24
}

/// <summary>
/// Discord Gateway payload wrapper
/// </summary>
public class DiscordGatewayPayload
{
	[JsonPropertyName("op")]
	public int Op { get; set; }

	[JsonPropertyName("d")]
	public object? Data { get; set; }

	[JsonPropertyName("s")]
	public int? Sequence { get; set; }

	[JsonPropertyName("t")]
	public string? Type { get; set; }
}

/// <summary>
/// Discord Gateway Hello payload
/// </summary>
public class DiscordHelloPayload
{
	[JsonPropertyName("heartbeat_interval")]
	public int HeartbeatInterval { get; set; }
}

/// <summary>
/// Discord Gateway Identify payload
/// </summary>
public class DiscordIdentifyPayload
{
	[JsonPropertyName("token")]
	public string Token { get; set; } = string.Empty;

	[JsonPropertyName("intents")]
	public int Intents { get; set; }

	[JsonPropertyName("properties")]
	public DiscordConnectionProperties Properties { get; set; } = new();
}

/// <summary>
/// Discord connection properties
/// </summary>
public class DiscordConnectionProperties
{
	[JsonPropertyName("$os")]
	public string Os { get; set; } = Environment.OSVersion.Platform.ToString();

	[JsonPropertyName("$browser")]
	public string Browser { get; set; } = "TagzApp";

	[JsonPropertyName("$device")]
	public string Device { get; set; } = "TagzApp";
}

/// <summary>
/// Discord Gateway opcodes
/// </summary>
public static class DiscordGatewayOpcodes
{
	public const int Dispatch = 0;
	public const int Heartbeat = 1;
	public const int Identify = 2;
	public const int PresenceUpdate = 3;
	public const int VoiceStateUpdate = 4;
	public const int Resume = 6;
	public const int Reconnect = 7;
	public const int RequestGuildMembers = 8;
	public const int InvalidSession = 9;
	public const int Hello = 10;
	public const int HeartbeatAck = 11;
}

/// <summary>
/// Discord Gateway intents
/// </summary>
public static class DiscordGatewayIntents
{
	public const int GuildMessages = 1 << 9;
	public const int MessageContent = 1 << 15;
}