namespace TagzApp.Providers.TwitchChat;

public class NewMessageEventArgs : EventArgs
{

	public required string DisplayName { get; set; }

	public required string UserName { get; set; }

	public required string Message { get; set; }

	public required string MessageId { get; set; }

	public string[] Badges { get; set; } = new string[0];

	public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.MinValue;

	public bool IsWhisper { get; set; } = false;

}
