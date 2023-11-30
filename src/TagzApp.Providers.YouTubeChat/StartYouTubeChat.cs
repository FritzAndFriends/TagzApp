using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using TagzApp.Communication;

namespace TagzApp.Providers.YouTubeChat;

public class StartYouTubeChat : BaseConfigurationProvider<YouTubeChatConfiguration>, IConfigureProvider
{

	public StartYouTubeChat(IConfigureTagzApp configureTagzApp) : base(configureTagzApp)
	{
	}

	public async Task<IServiceCollection> RegisterServices(IServiceCollection services, CancellationToken cancellationToken = default)
	{

		// Exit not if we don't have a clientid
		var config = await LoadConfigurationValuesAsync(YouTubeChatProvider.ProviderName);
		if (string.IsNullOrEmpty(config.ClientId)) return services;


		services.AddSingleton(config);
		services.AddTransient<ISocialMediaProvider, YouTubeChatProvider>();
		return services;

	}

}
