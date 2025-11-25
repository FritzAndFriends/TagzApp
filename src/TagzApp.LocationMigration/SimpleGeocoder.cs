using System.Net.Http;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using TagzApp.Common.Models;
using TagzApp.Blazor.Services;
using TagzApp.Storage.Postgres;
using Microsoft.EntityFrameworkCore;

namespace TagzApp.LocationMigration;

public class SimpleGeocoder
{
	private readonly HttpClient _httpClient;
	private readonly ILogger<SimpleGeocoder> _logger;
	private readonly TagzAppContext _dbContext;
	private static readonly TimeSpan RateLimitDelay = TimeSpan.FromSeconds(1);
	private DateTime _lastRequestTime = DateTime.MinValue;
	private readonly Dictionary<string, GeographicPoint> _locationCache = new(StringComparer.OrdinalIgnoreCase);
	private bool _cacheLoaded = false;
	private int _cacheHits = 0;
	private int _apiCalls = 0;

	public SimpleGeocoder(HttpClient httpClient, ILogger<SimpleGeocoder> logger, TagzAppContext dbContext)
	{
		_httpClient = httpClient;
		_logger = logger;
		_dbContext = dbContext;

		// Set a user agent for Nominatim API
		_httpClient.DefaultRequestHeaders.Add("User-Agent", "TagzApp-LocationMigration/1.0");
	}

	public async Task EnsureCacheLoadedAsync()
	{
		if (_cacheLoaded) return;

		try
		{
			_logger.LogInformation("Loading existing locations from database into cache...");
			var existingLocations = await _dbContext.Locations.ToListAsync();

			foreach (var location in existingLocations)
			{
				var point = new GeographicPoint
				{
					Latitude = location.Latitude,
					Longitude = location.Longitude,
					Name = location.Name
				};
				_locationCache[location.Name] = point;
			}

			_cacheLoaded = true;
			_logger.LogInformation("Loaded {Count} existing locations into cache", _locationCache.Count);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error loading location cache from database");
			_cacheLoaded = true; // Don't keep trying on every request
		}
	}

	public async Task<GeographicPoint?> GeocodeAsync(string locationText)
	{
		await EnsureCacheLoadedAsync();

		// Check cache first
		if (_locationCache.TryGetValue(locationText, out var cachedLocation))
		{
			_cacheHits++;
			_logger.LogDebug("Found '{LocationText}' in cache at {Lat}, {Lon} (Cache hit #{CacheHits})",
					locationText, cachedLocation.Latitude, cachedLocation.Longitude, _cacheHits);
			return cachedLocation;
		}
		try
		{
			// Respect rate limiting
			var timeSinceLastRequest = DateTime.UtcNow - _lastRequestTime;
			if (timeSinceLastRequest < RateLimitDelay)
			{
				var delayTime = RateLimitDelay - timeSinceLastRequest;
				await Task.Delay(delayTime);
			}
			_lastRequestTime = DateTime.UtcNow;

			var encodedLocation = Uri.EscapeDataString(locationText);
			var url = $"https://nominatim.openstreetmap.org/search?format=json&limit=1&q={encodedLocation}";

			_apiCalls++;
			_logger.LogDebug("Geocoding: {LocationText} (API call #{ApiCalls})", locationText, _apiCalls);

			var response = await _httpClient.GetAsync(url);
			response.EnsureSuccessStatusCode();

			var jsonResponse = await response.Content.ReadAsStringAsync();
			var results = JsonSerializer.Deserialize<NominatimResult[]>(jsonResponse);

			if (results?.Length > 0)
			{
				var result = results[0];
				if (decimal.TryParse(result.lat, out var lat) && decimal.TryParse(result.lon, out var lon))
				{
					var point = new GeographicPoint { Latitude = lat, Longitude = lon, Name = locationText };

					// Add to cache
					_locationCache[locationText] = point;

					// Save to database for future use
					try
					{
						var pgLocation = new PgGeolocation
						{
							Name = locationText,
							Latitude = lat,
							Longitude = lon
						};

						_dbContext.Locations.Add(pgLocation);
						await _dbContext.SaveChangesAsync();

						_logger.LogInformation("Geocoded and saved '{LocationText}' to {Lat}, {Lon}", locationText, lat, lon);
					}
					catch (Exception ex)
					{
						// Don't fail if we can't save to cache table, just log it
						_logger.LogWarning(ex, "Could not save location '{LocationText}' to cache table: {Error}", locationText, ex.Message);
						_logger.LogInformation("Geocoded '{LocationText}' to {Lat}, {Lon} (cache save failed)", locationText, lat, lon);
					}

					return point;
				}
			}

			_logger.LogWarning("No geocoding results for: {LocationText}", locationText);
			return null;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error geocoding location: {LocationText}", locationText);
			return null;
		}
	}

	private class NominatimResult
	{
		public string lat { get; set; } = string.Empty;
		public string lon { get; set; } = string.Empty;
		public string display_name { get; set; } = string.Empty;
	}

	public async Task<(int cachedLocations, int locationsToGeocode)> AnalyzeLocationCacheAsync(HashSet<string> locations)
	{
		await EnsureCacheLoadedAsync();

		var cachedCount = 0;
		var needGeocoding = 0;

		foreach (var location in locations)
		{
			if (_locationCache.ContainsKey(location))
			{
				cachedCount++;
			}
			else
			{
				needGeocoding++;
			}
		}

		return (cachedCount, needGeocoding);
	}

