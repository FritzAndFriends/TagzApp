using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TagzApp.Common.Exceptions;
using TagzApp.Providers.Youtube.Configuration;

namespace TagzApp.Providers.Youtube;

public class StartYoutube : IConfigureProvider
{

	public IServiceCollection RegisterServices(IServiceCollection services, IConfiguration configuration)
	{

		var config = configuration.GetSection(YoutubeConfiguration.AppSettingsSection)!;
		if (config is null) throw new InvalidConfigurationException(YoutubeConfiguration.AppSettingsSection);
		services.Configure<YoutubeConfiguration>(config);

		services.AddTransient<ISocialMediaProvider, YoutubeProvider>();
		return services;
	}
}
