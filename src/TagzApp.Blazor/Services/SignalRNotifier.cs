using Microsoft.AspNetCore.SignalR;
using TagzApp.Blazor.Hubs;
using TagzApp.ViewModels.Data;

namespace TagzApp.Blazor.Services;

public class SignalRNotifier : INotifyNewMessages
{

	private readonly IHubContext<MessageHub> _HubContext;
	private readonly IHubContext<ModerationHub, IModerationClient> _ModerationService;
	private bool _ModerationEnabled = false;

	public SignalRNotifier(IHubContext<MessageHub> hubContext, IHubContext<ModerationHub, IModerationClient> moderationService, ApplicationConfiguration appConfiguration)
	{
		_HubContext = hubContext;
		_ModerationService = moderationService;
		_ModerationEnabled = appConfiguration.ModerationEnabled;
	}

	public void NotifyNewContent(string hashtag, Content content)
	{

		if (_ModerationEnabled)
		{

			_ModerationService.Clients.All.NewWaterfallMessage((ContentModel)content);

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

		_ModerationService.Clients.All.NewApprovedMessage(ModerationContentModel.ToModerationContentModel(content, action));

		Console.WriteLine($"Sending new waterfall message for tag {hashtag} for content: {content.Text}");

		_HubContext.Clients
			.Group(hashtag)
			.SendAsync("NewWaterfallMessage", (ContentModel)content);

	}

	public void NotifyRejectedContent(string hashtag, Content content, ModerationAction action)
	{

		_HubContext.Clients
			.Group(hashtag)
			.SendAsync("RemoveMessage", content.Provider, content.ProviderId);

		_ModerationService.Clients.All.NewRejectedMessage(ModerationContentModel.ToModerationContentModel(content, action));

	}

	public void NotifyNewBlockedCount(int blockedCount)
	{
		_ModerationService.Clients.All.NewBlockedUserCount(blockedCount);
	}
}
