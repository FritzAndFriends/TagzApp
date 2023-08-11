# TagzApp Documentation

This is documentation for writing new features and for using the TagzApp software.

## Table of Contents
1. [Adding new social media providers](#media-providers)
2. [Icons](#icons)
3. [Testing](#testing)
4. [Custom Test Execution Ordering](#ordering)

<div id='media-providers'/>

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

<div id='icons'/>

## Icons

We are using the MIT-licensed [Bootstrap Icon Library](https://icons.getbootstrap.com/) for icons to present on the website.  Please do not introduce a library or icons from outside this library.

<div id='testing'/>

## Testing

We are using the [xUnit](https://xunit.net/) testing framework for unit testing.
[Playwright](https://playwright.dev/) for end-to-end testing.
Please ensure tests are written for new code & that all tests pass before submitting a pull request.

Testing proves code operated as expected under given conditions.

Tests typically consist of 3 parts:
Arrange, Act, Assert. (aka Given-When-Then)
Arrange test state, Act to perform actionionable behavior, and Assert actual versus expected results match.

<div id='ordering'/>

### Custom Test Execution Ordering
We are using the xUnit's ability to provide customize test execution ordering, and a functional example can be seen in the [TagzApp.WebTests/ModalWebTests.cs](../src/TagzApp.WebTest/ModalWebTests.cs) test class.

An implementation of a [PriorityOrderer.cs](../src/TagzApp.WebTest/PriorityOrderer.cs) was created and then decorates the class as a TestCaseOrdererAttribute. The orderer uses a test attribute class and its properties to determine the ordering logic.

Test cases which want to avail of the orderer are decorated with the test Attribute with required passed property values. The orderer then uses these values to determine the order of execution.

This example came directly out of [Microsoft learn documentation](https://learn.microsoft.com/en-us/dotnet/core/testing/order-unit-tests?pivots=xunit#order-by-custom-attribute) for more information on how to use this feature.
