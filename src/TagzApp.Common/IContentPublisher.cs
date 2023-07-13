namespace TagzApp.Common;

public interface IContentPublisher
{

	Task PublishContentAsync(Hashtag tag, Content newContent);

}
