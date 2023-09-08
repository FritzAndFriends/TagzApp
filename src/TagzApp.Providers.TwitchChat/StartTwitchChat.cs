using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TagzApp.Common.Exceptions;
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

	public IServiceCollection RegisterServices(IServiceCollection services, IConfiguration configuration)
	{

		IConfiguration config = null;
		try
		{
			config = configuration.GetSection(ConfigurationKey);
			services.Configure<TwitchChatConfiguration>(config);
		}
		catch (Exception ex)
		{

			// Was not able to configure the provider
			throw new InvalidConfigurationException(ex.Message, ConfigurationKey);

		}

		if (config is null || string.IsNullOrEmpty(config.GetValue<string>("ClientId")))
		{
			// No configuration provided, no registration to be added
			return services;
		}

		services.AddSingleton<ISocialMediaProvider, TwitchChatProvider>();


		return services;

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
		var rootElement = providerConfiguration.ConfigurationSettings?.RootElement;

		_TwitchChatConfiguration = new TwitchChatConfiguration
		{
			ClientId = rootElement?.GetProperty("ClientId").GetString() ?? string.Empty,
			ClientSecret = rootElement?.GetProperty("ClientSecret").GetString() ?? string.Empty,
			ChatBotName = rootElement?.GetProperty("ChatBotName").GetString() ?? string.Empty,
			OAuthToken = rootElement?.GetProperty("OAuthToken").GetString() ?? string.Empty
		};
	}
}
