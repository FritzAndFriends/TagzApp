# TagzApp Documentation

This is documentation for writing new features and for using the TagzApp software.

## Table of Contents
1. [Integrate with your app](QueueIntegration.md)
2. [Adding new social media providers](#media-providers)
3. [Provider Configuration Pattern](#provider-configuration)
4. [LinkedIn Provider](#linkedin-provider)
5. [Event Display](#event-display)
6. [Icons](#icons)
7. [Testing](#testing)
8. [Custom Test Execution Ordering](#ordering)
9. [Docker](#docker)

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

<div id='provider-configuration'/>

## Provider Configuration Pattern

TagzApp uses a standardized configuration pattern with `IOptionsMonitor<T>` for reactive configuration updates. This pattern provides immediate configuration updates, self-contained configuration management, and support for testing scenarios.

For detailed implementation guidance, see the [Provider Configuration Pattern Guide](./Provider-Configuration-Pattern.md).

**Key Features:**
- **Immediate Updates**: Configuration changes affect providers instantly without restart
- **Self-Contained**: Each provider handles its own configuration persistence
- **Reactive**: Automatic notifications when configuration changes
- **Testable**: Clean separation between production and testing scenarios

**Quick Start:**
1. Inherit from `BaseProviderConfiguration<T>` in `TagzApp.Common.Client`
2. Use `IOptionsMonitor<T>` in your provider constructor
3. Handle configuration changes with `HandleConfigurationChange()` method
4. Use `StaticOptionsMonitor<T>` from `TagzApp.Common.Configuration` for testing

<div id='linkedin-provider'/>

## LinkedIn Provider

The LinkedIn provider (`TagzApp.Providers.LinkedIn`) integrates with LinkedIn's Marketing API to aggregate hashtag content from LinkedIn posts.

### Prerequisites
- A **LinkedIn Developer App** registered at https://www.linkedin.com/developers/
- **Marketing Developer Platform (MDP)** approval for hashtag search API access
- OAuth 2.0 credentials: Client ID, Client Secret, Access Token, and Refresh Token

### Configuration
Configure the LinkedIn provider through the admin panel at `/Admin/GenericProvider` (select LinkedIn). Required fields:
- **Client ID / Client Secret** — from your LinkedIn Developer App
- **Access Token / Refresh Token** — obtained via OAuth 2.0 Authorization Code Flow
- **Token Expires At** — ISO 8601 timestamp for token expiry monitoring
- **Polling Interval** — minutes between API calls (minimum 5, default 5)
- **Daily Call Budget** — maximum API calls per day (minimum 10, default 100)

### Rate Limits
LinkedIn's Marketing API has strict rate limits:
- **Member token:** 100 requests/day
- **App-level token:** 500 requests/day
- The provider tracks daily usage and degrades gracefully when the budget is exhausted

### API Details
- **Endpoint:** `GET /rest/posts?q=hashtag&hashtag={encodedHashtag}`
- **Version header:** `LinkedIn-Version: 202401`
- **Authentication:** Bearer token via OAuth 2.0

<div id='event-display'/>

## Event Display

The Event Display (`/EventDisplay`) is a full-screen, auto-scrolling kiosk view designed for big screens at live events like conferences and meetups.

### Features
- **Hands-free operation** — no interaction required; auto-scrolls through content
- **Real-time updates** — new content appears via SignalR as it's approved
- **Branding strip** — shows the tracked hashtag at the bottom of the screen
- **Content limit** — keeps the last 50 messages to prevent memory growth
- **Auto-scroll** — smooth continuous scroll with pause-on-new-content behavior

### Usage
1. Configure tracked hashtags in the admin panel
2. Navigate to `/EventDisplay` on the display screen
3. The page runs hands-free — content appears and scrolls automatically

### Tips for Events
- Use a dedicated browser in full-screen/kiosk mode (F11)
- The branding strip shows the tracked hashtag automatically
- Content scrolls continuously; new posts pause the scroll briefly for visibility
- Works best on landscape displays (1080p or higher)

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

#### Running on Port 8080

The ASP.NET Core images that TagzApp is based on expose port 8080 by default.  You can remap this to port 80 in the container by passing in an environment variable called `ASPNETCORE_HTTP_PORTS` with a value of `80`

More details in the [ASP.NET Core Documentation](https://learn.microsoft.com/en-us/dotnet/core/compatibility/containers/8.0/aspnet-port)

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
