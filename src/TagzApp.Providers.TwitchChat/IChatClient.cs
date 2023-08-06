namespace TagzApp.Providers.TwitchChat;

public interface IChatClient
{
	event EventHandler<NewMessageEventArgs> NewMessage;

	void Init();
}