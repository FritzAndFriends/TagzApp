namespace TagzApp.Providers.TwitchChat;

public class TwitchChatConfiguration
{
	public string ClientId { get; set; } = string.Empty;
	public string ClientSecret { get; set; } = string.Empty;
	public required string ChatBotName { get; set; }
	public required string OAuthToken { get; set; }
	public required string ChannelName { get; set; } = "csharpfritz";
}
