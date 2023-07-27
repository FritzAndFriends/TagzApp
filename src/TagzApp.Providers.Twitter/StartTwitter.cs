using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using TagzApp.Common.Exceptions;

namespace TagzApp.Providers.Twitter;

public class StartTwitter : IConfigureProvider
{

	private const string ConfigurationKey = "providers:twitter";

	public IServiceCollection RegisterServices(IServiceCollection services, IConfiguration configuration)
	{

		IConfiguration config = null;
		try
		{
			config = configuration.GetSection(ConfigurationKey);
			services.Configure<TwitterConfiguration>(config);
		} catch (Exception ex) {

			// Was not able to configure the provider
			throw new InvalidConfigurationException(ex.Message, ConfigurationKey);

		}

		if (config is null || string.IsNullOrEmpty(config.GetValue<string>("ApiKey")))
		{
			// No configuration provided, no registration to be added
			return services;
		}

		services.AddHttpClient<ISocialMediaProvider, TwitterProvider>((svc, c) =>
		{
			var config = svc.GetRequiredService<IOptions<TwitterConfiguration>>().Value;
			c.DefaultRequestHeaders.Add("Authorization", $"Bearer {config.BearerToken}");
		});

		return services;

	}

}
