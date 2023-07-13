using TagzApp.Web.Services;

public static class ServiceExtensions
{

	public static IServiceCollection AddTagzAppHostedServices(this IServiceCollection services) 
	{

		services.AddSingleton<InMemoryMessagingService>();
		services.AddHostedService(s => s.GetRequiredService<InMemoryMessagingService>());

		return services;

	}


}