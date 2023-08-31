# TagzApp Documentation

This is documentation for writing new features and for using the TagzApp software.

## Table of Contents
1. [Adding new social media providers](#media-providers)
2. [Icons](#icons)
3. [Testing](#testing)
4. [Custom Test Execution Ordering](#ordering)
5. [Docker](#docker)

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


<div id='docker'/>

### Docker
TagzApp includes docker support and docker-compose files for easy deployment. You can either run the latest image that's built by us, or build locally.

### Running the Official Version

```
git clone https://github.com/FritzAndFriends/TagzApp
cd TagzApp

# update primarily .env and docker-compose.yml as needed (custom API tokens, etc.)
docker compose -f docker-compose.yml up

```


### Building a Local Docker Container

```
git clone https://github.com/FritzAndFriends/TagzApp
cd TagzApp

# update primarily .env.local and docker-compose.local.yml as needed (custom API tokens, etc.)
docker compose -f docker-compose.local.yml up --build

# or

docker build -t tagzapp/web:dev -f TagzApp.Web/dockerfile .
docker run \
	--rm \
	-p "8080:80"
	tagzapp/web:dev
```


## Overriding Default Config settings

You don't *need* to override any configurations, the app will work out of the box, but you might not be able to search every provider.

In case you do have your own API keys or other values (see the  the [`appsettings.json`](../src/TagzApp.Web/appsettings.json) for what can be supplied), then you can supply them like this:

* any [`appsettings.json`](../src/TagzApp.Web/appsettings.json) can be set in `.env` and `.env.local` files. Please set them this way: `key__subkey__property=value`. See the provided .env files for examples.

* from `docker-compose.yml` - use `key:subkey:property=value` syntax. See the docker-compose files for examples. Ex.

```
environment:
	- providers:twitter:ApiKey=MySecretKey
```

* from `docker run` - use `-e "providers:twitter:ApiKey=MySecretKey"`. You need to repeat the `-e ` flag for each override. 
