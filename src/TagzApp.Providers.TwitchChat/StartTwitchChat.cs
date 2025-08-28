using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using TagzApp.Communication.Configuration;
using TagzApp.Communication.Extensions;

namespace TagzApp.Providers.TwitchChat;

public class StartTwitchChat : IConfigureProvider
{
	private const string _DisplayName = "TwitchChat";

	public async Task<IServiceCollection> RegisterServices(IServiceCollection services, CancellationToken cancellationToken = default)
	{
		// Load initial configuration
		var initialConfig = await BaseProviderConfiguration<TwitchChatConfiguration>.CreateFromConfigurationAsync<TwitchChatConfiguration>(ConfigureTagzAppFactory.Current);

		// Configure options for IOptionsMonitor
		services.Configure<TwitchChatConfiguration>(options =>
		{
			options.UpdateFrom(initialConfig);
		});

		// Add a configuration reload service that can be used to update the options
		services.AddSingleton<IConfigureOptions<TwitchChatConfiguration>, TwitchChatConfigurationSetup>();

		services.AddHttpClient<ISocialMediaProvider, TwitchChatProvider, HttpClientOptions>(new());
		services.AddSingleton<ISocialMediaProvider, TwitchChatProvider>();

		return services;
	}
}

/// <summary>
/// Handles configuration setup for TwitchChatConfiguration
/// </summary>
public class TwitchChatConfigurationSetup : IConfigureOptions<TwitchChatConfiguration>
{
	public void Configure(TwitchChatConfiguration options)
	{
		// This will be called when the configuration is first accessed
		var config = BaseProviderConfiguration<TwitchChatConfiguration>
			.CreateFromConfigurationAsync<TwitchChatConfiguration>(ConfigureTagzAppFactory.Current)
			.GetAwaiter()
			.GetResult();
		
		options.UpdateFrom(config);
	}
}
