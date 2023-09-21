using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TagzApp.Common.Exceptions;

namespace TagzApp.Providers.YouTubeChat;

public class StartYouTubeChat : IConfigureProvider
{
	public IServiceCollection RegisterServices(IServiceCollection services, IConfiguration configuration)
	{
		IConfigurationSection config;

		try
		{
			config = configuration.GetSection(YouTubeChatConfiguration.AppSettingsSection);
			services.Configure<YouTubeChatConfiguration>(config);
		}
		catch (Exception ex)
		{

			throw new InvalidConfigurationException(ex.Message, YouTubeChatConfiguration.AppSettingsSection);
		}

		YouTubeChatConfiguration? options = config.Get<YouTubeChatConfiguration>();

		if (string.IsNullOrEmpty(options?.ApiKey))
		{
			// No configuration provided, no registration to be added
			return services;
		}

		return services;

	}
}
}
