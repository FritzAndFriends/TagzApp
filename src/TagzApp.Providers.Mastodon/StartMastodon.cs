using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TagzApp.Communication.Extensions;
using TagzApp.Providers.Mastodon.Configuration;

namespace TagzApp.Providers.Mastodon;

public class StartMastodon : IConfigureProvider
{
	public IServiceCollection RegisterServices(IServiceCollection services, IConfiguration configuration)
	{
		services.AddTransient<ISocialMediaProvider, MastodonProvider>();
		services.AddHttpClient<ISocialMediaProvider, MastodonProvider, MastodonConfiguration>(configuration, MastodonConfiguration.AppSettingsSection);
		return services;
	}
}