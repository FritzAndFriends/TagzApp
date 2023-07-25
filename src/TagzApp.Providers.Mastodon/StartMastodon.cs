using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
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

		//services
		//	.AddSingleton<ISocialMediaProvider, MastodonProvider>();

		services.AddHttpClient<ISocialMediaProvider, MastodonProvider>((svc, c) => {
			var config = svc.GetRequiredService<IOptions<MastodonConfiguration>>().Value;
			c.BaseAddress = new Uri(config.BaseAddress);
		});

		return services;

	}

	public class MastodonConfiguration
	{

		/// <summary>
		/// Base web address to request Mastodon API
		/// </summary>
		public required string BaseAddress { get; set; }


	}

}