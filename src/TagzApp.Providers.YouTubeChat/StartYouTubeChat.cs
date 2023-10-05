using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using TagzApp.Communication;

namespace TagzApp.Providers.YouTubeChat;

public class StartYouTubeChat : BaseConfigurationProvider, IConfigureProvider, INeedConfiguration
{
	private YouTubeChatConfiguration _Config;

	public StartYouTubeChat(IProviderConfigurationRepository providerConfigurationRepository) : base(providerConfigurationRepository)
	{
	}

	public async Task<IServiceCollection> RegisterServices(IServiceCollection services, CancellationToken cancellationToken = default)
	{

		// Exit not if we don't have a clientid
		if (string.IsNullOrEmpty(_Config.ClientId)) return services;

		await LoadConfigurationValuesAsync(YouTubeChatProvider.ProviderName, cancellationToken);

		services.AddSingleton(_Config);
		services.AddTransient<ISocialMediaProvider, YouTubeChatProvider>();
		return services;

	}

	public void SetConfiguration(IConfiguration configuration)
	{
		_Config = new YouTubeChatConfiguration
		{
			ClientId = configuration[YouTubeChatConfiguration.Key_Google_ClientId],
			ClientSecret = configuration[YouTubeChatConfiguration.Key_Google_ClientSecret]
		};
	}

	protected override void MapConfigurationValues(ProviderConfiguration providerConfiguration)
	{
		// do nothing... yet
	}

}
