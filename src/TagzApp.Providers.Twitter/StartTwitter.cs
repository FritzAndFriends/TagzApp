using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using TagzApp.Communication;
using TagzApp.Communication.Extensions;
using TagzApp.Providers.Twitter.Configuration;

namespace TagzApp.Providers.Twitter;

public class StartTwitter : BaseConfigurationProvider, IConfigureProvider
{
	private const string _DisplayName = "Twitter";
	private TwitterConfiguration? _TwitterConfiguration;

	public StartTwitter(IProviderConfigurationRepository providerConfigurationRepository)
		: base(providerConfigurationRepository)
	{
	}

	public async Task<IServiceCollection> RegisterServices(IServiceCollection services, CancellationToken cancellationToken = default)
	{
		await LoadConfigurationValuesAsync(_DisplayName, cancellationToken);

		if (string.IsNullOrEmpty(_TwitterConfiguration?.BaseAddress?.ToString()))
		{
			// No configuration provided, no registration to be added
			return services;
		}

		services.AddHttpClient<ISocialMediaProvider, TwitterProvider, TwitterConfiguration>(_TwitterConfiguration);
		services.AddTransient<ISocialMediaProvider, TwitterProvider>();
		return services;
	}

	protected override void MapConfigurationValues(ProviderConfiguration providerConfiguration)
	{

		var config = providerConfiguration.ConfigurationSettings;

		if (config != null)
		{
			_TwitterConfiguration = new TwitterConfiguration
			{
				Activated = providerConfiguration.Activated,
				BaseAddress = new Uri(config["BaseAddress"] ?? string.Empty),
				Timeout = TimeSpan.Parse(config["Timeout"] ?? string.Empty),
				DefaultHeaders = JsonSerializer.Deserialize<Dictionary<string, string>?>(config["DefaultHeaders"]),
				ApiKey = config["ApiKey"] ?? string.Empty,
				ApiSecretKey = config["ApiSecretKey"] ?? string.Empty,
				AccessToken = config["AccessToken"] ?? string.Empty,
				AccessTokenSecret = config["AccessTokenSecret"] ?? string.Empty
			};
		}
	}
}
