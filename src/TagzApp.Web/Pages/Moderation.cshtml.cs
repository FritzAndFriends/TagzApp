using Microsoft.AspNetCore.Mvc.RazorPages;
using TagzApp.Web.Services;

namespace TagzApp.Web.Pages;

public class ModerationModel(IMessagingService _Service, IModerationRepository _ModerationRepository) : PageModel
{
	public List<string> Tags { get; } = new List<string>();

	public IEnumerable<ISocialMediaProvider> Providers { get; set; }

	public int BlockedUserCount { get; set; } = 0;

	public async Task OnGet()
	{

		Tags.AddRange(_Service.TagsTracked);

		BlockedUserCount = await _ModerationRepository.GetCurrentBlockedUserCount();

		Providers = _Service.Providers.OrderBy(x => x.DisplayName);

		var activeStatus = new[] { SocialMediaStatus.Healthy, SocialMediaStatus.Degraded, SocialMediaStatus.Unhealthy };

		Providers = Providers.Where(x => activeStatus.Any(a => a == x.GetHealth().GetAwaiter().GetResult().Status))
			.ToArray();

	}
}
