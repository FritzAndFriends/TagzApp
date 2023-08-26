using Microsoft.AspNetCore.SignalR;
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

	public void Notify(string hashtag, ContentModel content)
	{

		_HubContext.Clients
			.Group(hashtag)
			.SendAsync("NewMessage", content);

	}
}