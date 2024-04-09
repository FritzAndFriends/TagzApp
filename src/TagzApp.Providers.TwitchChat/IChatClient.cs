namespace TagzApp.Providers.TwitchChat;

public interface IChatClient : IDisposable
{
	event EventHandler<NewMessageEventArgs> NewMessage;

	void Init();

	void Stop();

	bool IsRunning { get; }

	bool IsConnected { get; }

	void ListenToNewChannel(string channelName);

}
