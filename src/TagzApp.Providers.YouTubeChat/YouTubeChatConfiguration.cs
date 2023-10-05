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


public class YouTubeChatApplicationConfiguration
{

	/// <summary>
	/// Title of the YouTube Channel we are monitoring
	/// </summary>
	/// <value></value>
	public string ChannelTitle { get; set; } = string.Empty;

	/// <summary>
	/// Email used to authenticate with YouTube
	/// </summary>
	/// <value></value>
	public string ChannelEmail { get; set; } = string.Empty;

	/// <summary>
	/// Id of the Broadcast we are monitoring
	/// </summary>
	/// <value></value>
	public string BroadcastId { get; set; } = string.Empty;

	/// <summary>
	/// Title of the Broadcast we are monitoring
	/// </summary>
	/// <value></value>
	public string BroadcastTitle { get; set; } = string.Empty;

	/// <summary>
	/// Id of the LiveChat we are monitoring
	/// </summary>
	/// <value></value>
	public string LiveChatId { get; set; } = string.Empty;

	/// <summary>
	/// Token used to refresh the access token
	/// </summary>
	/// <value></value>
	public string RefreshToken { get; set; } = string.Empty;

}
