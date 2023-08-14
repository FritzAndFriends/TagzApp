using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using TagzApp.Common.Exceptions;
using TagzApp.Communication.Extensions;
using TagzApp.Providers.Twitter.Configuration;

namespace TagzApp.Providers.Twitter;

public class StartTwitter : IConfigureProvider
{
	public IServiceCollection RegisterServices(IServiceCollection services, IConfiguration configuration)
	{

		IConfigurationSection config;

		try
		{
			config = configuration.GetSection(TwitterConfiguration.AppSettingsSection);
			services.Configure<TwitterConfiguration>(config);
		}
		catch (Exception ex)
		{

			throw new InvalidConfigurationException(ex.Message, TwitterConfiguration.AppSettingsSection);
		}

		TwitterConfiguration? options = config.Get<TwitterConfiguration>();

		if (string.IsNullOrEmpty(options?.BaseAddress?.ToString()) || string.IsNullOrEmpty(options?.ApiKey))
		{
			// No configuration provided, no registration to be added
			return services;
		}

		services.AddHttpClient<ISocialMediaProvider, TwitterProvider, TwitterConfiguration>(configuration, TwitterConfiguration.AppSettingsSection);
		services.AddTransient<ISocialMediaProvider, TwitterProvider>();

		return services;
	}

}
