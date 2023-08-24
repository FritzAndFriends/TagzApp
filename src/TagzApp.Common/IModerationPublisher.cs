namespace TagzApp.Common;

public interface IModerationPublisher
{

	Task ModerateContentAsync(Content newContent, ModerationState state, string user, DateTimeOffset dateStamp);

}
