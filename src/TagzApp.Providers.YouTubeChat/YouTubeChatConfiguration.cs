namespace TagzApp.Providers.YouTubeChat;

public class YouTubeChatConfiguration
{

	public const string AppSettingsSection = "providers:youtubechat";

	public required string ClientId { get; set; } = "foo";

	public required string ClientSecret { get; set; }

	public string BroadcastId { get; set; }


}
