using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace TagzApp.Providers.YouTubeChat;

public class StartYouTubeChat : IConfigureProvider
{
	public IServiceCollection RegisterServices(IServiceCollection services, IConfiguration configuration)
	{
		//IConfigurationSection config;

		//try
		//{
		//	config = configuration.GetSection(YouTubeChatConfiguration.AppSettingsSection);
		//	if (config is not null) services.Configure<YouTubeChatConfiguration>(config);
		//}
		//catch (Exception ex)
		//{

		//	throw new InvalidConfigurationException(ex.Message, YouTubeChatConfiguration.AppSettingsSection);
		//}

		//// No configuration provided, no registration to be added
		//if (config is null) return services;

		//YouTubeChatConfiguration? options = config.Get<YouTubeChatConfiguration>();
		//if (string.IsNullOrEmpty(options?.ClientId)) return services;

		services.AddTransient<ISocialMediaProvider, YouTubeChatProvider>();

		return services;

	}

}
