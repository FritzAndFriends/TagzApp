using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using TagzApp.Storage.Postgres;
using Fritz.Charlie.Components.Services;
using TagzApp.Common.Models;
using TagzApp.Common;

namespace TagzApp.LocationMigration;

public class LocationMigrationService
{
	private readonly IDbContextFactory<TagzAppContext> _DbContextFactory;
	private readonly LocationTextService _locationTextService;
	private readonly SimpleGeocoder _geocoder;
	private readonly ILogger<LocationMigrationService> _logger;
	private readonly HashSet<string> _processedUsers = new();

	public LocationMigrationService(
			IDbContextFactory<TagzAppContext> DbContextFactory,
			SimpleGeocoder geocoder,
			ILoggerFactory loggerFactory,
			ILogger<LocationMigrationService> logger)
	{
		_DbContextFactory = DbContextFactory;
		_locationTextService = new LocationTextService(loggerFactory.CreateLogger<LocationTextService>());
		_geocoder = geocoder;
		_logger = logger;
	}

	public async Task<MigrationAnalysis> AnalyzeContentAsync()
	{
		_logger.LogInformation("Starting dry-run analysis of Content table...");

		var analysis = new MigrationAnalysis();

		using var dbContext = await _DbContextFactory.CreateDbContextAsync();

		try
		{
			// Load existing viewer locations to see how many users would be skipped
			await LoadExistingViewerLocations();
			analysis.ExistingUsers = _processedUsers.Count;

			// Load location cache to see what's already available
			await _geocoder.EnsureCacheLoadedAsync();
			var (_, _, cacheSize) = _geocoder.GetStatistics();

			// Query content records by provider with AsNoTracking for read-only operations
			var twitchContent = await dbContext.Content
					.AsNoTracking()
					.Where(c => c.Provider == "TWITCH")
					.ToListAsync();

			var youtubeContent = await dbContext.Content
					.AsNoTracking()
					.Where(c => c.Provider == "YOUTUBE-CHAT")
					.ToListAsync();

			analysis.TwitchRecords = twitchContent.Count;
			analysis.YouTubeRecords = youtubeContent.Count;
			analysis.TotalRecords = analysis.TwitchRecords + analysis.YouTubeRecords;

			_logger.LogInformation("Analyzing {TotalRecords} content records ({TwitchRecords} Twitch, {YouTubeRecords} YouTube-Chat)...",
					analysis.TotalRecords, analysis.TwitchRecords, analysis.YouTubeRecords);

			var allContent = twitchContent.Concat(youtubeContent);
			var usersToProcess = new HashSet<string>();

			foreach (var content in allContent)
			{
				try
				{
					var author = JsonSerializer.Deserialize<Creator>(content.Author);
					var userId = author?.UserName ?? "unknown";

					// Skip users who already have locations
					if (_processedUsers.Contains(userId))
						continue;

					usersToProcess.Add(userId);

					// Extract locations from the content text
					var locationText = _locationTextService.GetLocationText(content.Text);
					if (!string.IsNullOrWhiteSpace(locationText))
					{
						// Validate that the extracted text is actually a location
						if (SimpleGeocoder.IsValidLocation(locationText))
						{
							analysis.UniqueLocations.Add(locationText);
						}
						else
						{
							_logger.LogInformation($"[DRY-RUN] Skipping invalid location text: '{locationText}' from user {userId}");
						}
					}
				}
				catch (Exception ex)
				{
					_logger.LogWarning(ex, "Error analyzing content ID {ContentId}: {Error}", content.Id, ex.Message);
				}
			}

			analysis.UsersToProcess = usersToProcess.Count;

			// Check how many locations are already cached vs need geocoding
			var (cachedLocations, locationsToGeocode) = await _geocoder.AnalyzeLocationCacheAsync(analysis.UniqueLocations);
			analysis.CachedLocations = cachedLocations;
			analysis.LocationsToGeocode = locationsToGeocode;

			_logger.LogInformation("Dry-run analysis completed");
		}
		finally
		{
			// Ensure change tracker is cleared to free up memory and resources
			dbContext.ChangeTracker.Clear();
		}

		return analysis;
	}

