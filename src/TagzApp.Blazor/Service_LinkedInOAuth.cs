using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using TagzApp.Providers.LinkedIn;

namespace TagzApp.Blazor;

public static class Service_LinkedInOAuth
{
	private const string LinkedInAuthorizationEndpoint = "https://www.linkedin.com/oauth/v2/authorization";
	private const string LinkedInTokenEndpoint = "https://www.linkedin.com/oauth/v2/accessToken";

	// Required scopes for LinkedIn Marketing API hashtag search
	private static readonly string[] RequiredScopes = ["openid", "profile", "w_member_social", "r_organization_social"];

	public static void MapLinkedInOAuthEndpoints(this WebApplication app)
	{
		// Admin-initiated OAuth authorization endpoint
		app.MapGet("/api/linkedin/authorize", async (HttpContext context, IConfigureTagzApp config) =>
		{
			// Require authentication - only admins should initiate provider OAuth
			if (!context.User.Identity?.IsAuthenticated ?? true)
			{
				return Results.Unauthorized();
			}

			// Verify user has admin role
			if (!context.User.IsInRole(RolesAndPolicies.Role.Admin))
			{
				return Results.Forbid();
			}

			// Load LinkedIn configuration to get ClientId
			var linkedInConfig = await config.GetConfigurationById<LinkedInConfiguration>(LinkedInConfiguration.AppSettingsSection);
			if (linkedInConfig == null || string.IsNullOrEmpty(linkedInConfig.ClientId))
			{
				return Results.BadRequest(new { error = "LinkedIn provider not configured. Please set Client ID and Client Secret first." });
			}

			// Build callback URL (must match LinkedIn Developer App settings)
			var request = context.Request;
			var scheme = request.Headers.ContainsKey("X-Forwarded-Proto")
				? request.Headers["X-Forwarded-Proto"].ToString()
				: request.Scheme;

			// Ensure HTTPS for OAuth security
			if (scheme.Equals("http", StringComparison.OrdinalIgnoreCase))
			{
				scheme = "https";
			}

			var host = request.Headers.ContainsKey("X-Forwarded-Host")
				? request.Headers["X-Forwarded-Host"].ToString()
				: request.Host.ToString();

			var redirectUri = $"{scheme}://{host}/api/linkedin/callback";

			// Generate state parameter for CSRF protection
			var state = Guid.NewGuid().ToString("N");
			var stateData = new
			{
				state,
				userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier),
				timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
			};

			// Store state in session or auth properties for verification in callback
			var authProperties = new AuthenticationProperties
			{
				Items = { ["LinkedInOAuthState"] = JsonSerializer.Serialize(stateData) }
			};
			await context.SignInAsync(IdentityConstants.ApplicationScheme, context.User, authProperties);

			// Build authorization URL
			var scope = string.Join(" ", RequiredScopes);
			var authUrl = $"{LinkedInAuthorizationEndpoint}" +
				$"?response_type=code" +
				$"&client_id={Uri.EscapeDataString(linkedInConfig.ClientId)}" +
				$"&redirect_uri={Uri.EscapeDataString(redirectUri)}" +
				$"&scope={Uri.EscapeDataString(scope)}" +
				$"&state={Uri.EscapeDataString(state)}";

			return Results.Redirect(authUrl);
		}).RequireAuthorization();

