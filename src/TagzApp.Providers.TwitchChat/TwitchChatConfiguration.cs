namespace TagzApp.Providers.TwitchChat;

public class TwitchChatConfiguration : ISocialMediaProviderConfiguration
{
	public string ClientId { get; set; } = string.Empty;
	public string ClientSecret { get; set; } = string.Empty;
	public required string ChatBotName { get; set; }
	public required string OAuthToken { get; set; }
}