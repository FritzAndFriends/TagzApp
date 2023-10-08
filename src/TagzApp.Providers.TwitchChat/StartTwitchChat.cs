using Microsoft.Extensions.DependencyInjection;
using TagzApp.Communication;

namespace TagzApp.Providers.TwitchChat;

public class StartTwitchChat : BaseConfigurationProvider, IConfigureProvider
{
	private const string ConfigurationKey = "providers:twitchchat";
	private const string _DisplayName = "TwitchChat";
	private TwitchChatConfiguration? _TwitchChatConfiguration;

	public StartTwitchChat(IProviderConfigurationRepository providerConfigurationRepository)
		: base(providerConfigurationRepository)
	{
	}

	public async Task<IServiceCollection> RegisterServices(IServiceCollection services, CancellationToken cancellationToken = default)
	{
		await LoadConfigurationValuesAsync(_DisplayName, cancellationToken);

		if (string.IsNullOrEmpty(_TwitchChatConfiguration?.ClientId))
		{
			// No configuration provided, no registration to be added
			return services;
		}

		services.AddSingleton(_TwitchChatConfiguration);
		services.AddSingleton<ISocialMediaProvider, TwitchChatProvider>();

		return services;
	}

	protected override void MapConfigurationValues(ProviderConfiguration providerConfiguration)
	{
		var config = providerConfiguration.ConfigurationSettings;

		if (config is null)
		{
			_TwitchChatConfiguration = TwitchChatConfiguration.Empty;
			return;
		}

		_TwitchChatConfiguration = new TwitchChatConfiguration
		{
			ClientId = config.GetValueOrDefault("ClientId", string.Empty),
			ClientSecret = config.GetValueOrDefault("ClientSecret", string.Empty),
			ChatBotName = config.GetValueOrDefault("ChatBotName", string.Empty),
			OAuthToken = config.GetValueOrDefault("OAuthToken", string.Empty),
			ChannelName = config.GetValueOrDefault("ChannelName", string.Empty)
		};

	}
}
