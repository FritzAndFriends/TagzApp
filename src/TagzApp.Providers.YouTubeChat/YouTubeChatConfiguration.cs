namespace TagzApp.Providers.YouTubeChat;

public class YouTubeChatConfiguration
{

	public const string AppSettingsSection = "providers:youtubechat";

	public const string Key_Google_ClientId = "Authentication:Google:ClientId";
	public const string Key_Google_ClientSecret = "Authentication:Google:ClientSecret";

	public const string Scope_YouTube = "https://www.googleapis.com/auth/youtube";

	public string ClientId { get; set; }

	public string ClientSecret { get; set; }

}
