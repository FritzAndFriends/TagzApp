using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TagzApp.Common.Exceptions;

namespace TagzApp.Providers.TwitchChat;

public class StartTwitchChat : IConfigureProvider
{

	private const string ConfigurationKey = "providers:twitchchat";

	public IServiceCollection RegisterServices(IServiceCollection services, IConfiguration configuration)
	{

		IConfiguration config = null;
		try
		{
			config = configuration.GetSection(ConfigurationKey);
			services.Configure<TwitchChatConfiguration>(config);
		}
		catch (Exception ex)
		{

			// Was not able to configure the provider
			throw new InvalidConfigurationException(ex.Message, ConfigurationKey);

		}

		if (config is null || string.IsNullOrEmpty(config.GetValue<string>("ClientId")))
		{
			// No configuration provided, no registration to be added
			return services;
		}

		services.AddSingleton<ISocialMediaProvider, TwitchChatProvider>();


		return services;

	}
}
