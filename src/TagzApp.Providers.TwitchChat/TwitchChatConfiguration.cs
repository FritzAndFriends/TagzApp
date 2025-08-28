using System.Text.Json.Serialization;
using TagzApp.Common;

namespace TagzApp.Providers.TwitchChat;

public class TwitchChatConfiguration : BaseProviderConfiguration<TwitchChatConfiguration>
{
	/// <summary>
	/// The configuration key used to store this configuration in the TagzApp configuration system
	/// </summary>
	protected override string ConfigurationKey => "provider-twitch";

	[JsonPropertyOrder(1)]
	public string ClientId { get; set; } = string.Empty;

	[JsonPropertyOrder(2)]
	public string ClientSecret { get; set; } = string.Empty;

	[JsonPropertyOrder(3)]
	public string ChatBotName { get; set; } = string.Empty;

	[JsonPropertyOrder(4)]
	public string OAuthToken { get; set; } = string.Empty;

	[JsonPropertyOrder(5)]
	public string ChannelName { get; set; } = "csharpfritz";

	public static TwitchChatConfiguration Empty => new()
	{
		ChatBotName = string.Empty,
		OAuthToken = string.Empty,
		ChannelName = string.Empty
	};

	[JsonIgnore]
	public override string Name => "TwitchChat";

	[JsonIgnore]
	public override string Description => "Read all messages from a specified Twitch channel";
	
	public override bool Enabled { get; set; }

	[JsonIgnore]
	public override string[] Keys => ["ClientId", "ClientSecret", "ChatBotName", "OAuthToken", "ChannelName"];

	public override string GetConfigurationByKey(string key)
	{
		return key switch
		{
			"ChannelName" => ChannelName,
			"ChatBotName" => ChatBotName,
			"ClientId" => ClientId,
			"ClientSecret" => ClientSecret,
			"OAuthToken" => OAuthToken,
			"Enabled" => Enabled.ToString(),
			_ => string.Empty
		};
	}

	public override void SetConfigurationByKey(string key, string value)
	{
		switch (key)
		{
			case "ChannelName":
				ChannelName = value;
				break;
			case "ChatBotName":
				ChatBotName = value;
				break;
			case "ClientId":
				ClientId = value;
				break;
			case "ClientSecret":
				ClientSecret = value;
				break;
			case "OAuthToken":
				OAuthToken = value;
				break;
			case "Enabled":
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
	protected override void UpdateFromConfiguration(TwitchChatConfiguration source)
	{
		ClientId = source.ClientId;
		ClientSecret = source.ClientSecret;
		ChatBotName = source.ChatBotName;
		OAuthToken = source.OAuthToken;
		ChannelName = source.ChannelName;
		Enabled = source.Enabled;
	}

	/// <summary>
	/// Public method to update configuration from another instance
	/// </summary>
	/// <param name="source">The source configuration to copy from</param>
	public void UpdateFrom(TwitchChatConfiguration source)
	{
		UpdateFromConfiguration(source);
	}

	/// <summary>
	/// Gets the configuration key used by this configuration type
	/// </summary>
	internal new static string GetConfigurationKey() => "provider-twitch";
}
