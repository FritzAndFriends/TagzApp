using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TagzApp.Common.Exceptions;
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

	public IServiceCollection RegisterServices(IServiceCollection services, IConfiguration configuration)
	{

		var config = configuration.GetSection(YoutubeConfiguration.AppSettingsSection)!;
		if (config is null) throw new InvalidConfigurationException(YoutubeConfiguration.AppSettingsSection);
		services.Configure<YoutubeConfiguration>(config);

		services.AddTransient<ISocialMediaProvider, YoutubeProvider>();
		return services;
	}

	public async Task<IServiceCollection> RegisterServices(IServiceCollection services, CancellationToken cancellationToken)
	{
		await LoadConfigurationValuesAsync(_DisplayName, cancellationToken);

		if(string.IsNullOrEmpty(_YoutubeConfiguration?.ApiKey))
		{
			// No configuration provided, no registration to be added
			return services;
		}

		services.AddTransient<ISocialMediaProvider, YoutubeProvider>();
		return services;
	}

	protected override void MapConfigurationValues(ProviderConfiguration providerConfiguration)
	{
		var rootElement = providerConfiguration.ConfigurationSettings?.RootElement;

		_YoutubeConfiguration = new YoutubeConfiguration
		{
			ApiKey = rootElement?.GetProperty("ApiKey").GetString() ?? string.Empty
		};
	}
}
