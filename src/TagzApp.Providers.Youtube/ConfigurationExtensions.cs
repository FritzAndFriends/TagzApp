using Microsoft.Extensions.DependencyInjection;

namespace TagzApp.Providers.Youtube;

public static class ConfigurationExtensions {

  public static IServiceCollection RegisterServices(this IServiceCollection services) {

    services.AddTransient<ISocialMediaProvider, YoutubeProvider>();
    return services;
  }
}
