using Microsoft.Extensions.DependencyInjection;
using TagzApp.Communication;
using TagzApp.Communication.Extensions;
using TagzApp.Providers.Blazot.Configuration;
using TagzApp.Providers.Blazot.Constants;
using TagzApp.Providers.Blazot.Converters;
using TagzApp.Providers.Blazot.Events;
using TagzApp.Providers.Blazot.Interfaces;
using TagzApp.Providers.Blazot.Services;

namespace TagzApp.Providers.Blazot;

public class StartBlazot : BaseConfigurationProvider, IConfigureProvider
{
	private string _DisplayName => BlazotConstants.DisplayName;
	private BlazotClientConfiguration? _BlazotClientConfiguration;
	private BlazotSettings? _BlazotSettings;

	public StartBlazot(IProviderConfigurationRepository providerConfigurationRepository)
		: base(providerConfigurationRepository)
	{
	}

	public async Task<IServiceCollection> RegisterServices(IServiceCollection services, CancellationToken cancellationToken = default)
	{
		await LoadConfigurationValuesAsync(_DisplayName, cancellationToken);

		services.AddSingleton(_BlazotSettings ?? new BlazotSettings());
		services.AddHttpClient<ISocialMediaProvider, BlazotProvider, BlazotClientConfiguration>(_BlazotClientConfiguration ?? new BlazotClientConfiguration());
		services.AddTransient<ISocialMediaProvider, BlazotProvider>();
		services.AddSingleton<IContentConverter, ContentConverter>();
		services.AddSingleton<ITransmissionsService, HashtagTransmissionsService>();
		services.AddSingleton<IAuthService, AuthService>();
		services.AddSingleton<IAuthEvents, AuthEvents>();
		return services;
	}

	protected override void MapConfigurationValues(ProviderConfiguration providerConfiguration)
	{
		var config = providerConfiguration.ConfigurationSettings;

		if (config != null)
		{
			_BlazotClientConfiguration = new BlazotClientConfiguration
			{
				BaseAddress = new Uri(config["BaseAddress"] ?? string.Empty),
				Timeout = TimeSpan.Parse(config["Timeout"] ?? string.Empty),
				ApiKey = config["ApiKey"] ?? string.Empty
			};

			_BlazotSettings = new BlazotSettings
			{
				ApiKey = config["ApiKey"] ?? string.Empty,
				SecretAuthKey = config["SecretAuthKey"] ?? string.Empty,
				WindowSeconds = int.Parse(config["WindowSeconds"]),
				WindowRequests = int.Parse(config["WindowRequests"])
			};
		}
	}
}
