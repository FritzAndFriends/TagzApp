using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace TagzApp.Providers.Twitter;

public class StartTwitter : IConfigureProvider
{

	private const string ConfigurationKey = "providers:twitter";

	public IServiceCollection RegisterServices(IServiceCollection services, IConfiguration configuration)
	{

		IConfiguration config = configuration.GetSection(ConfigurationKey);
		services.Configure<TwitterConfiguration>(config);

		services.AddHttpClient<ISocialMediaProvider, TwitterProvider>((svc, c) =>
		{
			var config = svc.GetRequiredService<IOptions<TwitterConfiguration>>().Value;
			c.DefaultRequestHeaders.Add("Authorization", $"Bearer {config.BearerToken}");
		});

		return services;

	}

}
