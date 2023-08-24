namespace TagzApp.Common;

public interface IModerationSubscriber
{

	void SubscribeToModerationAsync(Hashtag tag, Action<ModeratedContent> onNewContent);

}

public class ModeratedContent : Content
{

	public ModerationState State { get; set; }

	public string UserId { get; set; }

	public DateTimeOffset Timestamp { get; set; }

}