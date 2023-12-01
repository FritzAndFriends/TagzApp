using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using TagzApp.Providers.Blazot.Constants;
using TagzApp.Providers.Blazot.Events;
using TagzApp.Providers.Blazot.Interfaces;
using TagzApp.Providers.Blazot.Models;
using TagzApp.Providers.Blazot.Configuration;

namespace TagzApp.Providers.Blazot.Services;

internal class AuthService : IAuthService
{
	private readonly string? _ApiKey;
	private readonly string? _SecretAuthKey;
	private readonly HttpClient _HttpClient;
	private readonly IAuthEvents _AuthEvents;
	private readonly ILogger<AuthService> _Logger;

	public AuthService(ILogger<AuthService> logger, IHttpClientFactory httpClientFactory, IAuthEvents authEvents, BlazotConfiguration configuration)
	{
		_Logger = logger ?? throw new ArgumentNullException(nameof(logger));
		_HttpClient = httpClientFactory.CreateClient(nameof(BlazotProvider));
		_AuthEvents = authEvents ?? throw new ArgumentNullException(nameof(authEvents));

		_ApiKey = configuration?.ApiKey ?? throw new ArgumentNullException(nameof(configuration));
		_HttpClient.DefaultRequestHeaders.Add("x-api-key", _ApiKey);
		_SecretAuthKey = configuration.SecretAuthKey;
	}

	public string? AccessToken { get; private set; }

	public async Task<(bool? isSuccessStatusCode, HttpStatusCode? httpStatusCode)> GetAccessTokenAsync()
	{
		HttpResponseMessage? response = null;

		try
		{
			if (string.IsNullOrEmpty(_ApiKey))
				throw new InvalidOperationException("Blazot API Key is missing from app settings.");

			if (string.IsNullOrWhiteSpace(_SecretAuthKey))
				throw new InvalidOperationException("Blazot Secret Auth Key is missing from app settings.");

			var address = Path.Combine(BlazotConstants.BaseAddress, "auth/token").Replace(@"\", "/");
			var uri = new Uri(address);

			_HttpClient.DefaultRequestHeaders.Add("x-secret-auth-key", _SecretAuthKey);
			response = await _HttpClient.GetAsync(uri);
			if (response.IsSuccessStatusCode)
			{
				var serializedResult = await response.Content.ReadAsStringAsync();
				var token = JsonSerializer.Deserialize<Token>(serializedResult, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
				AccessToken = $"Bearer {token?.AccessToken}";
				_AuthEvents.NotifyAccessTokenUpdated();
			}
			else
			{
				_Logger.LogWarning("Blazot access token request failed: {reason}", response.ReasonPhrase);
			}
		}
		catch (Exception ex)
		{
			_Logger.LogError(ex, "Failed to get Blazot Access Token: {message}", ex.Message);
		}
		finally
		{
			_HttpClient.DefaultRequestHeaders.Remove("x-secret-auth-key");
		}

		return (response?.IsSuccessStatusCode, response?.StatusCode);
	}
}
