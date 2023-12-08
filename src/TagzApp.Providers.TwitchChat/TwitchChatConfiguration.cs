namespace TagzApp.Providers.TwitchChat;

public class TwitchChatConfiguration : IProviderConfiguration
{
	public string ClientId { get; set; } = string.Empty;
	public string ClientSecret { get; set; } = string.Empty;
	public string ChatBotName { get; set; } = string.Empty;
	public string OAuthToken { get; set; } = string.Empty;
	public string ChannelName { get; set; } = "csharpfritz";
	public string Description => "Read all messages from a specified Twitch channel";

	public static TwitchChatConfiguration Empty => new()
	{
		ClientId = string.Empty,
		ClientSecret = string.Empty,
		ChatBotName = string.Empty,
		OAuthToken = string.Empty,
		ChannelName = string.Empty
	};

	public string Name => "TwitchChat";
	public bool Enabled { get; set; }
}
