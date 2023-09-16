using System.Net;
using System.Text.Json;
using System.Timers;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using TagzApp.Providers.Blazot.Configuration;
using TagzApp.Providers.Blazot.Constants;
using TagzApp.Providers.Blazot.Events;
using TagzApp.Providers.Blazot.Interfaces;
using TagzApp.Providers.Blazot.Models;

using Timer = System.Timers.Timer;

namespace TagzApp.Providers.Blazot.Services;

internal class HashtagTransmissionsService : ITransmissionsService, IDisposable
{
	private readonly int _WindowSeconds;
	private readonly Timer? _WindowTimer;
	private readonly HttpClient _HttpClient;
	private readonly IAuthService _AuthService;
	private readonly IAuthEvents _AuthEvents;
	private DateTime _SinceDate = DateTime.MinValue;
	private readonly ILogger<HashtagTransmissionsService> _Logger;

	public HashtagTransmissionsService(IConfiguration configuration, ILogger<HashtagTransmissionsService> logger,
		IHttpClientFactory httpClientFactory, IAuthEvents authEvents, IAuthService authService)
	{
		_Logger = logger ?? throw new ArgumentNullException(nameof(logger));
		_HttpClient = httpClientFactory.CreateClient(nameof(BlazotProvider));
		_AuthService = authService ?? throw new ArgumentNullException(nameof(authService));

		var settings = configuration.GetSection(BlazotSettings.AppSettingsSection).Get<BlazotSettings>();
		_WindowSeconds = settings?.WindowSeconds ?? throw new ArgumentNullException(nameof(settings));

		_WindowTimer = new Timer { Interval = TimeSpan.FromSeconds(_WindowSeconds).TotalMilliseconds, AutoReset = true };
		// TODO: Check CS8622 -- Nullability of reference types in type of parameter doesn't match the target delegate (possibly because of nullability attributes).
#pragma warning disable CS8622 // Nullability of reference types in type of parameter doesn't match the target delegate (possibly because of nullability attributes).
		_WindowTimer.Elapsed += HandleWindowTimerElapsed;

		_AuthEvents = authEvents ?? throw new ArgumentNullException(nameof(authEvents));
		_AuthEvents.AccessTokenUpdated += UpdateAuthorizationHeader;
#pragma warning restore CS8622 // Nullability of reference types in type of parameter doesn't match the target delegate (possibly because of nullability attributes).
	}

	public bool HasMadeTooManyRequests { get; private set; }

	public Task StartInitialWindowTimerAsync()
	{
		_WindowTimer?.Start();
		return Task.CompletedTask;
	}

	public async Task<List<Transmission>?> GetHashtagTransmissionsAsync(Hashtag tag, DateTimeOffset dateTimeOffset)
	{
		Exception? exception = null;
		List<Transmission>? transmissions = null;
		var maxTryCount = 4;

		if (_SinceDate == DateTime.MinValue)
			_SinceDate = dateTimeOffset.UtcDateTime;

		for (var i = 0; i < maxTryCount; i++)
		{
			try
			{
				var response = await RequestHashtagTransmissionsAsync(tag);
				if (response == null)
					throw new InvalidOperationException("Null response from Blazot.");

				var serializedResult = await response.Content.ReadAsStringAsync();
				transmissions = JsonSerializer.Deserialize<List<Transmission>>(serializedResult, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

				if (transmissions?.Count > 0)
					_SinceDate = transmissions.OrderByDescending(p => p.DateTransmitted).First().DateTransmitted;

				exception = null;
				break;
			}
			catch (UnauthorizedAccessException)
			{
				_Logger.LogInformation("Unauthorized request to Blazot. The app will attempt to refresh the access token. Attempt {count} of {total}", i + 1, maxTryCount);
				// If token has expired, request a new one.
				var response = await _AuthService.GetAccessTokenAsync();
				if (response.isSuccessStatusCode is null or false)
					break;
			}
			catch (InvalidOperationException ex) when (ex.Message.Contains("Rate limit exceeded"))
			{
				HasMadeTooManyRequests = true;
				var retrySeconds = _WindowTimer != null ? (int)TimeSpan.FromMilliseconds(_WindowTimer.Interval).TotalSeconds : _WindowSeconds;
				_Logger.LogWarning("Blazot rate limit exceeded. Will try again in {seconds} seconds", retrySeconds);
				break;
			}
			catch (Exception ex)
			{
				_Logger.LogError(ex, "Error fetching Blazot hashtag transmissions on attempt {count} of {total}: {message}", i, maxTryCount, ex.Message);
				await Task.Delay(3000);
			}
		}

		if (exception != null)
			throw exception;

		return transmissions;
	}

	private async Task<HttpResponseMessage> RequestHashtagTransmissionsAsync(Hashtag tag)
	{
		var uri = new Uri(Path.Combine(BlazotConstants.BaseAddress, $"hashtag?t={tag.Text.TrimStart('#')}&takeCount=20&sinceDate={_SinceDate.ToString("M/dd/yyyy H:mm:ss.fffffff tt")}").Replace(@"\", "/"));
		var response = await _HttpClient.GetAsync(uri);
		if (response.StatusCode == HttpStatusCode.Unauthorized)
			throw new UnauthorizedAccessException("Request to fetch results from Blazot was unauthorized.");

		if (response.StatusCode == HttpStatusCode.TooManyRequests)
		{
			await UpdateWindowTimerWithRetryHeaderAsync(response);
			throw new InvalidOperationException("Rate limit exceeded.");
		}

		if (response.StatusCode != HttpStatusCode.OK)
			throw new InvalidOperationException($"Blazot API returned a non-OK response: {response.ReasonPhrase}");

		return response;
	}

	private Task<double> UpdateWindowTimerWithRetryHeaderAsync(HttpResponseMessage? response)
	{
		var secondsUntilReset = response?.Headers.RetryAfter;
		if (secondsUntilReset == null)
			return Task.FromResult((double)_WindowSeconds);

		var isInt = int.TryParse(secondsUntilReset.ToString(), out var retrySeconds);
		if (isInt && _WindowTimer != null)
			_WindowTimer.Interval = TimeSpan.FromSeconds(retrySeconds).TotalMilliseconds;

		return Task.FromResult(isInt ? (double)retrySeconds : _WindowSeconds);
	}

	private void HandleWindowTimerElapsed(object sender, ElapsedEventArgs args) =>
		HasMadeTooManyRequests = false;

	private void UpdateAuthorizationHeader(object sender, EventArgs args)
	{
		_HttpClient.DefaultRequestHeaders.Remove("Authorization");
		_HttpClient.DefaultRequestHeaders.Add("Authorization", _AuthService.AccessToken);
	}

	public void Dispose()
	{
		_WindowTimer?.Dispose();
		// TODO: Check CS8622 -- Nullability of reference types in type of parameter doesn't match the target delegate (possibly because of nullability attributes).
#pragma warning disable CS8622 // Nullability of reference types in type of parameter doesn't match the target delegate (possibly because of nullability attributes).
		_AuthEvents.AccessTokenUpdated -= UpdateAuthorizationHeader;
#pragma warning restore CS8622 // Nullability of reference types in type of parameter doesn't match the target delegate (possibly because of nullability attributes).
	}
}
