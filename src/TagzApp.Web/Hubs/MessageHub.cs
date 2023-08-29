﻿using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using TagzApp.Common.Models;
using TagzApp.Web.Data;
using TagzApp.Web.Services;

namespace TagzApp.Web.Hubs;

public class MessageHub : Hub
{
  private readonly IMessagingService _Service;

  public MessageHub(IMessagingService svc)
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

	public async Task<IEnumerable<ContentModel>> GetExistingContentForTag(string tag)
  {

    return (await _Service.GetExistingContentForTag(tag))
      .Select(c => (ContentModel)c)
      .ToArray();

  }

	public async Task SendMessageToOverlay(string tag, string provider, string providerId)
	{
		var formattedTag = Hashtag.ClearFormatting(tag);
		var message = await _Service.GetContentByIds(provider, providerId);

		if (message is null) return;

		await Clients.Group(FormatOverlayGroupname(formattedTag))
			.SendAsync("DisplayOverlay", (ContentModel)message);
	}

}

// TODO: set up authorization to get existing content for tag, for the waterfall
// docs at:  https://learn.microsoft.com/en-us/aspnet/core/signalr/authn-and-authz?view=aspnetcore-7.0

public class UserIdProvider : IUserIdProvider
{

	public UserIdProvider(UserManager<IdentityUser> userManager)
	{
		
	}

	public string? GetUserId(HubConnectionContext connection)
	{
		return "";
		//return connection.User?
	}
}