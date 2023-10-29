using Microsoft.Extensions.Logging;
using TagzApp.Providers.Blazot.Constants;
using TagzApp.Providers.Blazot.Interfaces;
using TagzApp.Providers.Blazot.Models;
using TagzApp.Providers.Blazot.Configuration;

namespace TagzApp.Providers.Blazot;

internal sealed class BlazotProvider : ISocialMediaProvider
{
	private readonly int _WindowSeconds;
	private readonly int _WindowRequests;
	private readonly ILogger<BlazotProvider> _Logger;
	private readonly IContentConverter _ContentConverter;
	private readonly ITransmissionsService _TransmissionsService;
	private readonly IAuthService _AuthService;
	public TimeSpan NewContentRetrievalFrequency => TimeSpan.FromSeconds((double)_WindowSeconds / _WindowRequests);
	public string Id => BlazotConstants.ProviderId;
	public string DisplayName => BlazotConstants.DisplayName;

	private SocialMediaStatus _Status = SocialMediaStatus.Unhealthy;
	private string _StatusMessage = "Not started";

	public string Description { get; init; } = "Blazot is an all new social networking platform and your launchpad to the social universe!";

	public BlazotProvider(ILogger<BlazotProvider> logger, BlazotSettings settings,
		IContentConverter contentConverter, ITransmissionsService transmissionsService, IAuthService authService)
	{
		_ContentConverter = contentConverter ?? throw new ArgumentNullException(nameof(contentConverter));
		_TransmissionsService = transmissionsService ?? throw new ArgumentNullException(nameof(transmissionsService));
		_AuthService = authService ?? throw new ArgumentNullException(nameof(authService));
		_Logger = logger ?? throw new ArgumentNullException(nameof(logger));

		_WindowSeconds = settings?.WindowSeconds ?? throw new ArgumentNullException(nameof(settings));
		_WindowRequests = settings.WindowRequests;

		if (!string.IsNullOrWhiteSpace(settings.Description))
		{
			Description = settings.Description;
		}
	}

	public async Task<IEnumerable<Content>> GetContentForHashtag(Hashtag tag, DateTimeOffset dateTimeOffset)
	{
		var transmissions = new List<Transmission>();

		try
		{
			// Throttle requests in case of exceeded rate.
			// Requesting multiple tags within a loop could make more requests than expected.
			// Some refactoring could be done to request multiple tags if app is adjusted to send them in a single call.
			// A Blazot hashtags request can currently accept up to 10 hashtags with "?t=sometag&t=anothertag".
			if (_TransmissionsService.HasMadeTooManyRequests)
			{
				_Logger.LogInformation("Exited Blazot request due to current rate limit exceeded state.");
				return Enumerable.Empty<Content>();
			}

			// Get initial access token if empty.
			if (string.IsNullOrWhiteSpace(_AuthService.AccessToken))
			{
				await _TransmissionsService.StartInitialWindowTimerAsync();

				var (isSuccessStatusCode, _) = await _AuthService.GetAccessTokenAsync();
				if (isSuccessStatusCode is null or false)
					return Enumerable.Empty<Content>();
			}

			transmissions = await _TransmissionsService.GetHashtagTransmissionsAsync(tag, dateTimeOffset);

			_Status = SocialMediaStatus.Healthy;
			_StatusMessage = "OK";

			if (transmissions == null)
				return Enumerable.Empty<Content>();

		}
		catch (Exception ex)
		{
			_Logger.LogError(ex, "Error fetching Blazot Hashtag Transmissions: {message}", ex.Message);

			_Status = SocialMediaStatus.Unhealthy;
			_StatusMessage = $"Error fetching Blazot Hashtag Transmissions: {ex.Message}";

		}

		return _ContentConverter.ConvertToContent(transmissions, tag);
	}

	public Task StartAsync()
	{
		return Task.CompletedTask;
	}

	public Task<(SocialMediaStatus Status, string Message)> GetHealth() => Task.FromResult((_Status, _StatusMessage));
}