	public async Task<int> MigrateLocationsAsync()
	{
		_logger.LogInformation("Starting location migration from Content table...");

		var locationsSaved = 0;

		using var readContext = await _DbContextFactory.CreateDbContextAsync();
		using var writeContext = await _DbContextFactory.CreateDbContextAsync();

		try
		{
			// First, load existing viewer locations to avoid duplicates
			await LoadExistingViewerLocations();

			// Get all content from Twitch and YouTube chat providers with AsNoTracking for read-only operations
			var contentQuery = readContext.Content
					.AsNoTracking()
					.Where(c => c.Provider == "TWITCH" || c.Provider == "YOUTUBE-CHAT")
					.OrderBy(c => c.Timestamp);

			var totalContent = await contentQuery.CountAsync();
			_logger.LogInformation($"Found {totalContent} content records from Twitch and YouTube Chat to process");

			var processedCount = 0;
			var locationsFound = 0;
			var batchSize = 50; // Save every 50 records
			var pendingLocations = new List<PgViewerLocation>();

			await foreach (var content in contentQuery.AsAsyncEnumerable())
			{
				processedCount++;

				if (processedCount % 100 == 0)
				{
					_logger.LogInformation($"Processed {processedCount}/{totalContent} records. Found {locationsFound} locations, saved {locationsSaved}.");
				}

				try
				{
					// Parse the author information from JSON
					var author = JsonSerializer.Deserialize<Creator>(content.Author);
					if (author == null)
					{
						_logger.LogWarning($"Could not parse author for content ID {content.Id}");
						continue;
					}

					// Create a consistent user identifier (fixing the existing issue)
					var userIdentifier = $"{content.Provider}-{author.DisplayName}";
					var userId = UserIdHasher.CreateHashedUserId(userIdentifier);

					// Skip if we've already processed this user (first location only)
					if (_processedUsers.Contains(userId))
					{
						continue;
					}

					// Extract location text using the Fritz.Charlie service
					var locationText = _locationTextService.GetLocationText(content.Text);

					if (string.IsNullOrWhiteSpace(locationText))
					{
						continue; // No location found in this message
					}

					// Validate that the extracted text is actually a location
					if (!SimpleGeocoder.IsValidLocation(locationText))
					{
						_logger.LogInformation($"Skipping invalid location text: '{locationText}' from user {userId}");
						continue; // Not a valid location
					}

					locationsFound++;
					_logger.LogDebug($"Found valid location '{locationText}' in message from user {userId}");

					// Geocode the location
					var location = await _geocoder.GeocodeAsync(locationText);

					if (location == null)
					{
						_logger.LogWarning($"Could not geocode location '{locationText}' for user {userId}");
						continue;
					}

					// Create ViewerLocation record
					var viewerLocation = new PgViewerLocation
					{
						StreamId = "all",
						HashedUserId = userId,
						Description = locationText,
						Latitude = location.Latitude,
						Longitude = location.Longitude
					};

					// Add to batch
					writeContext.ViewerLocations.Add(viewerLocation);
					pendingLocations.Add(viewerLocation);

					// Mark this user as processed
					_processedUsers.Add(userId);
					locationsSaved++;

					_logger.LogDebug($"Queued location for user {userId}: {locationText} at ({location.Latitude}, {location.Longitude})");

					// Save in batches to improve performance and avoid connection issues
					if (pendingLocations.Count >= batchSize)
					{
						try
						{
							await writeContext.SaveChangesAsync();
							_logger.LogInformation($"Saved batch of {pendingLocations.Count} locations to database");
							pendingLocations.Clear();
						}
						catch (Exception batchEx)
						{
							_logger.LogError(batchEx, $"Error saving batch of {pendingLocations.Count} locations");
							// Clear the context to avoid further issues
							writeContext.ChangeTracker.Clear();
							pendingLocations.Clear();
						}
					}
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, $"Error processing content ID {content.Id}: {ex.Message}");
				}
			}

			// Save any remaining locations in the final batch
			if (pendingLocations.Count > 0)
			{
				try
				{
					await writeContext.SaveChangesAsync();
					_logger.LogInformation($"Saved final batch of {pendingLocations.Count} locations to database");
				}
				catch (Exception finalEx)
				{
					_logger.LogError(finalEx, $"Error saving final batch of {pendingLocations.Count} locations");
				}
			}

			var (cacheHits, apiCalls, cacheSize) = _geocoder.GetStatistics();

			_logger.LogInformation($"Migration completed. Processed {processedCount} records, found {locationsFound} locations, saved {locationsSaved} unique user locations.");
			_logger.LogInformation($"Geocoding performance:");
			_logger.LogInformation($"  - Cache hits: {cacheHits}");
			_logger.LogInformation($"  - API calls: {apiCalls}");
			_logger.LogInformation($"  - Cache efficiency: {(cacheHits + apiCalls > 0 ? (double)cacheHits / (cacheHits + apiCalls) * 100 : 0):F1}%");
			_logger.LogInformation($"  - Final cache size: {cacheSize} locations");
		}
		finally
		{
			// Ensure change tracker is cleared and resources are released
			readContext.ChangeTracker.Clear();
			writeContext.ChangeTracker.Clear();
		}

		return locationsSaved;
	}

	public async Task LoadExistingViewerLocations()
	{
		_logger.LogInformation("Loading existing viewer locations to avoid duplicates...");

		using var dbContext = await _DbContextFactory.CreateDbContextAsync();

		try
		{
			var existingUserIds = await dbContext.ViewerLocations
					.AsNoTracking()
					.Select(vl => vl.HashedUserId)
					.ToListAsync();

			foreach (var userId in existingUserIds)
			{
				_processedUsers.Add(userId);
			}

			_logger.LogInformation($"Loaded {existingUserIds.Count} existing viewer locations");
		}
		finally
		{
			// Ensure change tracker is cleared to free up memory and resources
			dbContext.ChangeTracker.Clear();
		}
	}


}
