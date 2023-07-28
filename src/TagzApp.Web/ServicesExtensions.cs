using System.Runtime.Loader;
using System.Reflection;
using TagzApp.Web.Services;

public static class ServicesExtensions {

	public static IServiceCollection ConfigureProvider<T>(this IServiceCollection services, IConfiguration configuration) where T : IConfigureProvider, new()
	{

		var providerStart = (IConfigureProvider)(Activator.CreateInstance<T>());
		providerStart.RegisterServices(services, configuration);

		return services;

	}

	public static IServiceCollection ConfigureProvider(this IServiceCollection services, IConfigureProvider provider, IConfiguration configuration)
	{

		provider.RegisterServices(services, configuration);

		return services;

	}

	public static IServiceCollection AddTagzAppHostedServices(this IServiceCollection services, IConfigurationRoot configuration)
	{

		services.AddSingleton<InMemoryMessagingService>();
		services.AddHostedService(s => s.GetRequiredService<InMemoryMessagingService>());

		// Register the providers
		SocialMediaProviders.LoadExternalProviders();

		if (SocialMediaProviders.Any())
		{
			foreach (var item in SocialMediaProviders)
			{
				services.ConfigureProvider(item, configuration);
			}
		}
		else
		{
			services.ConfigureProvider<TagzApp.Providers.Mastodon.StartMastodon>(configuration);
			services.ConfigureProvider<TagzApp.Providers.Twitter.StartTwitter>(configuration);
		}

		return services;

	}

	private static void LoadExternalProviders(this List<IConfigureProvider> providers)
	{

    var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty;

		if (!string.IsNullOrWhiteSpace(path))
		{
			foreach (string dllPath in Directory.GetFiles(path, "*.dll", SearchOption.AllDirectories))
			{
        var assembly = Assembly.LoadFrom(dllPath);
				var providerAssemblies = assembly.GetTypes()
					.Where(t => typeof(IConfigureProvider).IsAssignableFrom(t) && !t.IsInterface);

				if (providerAssemblies != null && 
					providerAssemblies.Count() > 0)
        {

					foreach(var provider in providerAssemblies)
					{
            var providerInstance = Activator.CreateInstance(provider) as IConfigureProvider;

            if (providerInstance != null)
            {
              providers.Add(providerInstance);
            }
          }
        }
			}
		}

	}

	/// <summary>
	/// A collection of externally configured providers
	/// </summary>
	public static List<IConfigureProvider> SocialMediaProviders { get; set; } = new();


}