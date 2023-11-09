using Microsoft.AspNetCore.Mvc.RazorPages;
using TagzApp.Web.Services;

namespace TagzApp.Web.Pages;

public class ModerationModel : PageModel
{
	private readonly IMessagingService _Service;
	private readonly IModerationRepository _ModerationRepository;
	private readonly IProviderConfigurationRepository _ProviderConfigurationRepository;

	public ModerationModel(IMessagingService service, IModerationRepository moderationRepository, IProviderConfigurationRepository providerConfigurationRepository)
	{
		_Service = service;
		_ModerationRepository = moderationRepository;
		_ProviderConfigurationRepository = providerConfigurationRepository;
	}

	public List<string> Tags { get; } = new List<string>();

	public IEnumerable<ISocialMediaProvider> Providers { get; set; }

	public int BlockedUserCount { get; set; } = 0;

	public async Task OnGet()
	{

		Tags.AddRange(_Service.TagsTracked);

		BlockedUserCount = await _ModerationRepository.GetCurrentBlockedUserCount();

		Providers = _Service.Providers.OrderBy(x => x.DisplayName);
		var providerConfigs = await _ProviderConfigurationRepository.GetConfigurationSettingsAsync();

		Providers = Providers.Where(x =>
				providerConfigs.FirstOrDefault(y => y.Name.Equals(x.DisplayName, StringComparison.InvariantCultureIgnoreCase))?.Activated ?? true)
			.ToArray();

	}
}
