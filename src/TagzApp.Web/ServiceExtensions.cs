using TagzApp.Web.Services;

public static class ServiceExtensions
{

	public static IServiceCollection AddTagzAppHostedServices(this IServiceCollection services, IConfiguration configuration) 
	{

		services.AddSingleton<InMemoryMessagingService>();
		services.AddHostedService(s => s.GetRequiredService<InMemoryMessagingService>());

		services.ConfigureProvider<StartMastodon>(configuration);

		return services;

	}


}