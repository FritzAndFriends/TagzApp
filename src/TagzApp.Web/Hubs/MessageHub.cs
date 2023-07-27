using Microsoft.AspNetCore.SignalR;
using TagzApp.Web.Data;
using TagzApp.Web.Services;

namespace TagzApp.Web.Hubs;

public class MessageHub : Hub
{
  private readonly InMemoryMessagingService _Service;

  public MessageHub(InMemoryMessagingService svc)
  {
    _Service = svc;
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

		await base.OnConnectedAsync();

  }

	internal static string FormatOverlayGroupname(string overlayForTag)
	{
		return "overlay_" + overlayForTag.TrimStart('#').ToLowerInvariant();
	}

	public IEnumerable<ContentModel> GetExistingContentForTag(string tag)
  {

    return _Service.GetExistingContentForTag(tag)
      .Select(c => (ContentModel)c)
      .ToArray();

  }

	public void SendMessageToOverlay(string tag, string provider, string providerId)
	{

		var formattedTag = Hashtag.ClearFormatting(tag);
		var message = _Service.Content[formattedTag].FirstOrDefault(c => c.Provider == provider && c.ProviderId == providerId);

		if (message is null) return;

		Clients.Group(FormatOverlayGroupname(formattedTag))
			.SendAsync("DisplayOverlay", (ContentModel)message);

	}

}
