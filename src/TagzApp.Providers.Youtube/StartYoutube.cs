using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TagzApp.Common.Exceptions;

namespace TagzApp.Providers.Youtube;

public class StartYoutube : IConfigureProvider {

	public const string ConfigurationKey = "providers:youtube";

	public IServiceCollection RegisterServices(IServiceCollection services, IConfiguration configuration)
	{

		var config = configuration.GetSection(ConfigurationKey)!;
		if (config is null) throw new InvalidConfigurationException(ConfigurationKey);
		services.Configure<YoutubeConfiguration>(config);

    services.AddTransient<ISocialMediaProvider, YoutubeProvider>();
    return services;
	}


}

public class YoutubeConfiguration
{

	/// <summary>
	/// YouTube assigned API key
	/// </summary>
	public required string ApiKey { get; set; }

}
