using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using TagzApp.Web.Data;
using TagzApp.Web.Services;

namespace TagzApp.Web.Hubs;

[Authorize(Policy = Security.Policy.Moderator)]
public class ModerationHub : Hub<IModerationClient>
{

	private readonly IMessagingService _Service;
	private bool ModerationEnabled = false;

	public ModerationHub(IMessagingService svc, IConfiguration configuration)
	{
		_Service = svc;
		ModerationEnabled = configuration.GetValue<bool>("ModerationEnabled", false);
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

	public async Task<IEnumerable<ModerationContentModel>> GetContentForTag(string tag)
	{

		return (await _Service.GetContentByTagForModeration(tag))
			.Select(c => ModerationContentModel.ToModerationContentModel(c.Item1, c.Item2))
			.ToArray();

	}

}

public interface IModerationClient
{

	Task NewWaterfallMessage(ModerationContentModel model);

	Task NewApprovedMessage(ModerationContentModel model);

	Task NewRejectedMessage(ModerationContentModel model);

}