namespace TagzApp.Providers.YouTubeChat;

public class YouTubeChatConfiguration : IProviderConfiguration
{

	public const string AppSettingsSection = "providers:youtubechat";

	public const string Key_Google_ClientId = "Authentication:Google:ClientId";
	public const string Key_Google_ClientSecret = "Authentication:Google:ClientSecret";

	public const string Scope_YouTube = "https://www.googleapis.com/auth/youtube";

	public string ClientId { get; set; }

	public string ClientSecret { get; set; }
	public string Name => "YouTubeChat";
	public string Description => "Listen to messages in YouTube LiveChat for a Live Stream";
	public bool Enabled { get; set; }
	public string[] Keys => ["ClientId", "ClientSecret"];

	public string GetConfigurationByKey(string key)
	{
		return key switch
		{
			"ClientId" => ClientId,
			"ClientSecret" => ClientSecret,
			_ => string.Empty
		};
	}

	public void SetConfigurationByKey(string key, string value)
	{

		switch (key)
		{
			case "ClientId":
				ClientId = value;
				break;
			case "ClientSecret":
				ClientSecret = value;
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
