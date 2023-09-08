using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using TagzApp.Common.Exceptions;
using TagzApp.Communication.Extensions;
using TagzApp.Providers.Mastodon.Configuration;
using TagzApp.Storage.Postgres;

namespace TagzApp.Providers.Mastodon;

public class StartMastodon : BaseConfigurationProvider, IConfigureProvider
{
	private MastodonConfiguration? _MastodonConfiguration;

	public StartMastodon(IProviderConfigurationRepository providerConfigurationRepository)
		: base(providerConfigurationRepository)
	{
	}

	public IServiceCollection RegisterServices(IServiceCollection services, IConfiguration configuration)
	{
		IConfigurationSection config;

		try
		{
			config = configuration.GetSection(MastodonConfiguration.AppSettingsSection);
			services.Configure<MastodonConfiguration>(config);
		}
		catch (Exception ex)
		{
			throw new InvalidConfigurationException(ex.Message, MastodonConfiguration.AppSettingsSection);
		}

		MastodonConfiguration? options = config.Get<MastodonConfiguration>();

		if (string.IsNullOrEmpty(options?.BaseAddress?.ToString()))
		{
			// No configuration provided, no registration to be added
			return services;
		}

		services.AddHttpClient<ISocialMediaProvider, MastodonProvider, MastodonConfiguration>(configuration, MastodonConfiguration.AppSettingsSection);
		services.AddTransient<ISocialMediaProvider, MastodonProvider>();
		return services;
	}

	protected override void MapConfigurationValues(JsonDocument configurationValues)
	{
		throw new NotImplementedException();
	}
}
