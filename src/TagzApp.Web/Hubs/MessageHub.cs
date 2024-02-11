using Microsoft.AspNetCore.SignalR;
using TagzApp.ViewModels.Data;


namespace TagzApp.Web.Hubs;

public class MessageHub : Hub
{

	private readonly IMessagingService _Service;
	private bool ModerationEnabled = false;

	public MessageHub(IMessagingService svc, ApplicationConfiguration configuration)
	{
		_Service = svc;
		ModerationEnabled = configuration.ModerationEnabled;
	}

	public override async Task OnConnectedAsync()
	{
		var tags = Context.GetHttpContext()?.Request.Query["t"].ToString();
		if (!string.IsNullOrEmpty(tags))
		{
			var tagsRequested = tags.Split(',');
			foreach (var tag in tagsRequested)
			{
				await Groups.AddToGroupAsync(Context.ConnectionId, tag.TrimStart('#').ToLowerInvariant());
			}
		}

		var overlay = Context.GetHttpContext()?.Request.Query["ot"].ToString();
		if (!string.IsNullOrEmpty(overlay))
		{
			await Groups.AddToGroupAsync(Context.ConnectionId, FormatOverlayGroupname(overlay));
		}
		else
		{
			// get the default tag and watch that
			var tag = _Service.TagsTracked.FirstOrDefault();
			if (tag != null)
			{
				System.Console.WriteLine($"Default tag: {tag}");
				await Groups.AddToGroupAsync(Context.ConnectionId, FormatOverlayGroupname(tag));
			}
		}

		await base.OnConnectedAsync();
	}

	internal static string FormatOverlayGroupname(string overlayForTag)
	{
		return "overlay_" + overlayForTag.TrimStart('#').ToLowerInvariant();
	}

	public async Task<IEnumerable<ContentModel>> GetExistingContentForTag(string tag)
	{

		if (ModerationEnabled)
		{
			return (await _Service.GetApprovedContentByTag(tag))
			.Select(c => (ContentModel)c)
			.ToArray();
		}

		return (await _Service.GetExistingContentForTag(tag))
			.Select(c => (ContentModel)c)
			.ToArray();

	}

	public async Task SendMessageToOverlay(string tag, string provider, string providerId)
	{

		var formattedTag = Hashtag.ClearFormatting(tag);
		var message = await _Service.GetContentByIds(provider, providerId);

		if (message is null) return;

		System.Console.WriteLine($"SendMessageToOverlay: {tag} {provider} {providerId}");

		await Clients.User(Context.UserIdentifier)
			.SendAsync("DisplayOverlay", (ContentModel)message);
	}

}
