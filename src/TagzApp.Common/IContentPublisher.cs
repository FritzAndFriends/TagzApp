namespace TagzApp.Common;

public interface IContentPublisher
{

	Task PublishContentAsync(Hashtag tag, Content newContent);

}

public enum ModerationState
{

	NotReviewed = 0,
	Approved,
	Rejected

}