using TagzApp.Common.Models;

namespace TagzApp.Web.Services;

public interface INotifyNewMessages
{
	void NotifyNewContent(string hashtag, Content content);

	void NotifyApprovedContent(string hashtag, Content content, ModerationAction action);

	void NotifyRejectedContent(string hashtag, Content content, ModerationAction action);

}