using TagzApp.Web.Data;

namespace TagzApp.Web.Services;

public interface INotifyNewMessages
{
	void Notify(string hashtag, ContentModel content);
}