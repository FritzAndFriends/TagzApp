using Fritz.Charlie.Common;
using Fritz.Charlie.Components.Map;
using Fritz.Charlie.Components.Services;
using Microsoft.AspNetCore.SignalR;
using TagzApp.Blazor.Hubs;
using TagzApp.ViewModels.Data;
using TagzApp.Common;

namespace TagzApp.Blazor.Services;

public class SignalRNotifier : INotifyNewMessages
{
	private readonly IHubContext<MessageHub> _HubContext;
	private readonly IHubContext<ModerationHub, IModerationClient> _ModerationService;
	private readonly IViewerLocationService _viewerLocationService;
	private readonly LocationTextService _locationTextService;
	private readonly OpenStreetMapLocationService _GeolocationService;
	private readonly ILogger<SignalRNotifier> _locationLogger;
	private bool _ModerationEnabled = false;

	// Hybrid tracking - in-memory cache with database fallback
	private readonly ConcurrentHashSet<string> _usersWithLocations = new();
	private bool _cacheLoaded = false;
	private readonly SemaphoreSlim _cacheLoadSemaphore = new(1, 1);

	public SignalRNotifier(
			IHubContext<MessageHub> hubContext,
			IHubContext<ModerationHub, IModerationClient> moderationService,
			IViewerLocationService viewerLocationService,
			OpenStreetMapLocationService geoLocationService,
			ApplicationConfiguration appConfiguration,
			ILoggerFactory loggerFactory)
	{
		_HubContext = hubContext;
		_ModerationService = moderationService;
		_ModerationEnabled = appConfiguration.ModerationEnabled;
		_viewerLocationService = viewerLocationService;
		_locationTextService = new LocationTextService(loggerFactory.CreateLogger<LocationTextService>());
		_GeolocationService = geoLocationService;
		_locationLogger = loggerFactory.CreateLogger<SignalRNotifier>();
	}

	private async Task<string?> ExtractLocationFromContentAsync(Content content)
	{
		if (!content.Provider.Equals("TWITCH", StringComparison.InvariantCultureIgnoreCase) &&
				!content.Provider.Equals("YOUTUBE-CHAT", StringComparison.InvariantCultureIgnoreCase))
		{
			// only running on the live video streams
			return string.Empty;
		}

		var userId = $"{content.Provider}-{content.Author.UserName}";
		var hashedUserId = UserIdHasher.CreateHashedUserId(userId);

		// Ensure cache is loaded from database on first use
		await EnsureCacheLoaded();

		// Check in-memory cache first (fast path) - use hashed ID for consistency with database
		if (_usersWithLocations.Contains(hashedUserId))
		{
			_locationLogger.LogDebug($"User {userId} (hashed: {hashedUserId}) has already submitted a location (from cache), skipping");
			return string.Empty;
		}

		// Extract location text
		var locationText = _locationTextService.GetLocationText(content.Text);

		// If we found a location, mark this user as having submitted one
		if (!string.IsNullOrEmpty(locationText))
		{
			_usersWithLocations.Add(hashedUserId);
			_locationLogger.LogInformation($"First location submission from user {userId} (hashed: {hashedUserId}): {locationText}");
		}

		return locationText;
	}

	private async Task EnsureCacheLoaded()
	{
		if (_cacheLoaded) return;

		await _cacheLoadSemaphore.WaitAsync();
		try
		{
			if (_cacheLoaded) return; // Double-check after acquiring lock

			_locationLogger.LogInformation("Loading existing user locations from database into cache");

			var existingLocations = await _viewerLocationService.GetLocationsForStreamAsync("all");
			var userCount = 0;

			foreach (var location in existingLocations)
			{
				if (!string.IsNullOrEmpty(location.UserId))
				{
					_usersWithLocations.Add(location.UserId);
					userCount++;
				}
			}

			_cacheLoaded = true;
			_locationLogger.LogInformation($"Loaded {userCount} existing users with locations into cache");
		}
		catch (Exception ex)
		{
			_locationLogger.LogError(ex, "Failed to load existing locations from database");
			// Set as loaded anyway to prevent repeated failures
			_cacheLoaded = true;
		}
		finally
		{
			_cacheLoadSemaphore.Release();
		}
	}

	private async Task ExtractAndNotifyLocation(Content content)
	{
		try
		{
			// Use the async version now
			var locationText = await ExtractLocationFromContentAsync(content);
			if (!string.IsNullOrEmpty(locationText))
			{
				var geoPoint = await _GeolocationService.GetPointFromLocation(locationText);
				if (geoPoint.IsValid)
				{
					var userName = string.IsNullOrEmpty(content.Author.UserName) ? content.Author.DisplayName.Replace(" ", "_") : content.Author.UserName;
					var userId = $"{content.Provider}-{userName}";
					var locationEvent = new ViewerLocationEvent(
							geoPoint.Latitude,
							geoPoint.Longitude,
							locationText)
					{
						StreamId = "all",
						UserId = userId
					};

					// Store in database
					await _viewerLocationService.PlotLocationAsync(locationEvent);
				}
			}
		}
		catch (Exception ex)
		{
			_locationLogger.LogWarning(ex, "Error extracting location from content");
		}
	}

	// Update NotifyNewContent to use async properly
	public void NotifyNewContent(string hashtag, Content content)
	{
		if (_ModerationEnabled)
		{
			_ModerationService.Clients.All.NewWaterfallMessage((ContentModel)content);
		}
		else
		{
			_HubContext.Clients
					.Group(hashtag)
					.SendAsync("NewWaterfallMessage", (ContentModel)content);
		}

		// Fire and forget with proper error handling
		_ = Task.Run(async () =>
		{
			try
			{
				await ExtractAndNotifyLocation(content);
			}
			catch (Exception ex)
			{
				_locationLogger.LogError(ex, "Background location extraction failed");
			}
		});
	}

	public void NotifyApprovedContent(string hashtag, Content content, ModerationAction action)
	{

		_ModerationService.Clients.All.NewApprovedMessage(ModerationContentModel.ToModerationContentModel(content, action));

		Console.WriteLine($"Sending new waterfall message for tag {hashtag} for content: {content.Text}");

		_HubContext.Clients
				.Group(hashtag)
				.SendAsync("NewWaterfallMessage", (ContentModel)content);

	}

	public void NotifyRejectedContent(string hashtag, Content content, ModerationAction action)
	{

		_HubContext.Clients
				.Group(hashtag)
				.SendAsync("RemoveMessage", content.Provider, content.ProviderId);

		_ModerationService.Clients.All.NewRejectedMessage(ModerationContentModel.ToModerationContentModel(content, action));

	}

	public void NotifyNewBlockedCount(int blockedCount)
	{
		_ModerationService.Clients.All.NewBlockedUserCount(blockedCount);
	}
}
