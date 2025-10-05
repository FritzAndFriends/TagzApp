namespace TagzApp.Providers.Kick;

public class NewMessageEventArgs : EventArgs
{
	public required string DisplayName { get; set; }

	public required string UserName { get; set; }

	public required string Message { get; set; }

	public required string MessageId { get; set; }

	public string[] Badges { get; set; } = [];

	public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.MinValue;

	public bool IsWhisper { get; set; } = false;

	public Emote[] Emotes { get; set; } = [];
}