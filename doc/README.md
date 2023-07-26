# TagzApp Documentation

This is documentation for writing new features and for using the TagzApp software.


## Adding new social media providers

Please create new providers in their own project and provide dedicated configuration for each provider using the `TagzApp.Common.IConfigureProvider` interface.  Provider-specific configuration should reside in configuration keys that are under the `provider` parent and lower-case named after the social media network they represent.

Here is an [example](../src/TagzApp.Providers.Mastodon/StartMastodon.cs) from Mastodon:

```csharp
public class StartMastodon : IConfigureProvider
{
	private const string ConfigurationKey = "providers:mastodon";

	public IServiceCollection RegisterServices(IServiceCollection services, IConfiguration configuration)
	{

		IConfigurationSection config = configuration.GetSection(ConfigurationKey);
		services.Configure<MastodonConfiguration>(config); 

		services.AddHttpClient<MastodonProvider>(c => ConfigureHttpClient(c, config));

		services
			.AddTransient<ISocialMediaProvider, MastodonProvider>();

		return services;

	}
	...
}
```

Please throw an `TagzApp.Common.Exceptions.InvalidConfigurationException` if any configuration for your provider is missing.

## Icons

We are using the MIT-licensed [Bootstrap Icon Library](https://icons.getbootstrap.com/) for icons to present on the website.  Please do not introduce a library or icons from outside this library.
