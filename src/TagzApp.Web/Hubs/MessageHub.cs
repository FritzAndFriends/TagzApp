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

    await base.OnConnectedAsync();

  }

  public IEnumerable<ContentModel> GetExistingContentForTag(string tag)
  {

    return _Service.GetExistingContentForTag(tag)
      .Select(c => (ContentModel)c)
      .ToArray();

  }

}
