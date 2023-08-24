namespace TagzApp.Common;

public interface IContentSubscriber
{

	void SubscribeToContentAsync(Hashtag tag, Action<Content> onNewContent);

	bool HasSubscribers { get; }

	IEnumerable<string> TagsSubscribedTo { get; }

}
