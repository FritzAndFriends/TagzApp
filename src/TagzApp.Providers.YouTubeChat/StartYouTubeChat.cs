using Microsoft.Extensions.DependencyInjection;

namespace TagzApp.Providers.YouTubeChat;

public class StartYouTubeChat : IConfigureProvider
{

	public async Task<IServiceCollection> RegisterServices(IServiceCollection services, CancellationToken cancellationToken = default)
	{

		// Exit not if we don't have a clientid
		var config = await ConfigureTagzAppFactory.Current.GetConfigurationById<YouTubeChatConfiguration>(YouTubeChatConfiguration.AppSettingsSection);
		if (string.IsNullOrEmpty(config.ClientId)) return services;


		services.AddSingleton(config);
		services.AddTransient<ISocialMediaProvider, YouTubeChatProvider>();
		return services;

	}

}
