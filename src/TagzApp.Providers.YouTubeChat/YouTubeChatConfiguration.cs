namespace TagzApp.Providers.YouTubeChat;

public class YouTubeChatConfiguration
{

	public const string AppSettingsSection = "providers:youtubechat";

	public required string ApiKey { get; set; }

	public string BroadcastId { get; set; }


}
