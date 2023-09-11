﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TagzApp.Common.Exceptions;
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

  public IServiceCollection RegisterServices(IServiceCollection services, IConfiguration configuration)
  {
    IConfigurationSection config;

    try
    {
      config = configuration.GetSection(BlazotClientConfiguration.AppSettingsSection);
      services.Configure<BlazotClientConfiguration>(config);
    }
    catch (Exception ex)
    {
      throw new InvalidConfigurationException(ex.Message, BlazotClientConfiguration.AppSettingsSection);
    }

    var settings = config.Get<BlazotClientConfiguration>();

    if (string.IsNullOrWhiteSpace(settings?.BaseAddress?.ToString()) || string.IsNullOrWhiteSpace(settings.ApiKey))
      return services;

    services.AddSingleton(configuration);
    services.AddHttpClient<ISocialMediaProvider, BlazotProvider, BlazotClientConfiguration>(configuration, BlazotClientConfiguration.AppSettingsSection);
    services.AddTransient<ISocialMediaProvider, BlazotProvider>();
    services.AddSingleton<IContentConverter, ContentConverter>();
    services.AddSingleton<ITransmissionsService, HashtagTransmissionsService>();
    services.AddSingleton<IAuthService, AuthService>();
    services.AddSingleton<IAuthEvents, AuthEvents>();

    return services;
  }

	public async Task<IServiceCollection> RegisterServices(IServiceCollection services, CancellationToken cancellationToken = default)
	{
		await LoadConfigurationValuesAsync(_DisplayName, cancellationToken);

		if (string.IsNullOrWhiteSpace(_BlazotClientConfiguration?.BaseAddress?.ToString()) ||
			string.IsNullOrWhiteSpace(_BlazotClientConfiguration.ApiKey) || _BlazotSettings == null)
			return services;

		services.AddSingleton(_BlazotSettings);
		services.AddHttpClient<ISocialMediaProvider, BlazotProvider, BlazotClientConfiguration>(_BlazotClientConfiguration);
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
				ApiKey = config["ApiKey"]?? string.Empty
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
