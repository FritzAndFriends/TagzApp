namespace TagzApp.Providers.TwitchChat;

public class TwitchChatConfiguration : IProviderConfiguration
{
	public string ChatBotName { get; set; } = string.Empty;
	public string OAuthToken { get; set; } = string.Empty;
	public string ChannelName { get; set; } = "csharpfritz";
	public string Description => "Read all messages from a specified Twitch channel";

	public static TwitchChatConfiguration Empty => new()
	{
		ChatBotName = string.Empty,
		OAuthToken = string.Empty,
		ChannelName = string.Empty
	};

	public string Name => "TwitchChat";
	public bool Enabled { get; set; }
	public string[] Keys => ["ClientId", "ClientSecret", "ChatBotName", "OAuthToken", "ChannelName"];

	public string ClientId { get; internal set; }
	public string ClientSecret { get; internal set; }

	public string GetConfigurationByKey(string key)
	{
		return key switch
		{
			"ChannelName" => ChannelName,
			"ChatBotName" => ChatBotName,
			"ClientId" => ClientId,
			"ClientSecret" => ClientSecret,
			"OAuthToken" => OAuthToken,
			_ => string.Empty
		};
	}

	public void SetConfigurationByKey(string key, string value)
	{
		switch (key)
		{
			case "ChannelName":
				ChannelName = value;
				break;
			case "ChatBotName":
				ChatBotName = value;
				break;
			case "ClientId":
				ClientId = value;
				break;
			case "ClientSecret":
				ClientSecret = value;
				break;
			case "OAuthToken":
				OAuthToken = value;
				break;
			default:
				throw new NotImplementedException();

		}
	}
}
