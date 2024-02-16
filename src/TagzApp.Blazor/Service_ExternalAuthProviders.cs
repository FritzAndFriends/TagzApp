using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

namespace TagzApp.Blazor;

public static class Service_ExternalAuthProviders
{

	/// <summary>
	/// A collection of externally configured providers
	/// </summary>
	public static AuthenticationBuilder AddExternalProvider(this AuthenticationBuilder builder, string name,
		IConfiguration configuration,
		Action<IConfiguration> action)
	{
		var section = configuration.GetSection($"Authentication:{name}");
		if (section is not null) action(section);
		return builder;
	}

	public static AuthenticationBuilder AddExternalProvider(this AuthenticationBuilder builder, string name,
		IConfiguration configuration,
		Action<Action<Microsoft.AspNetCore.Authentication.OAuth.OAuthOptions>> action)
	{
		return builder.AddExternalProvider(name, configuration, (section) =>
		{
			var clientID = section["ClientID"];
			var clientSecret = section["ClientSecret"];
			if (!string.IsNullOrEmpty(clientID) && !string.IsNullOrEmpty(clientSecret))
			{
				action(options =>
				{
					options.ClientId = clientID;
					options.ClientSecret = clientSecret;
				});
			}
		});
	}

	public static AuthenticationBuilder AddExternalProviders(this AuthenticationBuilder builder,
		IConfiguration configuration)
	{
		builder.AddExternalProvider("Microsoft", configuration, options => builder.AddMicrosoftAccount(options));
		builder.AddExternalProvider("GitHub", configuration, options => builder.AddGitHub(options));
		builder.AddExternalProvider("LinkedIn", configuration, options => builder.AddLinkedIn(options));

		// AddYouTubeProvider(builder, configuration);

		return builder;
	}

	private static void AddYouTubeProvider(AuthenticationBuilder builder, IConfiguration configuration)
	{
		if (!string.IsNullOrEmpty(configuration["Authentication:Google:ClientId"]))
		{
			builder.AddGoogle(options =>
			{

				// TODO: Add YouTubeChatConfiguration

				//options.ClientId = configuration[YouTubeChatConfiguration.Key_Google_ClientId];
				//options.ClientSecret = configuration[YouTubeChatConfiguration.Key_Google_ClientSecret];
				//options.SaveTokens = true;
				//options.AccessType = "offline";  // Allow a refresh token to be delivered
				//options.Scope.Add(YouTubeChatConfiguration.Scope_YouTube);
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
