using Microsoft.AspNetCore.Mvc.RazorPages;
using TagzApp.Web.Services;

namespace TagzApp.Web.Pages;

public class ModerationModel : PageModel
{
	private readonly IMessagingService _Service;
	private readonly IProviderConfigurationRepository _ProviderConfigurationRepository;

	public ModerationModel(IMessagingService service, IProviderConfigurationRepository providerConfigurationRepository)
	{
		_Service = service;
		_ProviderConfigurationRepository = providerConfigurationRepository;
	}

	public List<string> Tags { get; } = new List<string>();

	public IEnumerable<ISocialMediaProvider> Providers { get; set; }

	public async Task OnGet()
	{

		Tags.AddRange(_Service.TagsTracked);

		Providers = _Service.Providers.OrderBy(x => x.DisplayName);
		var providerConfigs = await _ProviderConfigurationRepository.GetConfigurationSettingsAsync();

		Providers = Providers.Where(x =>
				providerConfigs.FirstOrDefault(y => y.Name.Equals(x.DisplayName, StringComparison.InvariantCultureIgnoreCase))?.Activated ?? true)
			.ToArray();

	}
}
