using Microsoft.Extensions.DependencyInjection;

namespace TagzApp.Providers.Mastodon;

public static class ConfigurationExtensions
{

	public static IServiceCollection RegisterServices(this IServiceCollection services)
	{

		// which package do I need in order to call AddHttpClient?

		services.AddHttpClient<MastodonProvider>(ConfigureHttpClient);

		services
			.AddTransient<ISocialMediaProvider, MastodonProvider>();


		return services;

	}

	internal static void ConfigureHttpClient(HttpClient client)
	{
		client.BaseAddress = new Uri("https://mastodon.social");
	}
}
