using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Text.Json;
using TagzApp.Common.Exceptions;
using TagzApp.Communication;
using TagzApp.Communication.Extensions;
using TagzApp.Providers.Mastodon.Configuration;

namespace TagzApp.Providers.Mastodon;

public class StartMastodon : BaseConfigurationProvider, IConfigureProvider
{
	private const string _DisplayName = "Mastodon";
	private MastodonConfiguration? _MastodonConfiguration;

	public StartMastodon(IProviderConfigurationRepository providerConfigurationRepository)
		: base(providerConfigurationRepository)
	{
	}

	public async Task<IServiceCollection> RegisterServices(IServiceCollection services, CancellationToken cancellationToken = default)
	{
		await LoadConfigurationValuesAsync(_DisplayName, cancellationToken);

		if (string.IsNullOrEmpty(_MastodonConfiguration?.BaseAddress?.ToString()))
		{
			// No configuration provided, no registration to be added
			return services;
		}

		services.AddHttpClient<ISocialMediaProvider, MastodonProvider, MastodonConfiguration>(_MastodonConfiguration);
		services.AddTransient<ISocialMediaProvider, MastodonProvider>();
		return services;
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

	protected override void MapConfigurationValues(ProviderConfiguration providerConfiguration)
	{
		var rootElement = providerConfiguration.ConfigurationSettings?.RootElement;

		_MastodonConfiguration = new MastodonConfiguration
		{
			BaseAddress = new Uri(rootElement?.GetProperty("BaseAddress").GetString() ?? string.Empty),
			Timeout = TimeSpan.Parse(rootElement?.GetProperty("Timeout").GetString() ?? string.Empty),
			DefaultHeaders = rootElement?.GetProperty("DefaultHeaders").Deserialize<Dictionary<string, string>?>(),
			UseHttp2 = rootElement?.GetProperty("UseHttp2").GetBoolean() ?? false
		};
	}
}
