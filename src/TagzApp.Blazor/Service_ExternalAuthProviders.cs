using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using System.Security.Claims;
using TagzApp.Providers.YouTubeChat;

namespace TagzApp.Blazor;

public static class Service_ExternalAuthProviders
{

	public const string CLIENTID_DUMMY = "<DUMMY CLIENT ID>";
	public const string CLIENTSECRET_DUMMY = "<DUMMY CLIENT SECRET>";

	public static AuthenticationBuilder AddExternalProvider(this AuthenticationBuilder builder, string name,
		IConfiguration configuration,
		Action<Action<OAuthOptions>> action)
	{

		var section = configuration.GetSection($"Authentication:{name}");

		//var clientID = section?["ClientID"] ?? CLIENTID_DUMMY;
		//var clientSecret = section?["ClientSecret"] ?? CLIENTSECRET_DUMMY;
		var clientID = CLIENTID_DUMMY;
		var clientSecret = CLIENTSECRET_DUMMY;

		action(options =>
			{

				options.ClientId = clientID;
				options.ClientSecret = clientSecret;
				if (clientID != CLIENTID_DUMMY && clientSecret != CLIENTSECRET_DUMMY) return;


				// Override the OAuth events to read configuration at runtime
				options.Events.OnRedirectToAuthorizationEndpoint = async context =>
				{
					// Get fresh configuration from your config service/database
					var configService = context.HttpContext.RequestServices.GetRequiredService<IConfiguration>();
					var innerClientID = configService[$"Authentication:{name}:ClientID"];
					var innerClientSecret = configService[$"Authentication:{name}:ClientSecret"];

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

					// This doesn't look like the right calculation of STATE
					var state = context.Properties.Items[".xsrf"];
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
		IConfiguration configuration)
	{
		builder.AddExternalProvider("Microsoft", configuration, options => builder.AddMicrosoftAccount(options));
		builder.AddExternalProvider("GitHub", configuration, options => builder.AddGitHub(options));
		builder.AddExternalProvider("LinkedIn", configuration, options => builder.AddLinkedIn(options));
		builder.AddExternalProvider("Google", configuration, options => builder.AddGoogle(options));


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
