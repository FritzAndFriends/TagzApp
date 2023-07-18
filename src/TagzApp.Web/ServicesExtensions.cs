public static class ServicesExtensions {

	public static IServiceCollection ConfigureProvider<T>(this IServiceCollection services, IConfiguration configuration) where T : IConfigureProvider, new()
	{

		var providerStart = (IConfigureProvider)(Activator.CreateInstance<T>());
		providerStart.RegisterServices(services, configuration);

		return services;

	}


}