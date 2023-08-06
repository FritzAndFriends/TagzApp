﻿namespace TagzApp.Providers.TwitchChat;

public interface IChatClient : IDisposable
{
	event EventHandler<NewMessageEventArgs> NewMessage;

	void Init();
}