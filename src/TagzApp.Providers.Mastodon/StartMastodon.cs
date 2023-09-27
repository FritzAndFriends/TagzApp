using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
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

	protected override void MapConfigurationValues(ProviderConfiguration providerConfiguration)
	{
		var config = providerConfiguration.ConfigurationSettings;

		if (config != null)
		{
			_MastodonConfiguration = new MastodonConfiguration
			{
				BaseAddress = new Uri(config["BaseAddress"] ?? string.Empty),
				Timeout = TimeSpan.Parse(config["Timeout"] ?? string.Empty),
				DefaultHeaders = JsonSerializer.Deserialize<Dictionary<string, string>?>(config["DefaultHeaders"]),
				UseHttp2 = bool.Parse(config["UseHttp2"])
			};
		}
	}
}