		// OAuth callback endpoint (LinkedIn redirects here with authorization code)
		app.MapGet("/api/linkedin/callback", async (
			HttpContext context,
			IConfigureTagzApp config,
			IHttpClientFactory httpClientFactory,
			string? code,
			string? state,
			string? error,
			string? error_description) =>
		{
			// Handle OAuth errors from LinkedIn
			if (!string.IsNullOrEmpty(error))
			{
				var errorMessage = Uri.EscapeDataString(error_description ?? error);
				return Results.Redirect($"/admin?linkedin_oauth_error={errorMessage}");
			}

			// Validate required parameters
			if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(state))
			{
				return Results.Redirect("/admin?linkedin_oauth_error=Missing+authorization+code+or+state");
			}

			// Verify state parameter for CSRF protection
			var authResult = await context.AuthenticateAsync(IdentityConstants.ApplicationScheme);
			if (!authResult.Succeeded ||
				!authResult.Properties.Items.TryGetValue("LinkedInOAuthState", out var storedStateJson))
			{
				return Results.Redirect("/admin?linkedin_oauth_error=Invalid+OAuth+state");
			}

			var storedState = JsonSerializer.Deserialize<JsonElement>(storedStateJson);
			if (storedState.GetProperty("state").GetString() != state)
			{
				return Results.Redirect("/admin?linkedin_oauth_error=State+mismatch+CSRF+protection+failed");
			}

			// Load LinkedIn configuration
			var linkedInConfig = await config.GetConfigurationById<LinkedInConfiguration>(LinkedInConfiguration.AppSettingsSection);
			if (linkedInConfig == null || string.IsNullOrEmpty(linkedInConfig.ClientId) || string.IsNullOrEmpty(linkedInConfig.ClientSecret))
			{
				return Results.Redirect("/admin?linkedin_oauth_error=LinkedIn+provider+not+configured");
			}

			// Build callback URL (must match authorize endpoint)
			var request = context.Request;
			var scheme = request.Headers.ContainsKey("X-Forwarded-Proto")
				? request.Headers["X-Forwarded-Proto"].ToString()
				: request.Scheme;

			if (scheme.Equals("http", StringComparison.OrdinalIgnoreCase))
			{
				scheme = "https";
			}

			var host = request.Headers.ContainsKey("X-Forwarded-Host")
				? request.Headers["X-Forwarded-Host"].ToString()
				: request.Host.ToString();

			var redirectUri = $"{scheme}://{host}/api/linkedin/callback";

			try
			{
				// Exchange authorization code for access token
				var tokenResponse = await ExchangeCodeForTokens(
					httpClientFactory,
					linkedInConfig.ClientId,
					linkedInConfig.ClientSecret,
					code,
					redirectUri);

				if (tokenResponse == null)
				{
					return Results.Redirect("/admin?linkedin_oauth_error=Failed+to+exchange+authorization+code");
				}

				// Store tokens in LinkedInConfiguration
				linkedInConfig.AccessToken = tokenResponse.AccessToken;
				linkedInConfig.RefreshToken = tokenResponse.RefreshToken ?? string.Empty;

				// Calculate token expiration (LinkedIn tokens expire in 60 days)
				var expiresAt = DateTimeOffset.UtcNow.AddSeconds(tokenResponse.ExpiresIn);
				linkedInConfig.TokenExpiresAt = expiresAt.ToString("O"); // ISO 8601 format

				// Save configuration with encrypted tokens
				await config.SetConfigurationById(LinkedInConfiguration.AppSettingsSection, linkedInConfig);

				// Redirect back to admin page with success message
				return Results.Redirect("/admin?linkedin_oauth_success=true");
			}
			catch (Exception ex)
			{
				var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
				logger.LogError(ex, "Error during LinkedIn OAuth callback");
				return Results.Redirect($"/admin?linkedin_oauth_error={Uri.EscapeDataString(ex.Message)}");
			}
		});
	}

	private static async Task<LinkedInTokenResponse?> ExchangeCodeForTokens(
		IHttpClientFactory httpClientFactory,
		string clientId,
		string clientSecret,
		string code,
		string redirectUri)
	{
		var httpClient = httpClientFactory.CreateClient();

		// Build token request
		var tokenRequestBody = new FormUrlEncodedContent(new Dictionary<string, string>
		{
			["grant_type"] = "authorization_code",
			["code"] = code,
			["redirect_uri"] = redirectUri,
			["client_id"] = clientId,
			["client_secret"] = clientSecret
		});

		var response = await httpClient.PostAsync(LinkedInTokenEndpoint, tokenRequestBody);

		if (!response.IsSuccessStatusCode)
		{
			var errorBody = await response.Content.ReadAsStringAsync();
			throw new Exception($"LinkedIn token exchange failed: {response.StatusCode} - {errorBody}");
		}

		var tokenData = await response.Content.ReadFromJsonAsync<LinkedInTokenResponse>();
		return tokenData;
	}

	private class LinkedInTokenResponse
	{
		[System.Text.Json.Serialization.JsonPropertyName("access_token")]
		public string AccessToken { get; set; } = string.Empty;

		[System.Text.Json.Serialization.JsonPropertyName("expires_in")]
		public int ExpiresIn { get; set; }

		[System.Text.Json.Serialization.JsonPropertyName("refresh_token")]
		public string? RefreshToken { get; set; }

		[System.Text.Json.Serialization.JsonPropertyName("refresh_token_expires_in")]
		public int? RefreshTokenExpiresIn { get; set; }
	}
}
