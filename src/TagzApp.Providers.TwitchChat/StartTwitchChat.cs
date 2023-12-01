using Microsoft.Extensions.DependencyInjection;

namespace TagzApp.Providers.TwitchChat;

public class StartTwitchChat : IConfigureProvider
{
	private const string ConfigurationKey = "providers:twitchchat";
	private const string _DisplayName = "TwitchChat";
	private TwitchChatConfiguration? _TwitchChatConfiguration;

	public async Task<IServiceCollection> RegisterServices(IServiceCollection services, CancellationToken cancellationToken = default)
	{

		_TwitchChatConfiguration = await ConfigureTagzAppFactory.Current.GetConfigurationById<TwitchChatConfiguration>(ConfigurationKey);

		services.AddSingleton(_TwitchChatConfiguration ?? TwitchChatConfiguration.Empty);
		services.AddSingleton<ISocialMediaProvider, TwitchChatProvider>();

		return services;
	}

}
