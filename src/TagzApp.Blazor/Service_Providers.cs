using TagzApp.Providers.Mastodon;
using TagzApp.Providers.TwitchChat;

namespace TagzApp.Blazor;

public static class Service_Providers
{

	private static readonly List<IConfigureProvider> _Providers = new()
	{
		new StartTwitchChat(),
		new StartMastodon()
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
