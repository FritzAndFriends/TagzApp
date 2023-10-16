namespace TagzApp.Providers.TwitchChat;

public class TwitchChatConfiguration
{
	public string ClientId { get; set; } = string.Empty;
	public string ClientSecret { get; set; } = string.Empty;
	public required string ChatBotName { get; set; }
	public required string OAuthToken { get; set; }
	public required string ChannelName { get; set; } = "csharpfritz";
	public string Description { get; set; } = string.Empty;

	public static TwitchChatConfiguration Empty => new()
	{
		ClientId = string.Empty,
		ClientSecret = string.Empty,
		ChatBotName = string.Empty,
		OAuthToken = string.Empty,
		ChannelName = string.Empty
	};

}
