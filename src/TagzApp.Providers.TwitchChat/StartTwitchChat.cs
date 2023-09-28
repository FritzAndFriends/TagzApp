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

		if (config != null)
		{
			_TwitchChatConfiguration = new TwitchChatConfiguration
			{
				ClientId = config["ClientId"] ?? string.Empty,
				ClientSecret = config["ClientSecret"] ?? string.Empty,
				ChatBotName = config["ChatBotName"] ?? string.Empty,
				OAuthToken = config["OAuthToken"] ?? string.Empty,
				ChannelName = config["ChannelName"] ?? string.Empty
			};
		}
	}
}
