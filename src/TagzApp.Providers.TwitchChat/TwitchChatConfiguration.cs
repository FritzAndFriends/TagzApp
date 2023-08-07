namespace TagzApp.Providers.TwitchChat;

public class TwitchChatConfiguration
{

	public string ClientId { get; set; } = string.Empty;
	public string ClientSecret { get; set; } = string.Empty;
  public string ChatBotName { get; set; }
  public string OAuthToken { get; set; }

}