using System.ComponentModel;

namespace TagzApp.Providers.TwitchChat;

public class TwitchChatConfigurationViewModel : IProviderConfigurationViewModel
{
	[DisplayName("Channel Name")]
	public string ChannelName { get; set; }
	public bool Activated { get; set; }
}
