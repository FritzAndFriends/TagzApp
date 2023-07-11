using Microsoft.Extensions.DependencyInjection;

namespace TagzApp.Providers.Mastodon;

public static class ConfigurationExtensions
{

	public static IServiceCollection RegisterServices(this IServiceCollection services)
	{

		services.AddTransient<ISocialMediaProvider, MastodonProvider>();

		return services;

	}

}
