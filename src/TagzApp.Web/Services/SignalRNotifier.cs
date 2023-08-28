using Microsoft.AspNetCore.SignalR;
using TagzApp.Common.Models;
using TagzApp.Web.Data;
using TagzApp.Web.Hubs;

namespace TagzApp.Web.Services;

public class SignalRNotifier : INotifyNewMessages
{
	private readonly IHubContext<MessageHub> _HubContext;

	public SignalRNotifier(IHubContext<MessageHub> hubContext)
	{
		_HubContext = hubContext;
	}

	public void Notify(string hashtag, Content content)
	{

		_HubContext.Clients
			.Group(hashtag)
			.SendAsync("NewMessage", (ContentModel)content);

	}

	public void NotifyApprovedContent(string hashtag, Content content)
	{

		_HubContext.Clients
			.Group(hashtag)
			.SendAsync("NewApprovedMessage", (ContentModel)content);

		Notify(hashtag, content);
	}

	public void NotifyRejectedContent(string hashtag, Content content)
	{
		_HubContext.Clients
			.Group(hashtag)
			.SendAsync("NewRejectedMessage", (ContentModel)content);

		Notify(hashtag, content);

	}
}