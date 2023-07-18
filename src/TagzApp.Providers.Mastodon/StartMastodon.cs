using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Runtime.CompilerServices;
using TagzApp.Common.Exceptions;

namespace TagzApp.Providers.Mastodon;

public class StartMastodon : IConfigureProvider
{
	private const string ConfigurationKey = "providers:mastodon";

	public IServiceCollection RegisterServices(IServiceCollection services, IConfiguration configuration)
	{

		IConfigurationSection config = configuration.GetSection(ConfigurationKey);
		services.Configure<MastodonConfiguration>(config); 

		services.AddHttpClient<MastodonProvider>(c => ConfigureHttpClient(c, config));

		services
			.AddTransient<ISocialMediaProvider, MastodonProvider>();

		return services;

	}

	internal static void ConfigureHttpClient(HttpClient client, IConfigurationSection config)
	{

		var configAddress = (Uri)config.GetValue(typeof(Uri), "BaseAddress")!;
		if (configAddress is null) throw new InvalidConfigurationException("Missing configuration for Mastodon BaseAddress", "providers:mastodon:BaseAddress");

		ConfigureHttpClient(client, configAddress.ToString());

	}

	internal static void ConfigureHttpClient(HttpClient client, string baseAddress = "https://mastodon.social")
	{

		client.BaseAddress = new Uri(baseAddress);

	}

	public class MastodonConfiguration
	{

		/// <summary>
		/// Base web address to request Mastodon API
		/// </summary>
		public required string BaseAddress { get; set; }


	}

}