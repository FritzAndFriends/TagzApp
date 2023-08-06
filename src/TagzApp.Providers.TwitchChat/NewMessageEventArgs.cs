﻿namespace TagzApp.Providers.TwitchChat;

public class NewMessageEventArgs : EventArgs
{

	public string UserName { get; set; }

	public string Message { get; set; }

	public string[] Badges { get; set; }

	public bool IsWhisper { get; set; } = false;

}
}