using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using System.Security.Claims;
using System.Threading.Tasks;
using TagzApp.Providers.YouTubeChat;

namespace TagzApp.Blazor;

public static class Service_ExternalAuthProviders
{

	public const string CLIENTID_DUMMY = "<DUMMY CLIENT ID>";
	public const string CLIENTSECRET_DUMMY = "<DUMMY CLIENT SECRET>";

	/// <summary>
	/// Dictionary of external authentication providers with their configuration actions
	/// </summary>
	public static readonly Dictionary<string, Action<AuthenticationBuilder, Action<OAuthOptions>>> ExternalProviders = new()
	{
		["Microsoft"] = (builder, options) => builder.AddMicrosoftAccount(options),
		["GitHub"] = (builder, options) => builder.AddGitHub(options),
		["LinkedIn"] = (builder, options) => builder.AddLinkedIn(options),
		["Google"] = (builder, options) => builder.AddGoogle(options),
		["Twitch"] = (builder, options) => builder.AddTwitch(options)
	};


	public static AuthenticationBuilder AddExternalProvider(this AuthenticationBuilder builder, string name,
		IConfigureTagzApp configuration,
		Action<Action<OAuthOptions>> action)
	{

		var section = configuration.GetConfigurationById<Dictionary<string, string>>($"Authentication:{name}").GetAwaiter().GetResult();

		var clientID = section?.ContainsKey("ClientID") == true ? (section["ClientID"] ?? CLIENTID_DUMMY) : CLIENTID_DUMMY;
		var clientSecret = section?.ContainsKey("ClientSecret") == true ? (section["ClientSecret"] ?? CLIENTSECRET_DUMMY) : CLIENTSECRET_DUMMY;

		action(options =>
			{

				options.ClientId = clientID;
				options.ClientSecret = clientSecret;
				if (clientID != CLIENTID_DUMMY && clientSecret != CLIENTSECRET_DUMMY) return;


				// Override the OAuth events to read configuration at runtime
				options.Events.OnRedirectToAuthorizationEndpoint = async context =>
				{
					// Get fresh configuration from your config service/database
					var configService = context.HttpContext.RequestServices.GetRequiredService<IConfigureTagzApp>();
					var keys = await configService.GetConfigurationById<Dictionary<string, string>>($"Authentication:{name}");
					var innerClientID = keys["ClientID"];
					var innerClientSecret = keys["ClientSecret"];

					if (innerClientID == CLIENTID_DUMMY || innerClientSecret == CLIENTSECRET_DUMMY)
					{
						// No configuration - redirect to error page
						context.Response.Redirect($"/Account/ProviderNotConfigured?provider={name}");
						context.Response.StatusCode = 403; // Forbidden
						return;
					}


					// Update the options with fresh configuration
					context.Options.ClientId = innerClientID;
					context.Options.ClientSecret = innerClientSecret;

					// Continue with normal OAuth flow
					//context.Response.Redirect(context.RedirectUri);

					// Manually build the authorization URL with the updated ClientId
					var authorizationEndpoint = context.Options.AuthorizationEndpoint;
					var redirectUri = context.Options.CallbackPath.HasValue
						? $"{context.Request.Scheme}://{context.Request.Host}{context.Options.CallbackPath}"
						: context.RedirectUri;

					// Properly generate the state parameter using the StateDataFormat
					var state = context.Options.StateDataFormat.Protect(context.Properties);
					var scope = string.Join(" ", context.Options.Scope);

					var authUrl = $"{authorizationEndpoint}" +
						$"?client_id={Uri.EscapeDataString(innerClientID)}" +
						$"&redirect_uri={Uri.EscapeDataString(redirectUri)}" +
						$"&response_type=code" +
						$"&scope={Uri.EscapeDataString(scope)}" +
						$"&state={Uri.EscapeDataString(state)}";

					// Continue with the recalculated OAuth flow
					context.Response.Redirect(authUrl);

				};

			});

		return builder;

	}

	public static AuthenticationBuilder AddExternalProviders(this AuthenticationBuilder builder,
		IConfigureTagzApp configuration)
	{

		foreach (var provider in ExternalProviders)
		{
			builder.AddExternalProvider(provider.Key, configuration, options => provider.Value(builder, options));
		}

		// AddYouTubeProvider(builder, configuration);

		return builder;
	}

	// This isn't currently used... but it might be useful in the future.
	private static void AddYouTubeProvider(AuthenticationBuilder builder, IConfiguration configuration)
	{
		if (!string.IsNullOrEmpty(configuration[YouTubeChatConfiguration.Key_Google_ClientId]))
		{
			builder.AddGoogle(options =>
			{

				// TODO: Add YouTubeChatConfiguration

				options.ClientId = configuration[YouTubeChatConfiguration.Key_Google_ClientId]!;
				options.ClientSecret = configuration[YouTubeChatConfiguration.Key_Google_ClientSecret]!;
				options.SaveTokens = true;
				options.AccessType = "offline";  // Allow a refresh token to be delivered
				options.Scope.Add(YouTubeChatConfiguration.Scope_YouTube);
				options.Events.OnTicketReceived = ctx =>
				{
					var tokens = ctx.Properties.GetTokens().ToList();
					tokens.Add(new AuthenticationToken
					{
						Name = "Ticket Created",
						Value = DateTime.UtcNow.ToString()
					});
					tokens.Add(new AuthenticationToken
					{
						Name = "Email",
						Value = ctx.Principal.Claims.First(c => c.Type == ClaimTypes.Email).Value
					});
					ctx.Properties.StoreTokens(tokens);
					return Task.CompletedTask;
				};
			});
		}
	}

}
