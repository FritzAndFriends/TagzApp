using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using TagzApp.Common.Exceptions;

namespace TagzApp.Providers.Mastodon;

public class StartMastodon : IConfigureProvider
{
	private const string ConfigurationKey = "providers:mastodon";

	public IServiceCollection RegisterServices(IServiceCollection services, IConfiguration configuration)
	{

		try
		{
			IConfigurationSection config = configuration.GetSection(ConfigurationKey);
			services.Configure<MastodonConfiguration>(config);
		} catch (Exception ex) {

			throw new InvalidConfigurationException(ex.Message, ConfigurationKey);

		}

		if (string.IsNullOrEmpty(configuration.GetValue<string>($"{ConfigurationKey}:BaseAddress")))
		{
			// No configuration provided, no registration to be added
			return services;
		}

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