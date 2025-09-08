using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using TagzApp.Providers.YouTubeChat;

namespace TagzApp.Blazor;

public static class Service_ExternalAuthProviders
{

	public static AuthenticationBuilder AddExternalProvider(this AuthenticationBuilder builder, string name,
		IConfiguration configuration,
		Action<Action<Microsoft.AspNetCore.Authentication.OAuth.OAuthOptions>> action)
	{

		var section = configuration.GetSection($"Authentication:{name}");

		var clientID = section?["ClientID"] ?? string.Empty;
		var clientSecret = section?["ClientSecret"] ?? string.Empty;

		action(options =>
			{

				if (!string.IsNullOrEmpty(clientID) && !string.IsNullOrEmpty(clientSecret))
				{
					options.ClientId = clientID;
					options.ClientSecret = clientSecret;
					return;
				}

				// Override the OAuth events to read configuration at runtime
				options.Events.OnRedirectToAuthorizationEndpoint = async context =>
				{
					// Get fresh configuration from your config service/database
					var configService = context.HttpContext.RequestServices.GetRequiredService<IConfigureTagzApp>();
					clientID = await configService.GetConfigurationStringById($"Authentication:{name}:ClientID");
					clientSecret = await configService.GetConfigurationStringById($"Authentication:{name}:ClientSecret");

					if (string.IsNullOrEmpty(clientID) || string.IsNullOrEmpty(clientSecret))
					{
						// No configuration - redirect to error page
						context.Response.Redirect($"/Account/ProviderNotConfigured?provider={name}");
						context.Response.StatusCode = 403; // Forbidden
						return;
					}


					// Update the options with fresh configuration
					context.Options.ClientId = clientID;
					context.Options.ClientSecret = clientSecret;

					// Continue with normal OAuth flow
					context.Response.Redirect(context.RedirectUri);
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
