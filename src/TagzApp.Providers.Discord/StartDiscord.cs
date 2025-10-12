using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using TagzApp.Communication.Configuration;
using TagzApp.Communication.Extensions;

namespace TagzApp.Providers.Discord;

/// <summary>
/// Service registration for Discord provider
/// </summary>
public class StartDiscord : IConfigureProvider
{
	private const string _DisplayName = "Discord";

	public async Task<IServiceCollection> RegisterServices(IServiceCollection services, CancellationToken cancellationToken = default)
	{
		// Load initial configuration
		var initialConfig = await BaseProviderConfiguration<DiscordConfiguration>.CreateFromConfigurationAsync<DiscordConfiguration>(ConfigureTagzAppFactory.Current);

		// Configure options for IOptionsMonitor
		services.Configure<DiscordConfiguration>(options =>
		{
			options.UpdateFrom(initialConfig);
		});

		// Add a configuration reload service that can be used to update the options
		services.AddSingleton<IConfigureOptions<DiscordConfiguration>, DiscordConfigurationSetup>();

		services.AddHttpClient<ISocialMediaProvider, DiscordProvider, HttpClientOptions>(new());
		services.AddSingleton<ISocialMediaProvider, DiscordProvider>();

		return services;
	}
}

/// <summary>
/// Handles configuration setup for DiscordConfiguration
/// </summary>
public class DiscordConfigurationSetup : IConfigureOptions<DiscordConfiguration>
{
	public void Configure(DiscordConfiguration options)
	{
		// This will be called when the configuration is first accessed
		var config = BaseProviderConfiguration<DiscordConfiguration>
			.CreateFromConfigurationAsync<DiscordConfiguration>(ConfigureTagzAppFactory.Current)
			.GetAwaiter()
			.GetResult();

		options.UpdateFrom(config);
	}
}