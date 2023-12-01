using Microsoft.Extensions.DependencyInjection;
using TagzApp.Providers.Youtube.Configuration;

namespace TagzApp.Providers.Youtube;

public class StartYoutube : IConfigureProvider
{
	private const string _DisplayName = "Youtube";
	private YoutubeConfiguration? _YoutubeConfiguration;

	public async Task<IServiceCollection> RegisterServices(IServiceCollection services, CancellationToken cancellationToken)
	{

		_YoutubeConfiguration = await ConfigureTagzAppFactory.Current.GetConfigurationById<YoutubeConfiguration>(YoutubeConfiguration.AppSettingsSection);

		if (string.IsNullOrEmpty(_YoutubeConfiguration?.ApiKey))
		{
			// No configuration provided, no registration to be added
			return services;
		}

		services.AddSingleton(_YoutubeConfiguration);
		services.AddTransient<ISocialMediaProvider, YoutubeProvider>();
		return services;
	}

}
