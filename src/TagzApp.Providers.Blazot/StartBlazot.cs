using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TagzApp.Common.Exceptions;
using TagzApp.Communication.Extensions;
using TagzApp.Providers.Blazot.Configuration;
using TagzApp.Providers.Blazot.Constants;
using TagzApp.Providers.Blazot.Converters;
using TagzApp.Providers.Blazot.Events;
using TagzApp.Providers.Blazot.Interfaces;
using TagzApp.Providers.Blazot.Services;

namespace TagzApp.Providers.Blazot;

public class StartBlazot : IConfigureProvider
{
  private readonly ILogger<StartBlazot> _Logger;

  public StartBlazot() { }
  public StartBlazot(ILogger<StartBlazot> logger)
  {
    _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
      _Logger.LogError(ex, "Error configuring Blazot: {message}", ex.Message);
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
}
