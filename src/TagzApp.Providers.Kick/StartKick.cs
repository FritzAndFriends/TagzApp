using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using TagzApp.Communication.Configuration;
using TagzApp.Communication.Extensions;

namespace TagzApp.Providers.Kick;

public class StartKick : IConfigureProvider
{
	private const string _DisplayName = "Kick";

	public async Task<IServiceCollection> RegisterServices(IServiceCollection services, CancellationToken cancellationToken = default)
	{
		// Load initial configuration
		var initialConfig = await BaseProviderConfiguration<KickConfiguration>.CreateFromConfigurationAsync<KickConfiguration>(ConfigureTagzAppFactory.Current);

		// Configure options for IOptionsMonitor
		services.Configure<KickConfiguration>(options =>
		{
			options.UpdateFrom(initialConfig);
		});

		// Add a configuration reload service that can be used to update the options
		services.AddSingleton<IConfigureOptions<KickConfiguration>, KickConfigurationSetup>();

		services.AddHttpClient<ISocialMediaProvider, KickProvider, HttpClientOptions>(new());
		services.AddSingleton<ISocialMediaProvider, KickProvider>();

		return services;
	}
}

/// <summary>
/// Handles configuration setup for KickConfiguration
/// </summary>
public class KickConfigurationSetup : IConfigureOptions<KickConfiguration>
{
	public void Configure(KickConfiguration options)
	{
		// This will be called when the configuration is first accessed
		var config = BaseProviderConfiguration<KickConfiguration>
			.CreateFromConfigurationAsync<KickConfiguration>(ConfigureTagzAppFactory.Current)
			.GetAwaiter()
			.GetResult();
		
		options.UpdateFrom(config);
	}
}