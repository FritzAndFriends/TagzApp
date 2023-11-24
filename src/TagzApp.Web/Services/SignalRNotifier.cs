using Microsoft.AspNetCore.SignalR;
using TagzApp.Web.Data;
using TagzApp.Web.Hubs;

namespace TagzApp.Web.Services;

public class SignalRNotifier : INotifyNewMessages
{

	private readonly IHubContext<MessageHub> _HubContext;
	private readonly IHubContext<ModerationHub, IModerationClient> _ModContext;
	private bool _ModerationEnabled = false;

	public SignalRNotifier(IHubContext<MessageHub> hubContext, IHubContext<ModerationHub, IModerationClient> modContext, IConfiguration configuration)
	{
		_HubContext = hubContext;
		_ModContext = modContext;
		_ModerationEnabled = configuration.GetValue<bool>("ModerationEnabled", false);
	}

	public void NotifyNewContent(string hashtag, Content content)
	{

		if (_ModerationEnabled)
		{

			_ModContext.Clients
				.Group(hashtag)
				.NewWaterfallMessage((ContentModel)content);

		}
		else
		{

			_HubContext.Clients
				.Group(hashtag)
				.SendAsync("NewWaterfallMessage", (ContentModel)content);

		}

	}

	public void NotifyApprovedContent(string hashtag, Content content, ModerationAction action)
	{

		_ModContext.Clients
			.Group(hashtag)
			.NewApprovedMessage(ModerationContentModel.ToModerationContentModel(content, action));

		_HubContext.Clients
			.Group(hashtag)
			.SendAsync("NewWaterfallMessage", (ContentModel)content);

	}

	public void NotifyRejectedContent(string hashtag, Content content, ModerationAction action)
	{

		_HubContext.Clients
			.Group(hashtag)
			.SendAsync("RemoveMessage", content.Provider, content.ProviderId);

		_ModContext.Clients
			.Group(hashtag)
			.NewRejectedMessage(ModerationContentModel.ToModerationContentModel(content, action));

	}

	public void NotifyNewBlockedCount(int blockedCount)
	{
		_ModContext.Clients
			.All
			.NewBlockedUserCount(blockedCount);
	}
}
