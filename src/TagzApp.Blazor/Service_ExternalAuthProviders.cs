using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using System.Security.Claims;
using TagzApp.Providers.YouTubeChat;

namespace TagzApp.Blazor;

public static class Service_ExternalAuthProviders
{

    public const string CLIENTID_DUMMY = "<DUMMY CLIENT ID>";
    public const string CLIENTSECRET_DUMMY = "<DUMMY CLIENT SECRET>";

    /// <summary>
    /// Ensures that the redirect URI uses HTTPS scheme for OAuth security compliance
    /// </summary>
    /// <param name="redirectUri">The original redirect URI</param>
    /// <returns>The redirect URI with HTTPS scheme enforced</returns>
    private static string EnsureHttpsRedirectUri(string redirectUri)
    {
        if (string.IsNullOrEmpty(redirectUri))
            return redirectUri;

        if (Uri.TryCreate(redirectUri, UriKind.Absolute, out var uri))
        {
            if (uri.Scheme.Equals("http", StringComparison.OrdinalIgnoreCase))
            {
                var httpsUri = new UriBuilder(uri) { Scheme = "https" };
                return httpsUri.ToString();
            }
        }
        
        return redirectUri;
    }

    /// <summary>
    /// Dictionary of external authentication providers with their configuration actions
    /// </summary>
    public static readonly Dictionary<string, Action<AuthenticationBuilder, Action<OAuthOptions>>> ExternalProviders = new()
    {
        ["Microsoft"] = (builder, options) => builder.AddMicrosoftAccount(options),
        ["GitHub"] = (builder, options) => builder.AddGitHub(options),
        ["LinkedIn"] = (builder, options) => builder.AddLinkedIn(options),
        ["Google"] = (builder, options) => builder.AddGoogle(options),
        ["Twitch"] = (builder, options) => builder.AddTwitch(options),
        ["Apple"] = (builder, options) => builder.AddApple(options)
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

                // Configure OAuth events for proper handling of authentication flow
                options.Events.OnRedirectToAuthorizationEndpoint = async context =>
                {
                    string actualClientId = clientID;
                    string actualClientSecret = clientSecret;

                    // If we have dummy values, try to get fresh configuration from database
                    if (clientID == CLIENTID_DUMMY || clientSecret == CLIENTSECRET_DUMMY)
                    {
                        var configService = context.HttpContext.RequestServices.GetRequiredService<IConfigureTagzApp>();
                        var keys = await configService.GetConfigurationById<Dictionary<string, string>>($"Authentication:{name}");
                        
                        if (keys == null || !keys.ContainsKey("ClientID") || !keys.ContainsKey("ClientSecret"))
                        {
                            // No configuration - redirect to error page
                            context.Response.Redirect($"/Account/ProviderNotConfigured?provider={name}");
                            return;
                        }

                        actualClientId = keys["ClientID"];
                        actualClientSecret = keys["ClientSecret"];

                        if (actualClientId == CLIENTID_DUMMY || actualClientSecret == CLIENTSECRET_DUMMY)
                        {
                            // No configuration - redirect to error page
                            context.Response.Redirect($"/Account/ProviderNotConfigured?provider={name}");
                            return;
                        }

                        // Update the options with fresh configuration
                        context.Options.ClientId = actualClientId;
                        context.Options.ClientSecret = actualClientSecret;
                    }

                    // Build redirect URI properly respecting forwarded headers and HTTPS
                    var request = context.HttpContext.Request;
                    var scheme = request.Headers.ContainsKey("X-Forwarded-Proto") ? 
                        request.Headers["X-Forwarded-Proto"].ToString() : 
                        request.Scheme;
                    
                    // Ensure HTTPS for external OAuth providers
                    if (scheme.Equals("http", StringComparison.OrdinalIgnoreCase))
                    {
                        scheme = "https";
                    }

                    var host = request.Headers.ContainsKey("X-Forwarded-Host") ? 
                        request.Headers["X-Forwarded-Host"].ToString() : 
                        request.Host.ToString();

                    var redirectUri = context.Options.CallbackPath.HasValue
                        ? $"{scheme}://{host}{context.Options.CallbackPath}"
                        : EnsureHttpsRedirectUri(context.RedirectUri);

                    // Properly generate the state parameter using the StateDataFormat
                    var state = context.Options.StateDataFormat.Protect(context.Properties);
                    var scope = string.Join(" ", context.Options.Scope);

                    var authUrl = $"{context.Options.AuthorizationEndpoint}" +
                        $"?client_id={Uri.EscapeDataString(actualClientId)}" +
                        $"&redirect_uri={Uri.EscapeDataString(redirectUri)}" +
                        $"&response_type=code" +
                        $"&scope={Uri.EscapeDataString(scope)}" +
                        $"&state={Uri.EscapeDataString(state)}";

                    // Continue with the recalculated OAuth flow
                    context.Response.Redirect(authUrl);

                };

                // Add callback handling for when OAuth provider redirects back
                options.Events.OnCreatingTicket = async context =>
                {
                    // Ensure we have fresh configuration for the callback as well
                    if (clientID == CLIENTID_DUMMY || clientSecret == CLIENTSECRET_DUMMY)
                    {
                        var configService = context.HttpContext.RequestServices.GetRequiredService<IConfigureTagzApp>();
                        var keys = await configService.GetConfigurationById<Dictionary<string, string>>($"Authentication:{name}");
                        
                        if (keys != null && keys.ContainsKey("ClientID") && keys.ContainsKey("ClientSecret"))
                        {
                            context.Options.ClientId = keys["ClientID"];
                            context.Options.ClientSecret = keys["ClientSecret"];
                        }
                    }
                };

                // Handle authentication failures
                options.Events.OnRemoteFailure = context =>
                {
                    // Log the error for debugging
                    var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                    logger.LogError("OAuth authentication failed for provider {Provider}: {Error}", name, context.Failure?.Message);
                    
                    // Redirect to a user-friendly error page
                    context.Response.Redirect($"/Account/ExternalLoginFailure?provider={name}&error={Uri.EscapeDataString(context.Failure?.Message ?? "Unknown error")}");
                    context.HandleResponse();
                    return Task.CompletedTask;
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
