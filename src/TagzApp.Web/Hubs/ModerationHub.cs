using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using TagzApp.Web.Data;
using TagzApp.Web.Services;

namespace TagzApp.Web.Hubs;

[Authorize(Policy = Security.Policy.Moderator)]
public class ModerationHub : Hub<IModerationClient>
{

	private readonly IMessagingService _Service;
	private readonly IModerationRepository _Repository;
	private readonly UserManager<IdentityUser> _UserManager;
	private bool ModerationEnabled = false;

	public ModerationHub(IMessagingService svc, IConfiguration configuration, IModerationRepository repository, UserManager<IdentityUser> userManager)
	{
		_Service = svc;
		_Repository = repository;
		_UserManager = userManager;
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

	public async Task SetStatus(string provider, string providerId, ModerationState newState)
	{

		if (!ModerationEnabled || Context.User is null)
		{
			return;
		}

		var thisUser = await _UserManager.GetUserAsync(Context.User);
		await _Repository.Moderate(thisUser?.NormalizedUserName ?? thisUser.Email, provider, providerId, newState);

	}

}

public interface IModerationClient
{

	Task NewWaterfallMessage(ContentModel model);

	Task NewApprovedMessage(ModerationContentModel model);

	Task NewRejectedMessage(ModerationContentModel model);

}