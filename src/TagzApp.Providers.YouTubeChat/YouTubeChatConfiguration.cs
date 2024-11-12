namespace TagzApp.Providers.YouTubeChat;

public class YouTubeChatConfiguration : IProviderConfiguration
{

	public const string AppSettingsSection = "providers:youtubechat";

	public const string Key_Google_ClientId = "Authentication:Google:ClientId";
	public const string Key_Google_ClientSecret = "Authentication:Google:ClientSecret";

	public const string Scope_YouTube = "https://www.googleapis.com/auth/youtube";

	public string Name => "YouTubeChat";
	public string Description => "Listen to messages in YouTube LiveChat for a Live Stream";
	public bool Enabled { get; set; }

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

	public string[] Keys => ["ChannelTitle", "ChannelEmail","BroadcastId","BroadcastTitle","LiveChatId","RefreshToken"];

	public string GetConfigurationByKey(string key)
	{
		return key switch
		{
			"ChannelTitle" => ChannelTitle,
			"ChannelEmail" => ChannelEmail,
			"BroadcastId" => BroadcastId,
			"BroadcastTitle" => BroadcastTitle,
			"LiveChatId" => LiveChatId,
			"RefreshToken" => RefreshToken,
			"Enabled" => Enabled.ToString(),
			_ => string.Empty
		};
	}

	public void SetConfigurationByKey(string key, string value)
	{

		switch (key)
		{
			case "ChannelTitle":
				ChannelTitle = value;
				break;
			case "ChannelEmail":
				ChannelEmail = value;
				break;
			case "BroadcastId":
				BroadcastId = value;
				break;
			case "BroadcastTitle":
				BroadcastTitle = value;
				break;
			case "LiveChatId":
				LiveChatId = value;
				break;
			case "RefreshToken":
				RefreshToken = value;
				break;
			case "Enabled":
				Enabled = bool.Parse(value);
				break;
			default:
				throw new NotImplementedException();

		}

	}
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