	public (int cacheHits, int apiCalls, int cacheSize) GetStatistics()
	{
		return (_cacheHits, _apiCalls, _locationCache.Count);
	}

	/// <summary>
	/// Validates if the extracted text is likely to be a real location
	/// </summary>
	public static bool IsValidLocation(string locationText)
	{
		if (string.IsNullOrWhiteSpace(locationText))
			return false;

		// Convert to lowercase for easier pattern matching
		var lower = locationText.ToLowerInvariant().Trim();

		// Filter out common non-location patterns
		var invalidPatterns = new[]
		{
            // Technical terms and commands
            "dotnet", "code", "debug", "build", "run", "test", "deploy", "git", "commit", "push", "pull",
						"npm", "yarn", "docker", "kubernetes", "azure", "aws", "cloud", "server", "api", "http", "https",
						"json", "xml", "html", "css", "javascript", "typescript", "csharp", "python", "java",
            
            // Common chat expressions
            "same", "lol", "haha", "yeah", "yes", "no", "ok", "okay", "thanks", "thank you",
						"hello", "hi", "hey", "bye", "goodbye", "good morning", "good night",
						"we're", "it's", "that's", "i'm", "you're", "they're", "there's", "here's",
            
            // Time-related (often confused as locations)
            "today", "tomorrow", "yesterday", "now", "later", "soon", "never", "always",
						"morning", "afternoon", "evening", "night", "midnight", "noon",
            
            // Programming specific
            "function", "method", "class", "interface", "variable", "property", "field",
						"if", "else", "for", "while", "switch", "case", "break", "continue", "return",
						"try", "catch", "finally", "throw", "using", "namespace", "public", "private",
						"static", "async", "await", "task", "thread", "lock", "mutex",
            
            // Common phrases that aren't locations
            "party", "getting set", "only a", "fritz is", "let me", "can you", "will you",
						"should we", "could we", "would you", "did you", "have you", "are you",
						"this is", "that is", "there is", "here is", "what is", "who is", "when is",
            
            // File extensions and paths
            ".cs", ".js", ".ts", ".html", ".css", ".json", ".xml", ".txt", ".md", ".yml", ".yaml",
						".exe", ".dll", ".zip", ".rar", ".pdf", ".doc", ".docx", ".xls", ".xlsx"
				};

		// Check if the location text contains any invalid patterns
		foreach (var pattern in invalidPatterns)
		{
			if (lower.Contains(pattern))
				return false;
		}

		// Additional heuristics for valid locations

		// Too short (likely abbreviations or tech terms)
		if (lower.Length < 2)
			return false;

		// Too long (likely sentences or descriptions)
		if (lower.Length > 50)
			return false;

		// Contains too many numbers (likely coordinates, IDs, or version numbers)
		var digitCount = lower.Count(char.IsDigit);
		if (digitCount > lower.Length / 2) // More than half digits
			return false;

		// Contains programming operators or symbols
		if (lower.Contains("==") || lower.Contains("!=") || lower.Contains("&&") ||
				lower.Contains("||") || lower.Contains("=>") || lower.Contains("->") ||
				lower.Contains("<=") || lower.Contains(">=") || lower.Contains("++") ||
				lower.Contains("--") || lower.Contains("+=") || lower.Contains("-="))
			return false;

		// Check for common location indicators (positive signals)
		var locationIndicators = new[]
		{
            // Geographic terms
            "city", "town", "village", "county", "state", "province", "country", "region",
						"island", "coast", "beach", "mountain", "hill", "valley", "river", "lake",
						"north", "south", "east", "west", "northern", "southern", "eastern", "western",
            
            // Known location patterns
            "new ", "old ", "upper ", "lower ", "greater ", "little ", "big ", "saint ", "st ",
						" city", " town", " beach", " valley", " hills", " mountains", " island"
				};

		var hasLocationIndicator = locationIndicators.Any(indicator => lower.Contains(indicator));

		// Known world cities, countries (basic list - could be expanded)
		var knownLocations = new[]
		{
						"london", "paris", "tokyo", "sydney", "toronto", "vancouver", "montreal",
						"chicago", "seattle", "portland", "austin", "denver", "atlanta", "miami",
						"boston", "philadelphia", "detroit", "phoenix", "dallas", "houston",
						"california", "texas", "florida", "new york", "washington", "oregon",
						"canada", "usa", "uk", "australia", "germany", "france", "japan", "brazil",
						"mexico", "spain", "italy", "netherlands", "sweden", "norway", "finland"
				};

		var isKnownLocation = knownLocations.Any(known => lower.Contains(known));

		// If it has location indicators or is a known location, it's likely valid
		if (hasLocationIndicator || isKnownLocation)
			return true;

		// Default: if it passes all the negative filters and isn't too weird, allow it
		// This catches legitimate place names that aren't in our known list
		return !lower.Any(c => char.IsDigit(c) || char.IsPunctuation(c) || char.IsSymbol(c)) ||
					 lower.All(c => char.IsLetterOrDigit(c) || char.IsWhiteSpace(c) || c == '.' || c == ',' || c == '-' || c == '\'');
	}
}
