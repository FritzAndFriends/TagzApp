using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Primitives;
using TagzApp.Web.Services;

namespace TagzApp.Web.Areas.Admin.Pages
{
	public class ProvidersModel : PageModel
	{
		public IEnumerable<ISocialMediaProvider> Providers { get; set; }
		private readonly IMessagingService _Service;

		internal static readonly string[] PasswordEndings = ["token", "key", "secret"];

		public ProvidersModel(IMessagingService service)
		{
			_Service = service;
			Providers = service.Providers;
		}

		public async Task OnPost()
		{
			var submittedValues = Request.Form.ToList();
			var providerName = submittedValues.FirstOrDefault(x => x.Key == "Name").Value.ToString() ?? string.Empty;

			var provider = Providers.FirstOrDefault(p => p.DisplayName == providerName);
			var config = await provider?.GetConfiguration(ConfigureTagzAppFactory.Current);

			if (config != null)
			{
				config.Enabled = GetActivatedStatus(submittedValues);
				submittedValues.ForEach(value =>
				{
					if (
							value.Key != "Name" &&
							value.Key != "Enabled" &&
							value.Key != "Description" &&
							value.Key != "__RequestVerificationToken")
					{
						// String handling of boolean properties submitted from HTML checkbox input controls
						if (value.Value.ToString().StartsWith(bool.TrueString.ToLower())
							|| value.Value.ToString().StartsWith(bool.FalseString.ToLower()))
						{
							config.SetConfigurationByKey(value.Key, value.Value.ToString().Split(',')[0]);
						}
						else
						{
							//config.ConfigurationSettings[value.Key] = value.Value.ToString() ?? config.ConfigurationSettings[value.Key];
							config.SetConfigurationByKey(value.Key, value.Value);
						}
					}
				});
			}
			//else
			//{
			//	config = new Common.Models.IProviderConfiguration
			//	{
			//		Name = submittedValues.FirstOrDefault(x => x.Key == "Name").Value[0] ?? string.Empty,
			//		Activated = GetActivatedStatus(submittedValues),
			//		ConfigurationSettings = new Dictionary<string, string>()
			//	};

			//	submittedValues.Where(x => x.Key != "Name" &&
			//												x.Key != "Activated" &&
			//												x.Key != "Description" &&
			//												x.Key != "__RequestVerificationToken").ToList()
			//		.ForEach(y =>
			//		{
			//			config.ConfigurationSettings.Add(y.Key, y.Value[0]!);
			//		});
			//}

			if (config != null)
			{
				await provider.SaveConfiguration(ConfigureTagzAppFactory.Current, config);
				await Program.Restart();
			}
		}

		private bool GetActivatedStatus(List<KeyValuePair<string, StringValues>>? values)
		{
			var activatedValues = values?.Where(x => x.Key.Contains("Enabled"));
			return activatedValues?.FirstOrDefault(x => x.Key == "Enabled").Value.ToString() == "on" ? true : false;
		}

		public static string GetClassForHealth(SocialMediaStatus status) => status switch
		{
			SocialMediaStatus.Healthy => "bi bi-check-circle-fill text-success",
			SocialMediaStatus.Degraded => "bi bi-exclamation-circle-fill text-warning",
			SocialMediaStatus.Unhealthy => "bi bi-x-circle-fill text-danger",
			_ => "bi bi text-primary"
		};

	}
}
