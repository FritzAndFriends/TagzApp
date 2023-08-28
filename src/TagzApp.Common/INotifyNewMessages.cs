using TagzApp.Common.Models;

namespace TagzApp.Web.Services;

public interface INotifyNewMessages
{
	void Notify(string hashtag, Content content);
}