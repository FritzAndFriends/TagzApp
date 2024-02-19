using TagzApp.Providers.AzureQueue;
using TagzApp.Providers.Blazot;
using TagzApp.Providers.Bluesky;
using TagzApp.Providers.Mastodon;
using TagzApp.Providers.TwitchChat;
using TagzApp.Providers.Twitter;
using TagzApp.Providers.Youtube;
using TagzApp.Providers.YouTubeChat;

namespace TagzApp.Blazor;

public static class Service_Providers
{

	private static readonly List<IConfigureProvider> _Providers = new()
	{
		new StartAzureQueue(),
		new StartBlazot(),
		new StartBluesky(),
		new StartMastodon(),
		new StartTwitchChat(),
		new StartTwitter(),
		new StartYoutube(),
		new StartYouTubeChat()
	};

	public static async Task<IServiceCollection> AddTagzAppProviders(this IServiceCollection services)
	{

		foreach (var provider in _Providers)
		{
			services = await provider.RegisterServices(services);
		}

		return services;

	}

	//public static void UseProviders(this WebApplication app)
	//{

	//	var messagingService = app.Services.GetRequiredService<IMessagingService>();
	//	var 

	//}

}
