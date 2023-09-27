using Microsoft.Extensions.DependencyInjection;
using TagzApp.Providers.Youtube.Configuration;
using TagzApp.Communication;

namespace TagzApp.Providers.Youtube;

public class StartYoutube : BaseConfigurationProvider, IConfigureProvider
{
	private const string _DisplayName = "Youtube";
	private YoutubeConfiguration? _YoutubeConfiguration;

	public StartYoutube(IProviderConfigurationRepository providerConfigurationRepository)
		: base(providerConfigurationRepository)
	{
	}

	public async Task<IServiceCollection> RegisterServices(IServiceCollection services, CancellationToken cancellationToken)
	{
		await LoadConfigurationValuesAsync(_DisplayName, cancellationToken);

		if (string.IsNullOrEmpty(_YoutubeConfiguration?.ApiKey))
		{
			// No configuration provided, no registration to be added
			return services;
		}

		services.AddSingleton(_YoutubeConfiguration);
		services.AddTransient<ISocialMediaProvider, YoutubeProvider>();
		return services;
	}

	protected override void MapConfigurationValues(ProviderConfiguration providerConfiguration)
	{
		var config = providerConfiguration.ConfigurationSettings;

		if (config != null)
		{
			_YoutubeConfiguration = new YoutubeConfiguration
			{
				ApiKey = config["ApiKey"] ?? string.Empty,
			};
		}
	}
}
