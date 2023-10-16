using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Primitives;
using TagzApp.Web.Services;

namespace TagzApp.Web.Areas.Admin.Pages
{
	public class ProvidersModel : PageModel
	{
		public IEnumerable<ISocialMediaProvider> Providers { get; set; }
		private readonly IMessagingService _Service;
		private readonly IProviderConfigurationRepository _ProviderConfigurationRepository;

		public ProvidersModel(IMessagingService service, IProviderConfigurationRepository providerConfigurationRepository)
		{
			_Service = service;
			_ProviderConfigurationRepository = providerConfigurationRepository;
			Providers = service.Providers;
		}

		public async Task OnPost()
		{
			var submittedValues = Request.Form.ToList();
			var providerName = submittedValues.FirstOrDefault(x => x.Key == "Name").Value.ToString() ?? string.Empty;

			var config = await _ProviderConfigurationRepository.GetConfigurationSettingsAsync(providerName);

			if (config != null)
			{
				config.Activated = GetActivatedStatus(submittedValues);
				submittedValues.ForEach(value =>
				{
					if (config.ConfigurationSettings != null &&
							value.Key != "Name" &&
							value.Key != "Activated" &&
							value.Key != "Description" &&
							value.Key != "__RequestVerificationToken")
					{
						// String handling of boolean properties submitted from HTML checkbox input controls
						if (value.Value.ToString().StartsWith(bool.TrueString.ToLower())
							|| value.Value.ToString().StartsWith(bool.FalseString.ToLower()))
						{
							config.ConfigurationSettings[value.Key] = value.Value.ToString().Split(',')[0];
						}
						else
						{
							config.ConfigurationSettings[value.Key] = value.Value.ToString() ?? config.ConfigurationSettings[value.Key];
						}
					}
				});
			}
			else
			{
				config = new ProviderConfiguration
				{
					Name = submittedValues.FirstOrDefault(x => x.Key == "Name").Value[0] ?? string.Empty,
					Activated = GetActivatedStatus(submittedValues),
					ConfigurationSettings = new Dictionary<string, string>()
				};

				submittedValues.Where(x => x.Key != "Name" &&
															x.Key != "Activated" &&
															x.Key != "Description" &&
															x.Key != "__RequestVerificationToken").ToList()
					.ForEach(y =>
					{
						config.ConfigurationSettings.Add(y.Key, y.Value[0]!);
					});
			}

			if (config != null)
				await _ProviderConfigurationRepository.SaveConfigurationSettingsAsync(config);
		}

		private bool GetActivatedStatus(List<KeyValuePair<string, StringValues>>? values)
		{
			var activatedValues = values?.Where(x => x.Key.Contains("Activated"));
			return activatedValues?.FirstOrDefault(x => x.Key == "Activated").Value.ToString() == "on" ? true : false;
		}
	}
}
