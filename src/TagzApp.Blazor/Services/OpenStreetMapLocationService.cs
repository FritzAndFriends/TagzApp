using System.Collections.Concurrent;
using Fritz.Charlie.Components.Map;
using System.Diagnostics;

namespace TagzApp.Blazor.Services;

public class OpenStreetMapLocationService
{

	
	private readonly Task _processingTask;
	private readonly CancellationTokenSource _cancellationTokenSource;

	private readonly IViewerLocationService _db;
	private readonly ILocationRepository _locationDb;
	private readonly ILogger<OpenStreetMapLocationService> _logger;
	private readonly System.Net.Http.IHttpClientFactory _httpClientFactory;
	private static readonly TimeSpan RateLimitDelay = TimeSpan.FromSeconds(1);
	private DateTime _lastRequestTime = DateTime.MinValue;
	private static readonly ConcurrentQueue<GeocodeRequest> _LocationQueue = new();

	public OpenStreetMapLocationService(
			ILogger<OpenStreetMapLocationService> logger,
			System.Net.Http.IHttpClientFactory httpClientFactory,
			IViewerLocationService db,
			ILocationRepository locationDb)
	{
		_logger = logger;
		_httpClientFactory = httpClientFactory;
		_db = db;
		_locationDb = locationDb;
		_cancellationTokenSource = new CancellationTokenSource();

		// Start the background processing task
		_processingTask = Task.Run(ProcessGeocodeRequestsAsync, _cancellationTokenSource.Token);

		_logger.LogInformation("NominatimGeocoder initialized with background processing queue");
	}

	public async Task<GeographicPoint> GetPointFromLocation(string location, bool force = false)
	{
		using var activity = MapFeatureHelper.Source.StartActivity("GeocodeLocation", ActivityKind.Client);
		var stopwatch = Stopwatch.StartNew();
		activity?.SetTag("Location", location);
		if (activity != null)
		{
			activity.DisplayName = location;
		}

		try
		{
			// Check cache first unless forced
			if (!force)
			{
				var cachedValue = await _locationDb.GetLocationFromTable(location);
				if (!string.IsNullOrEmpty(cachedValue.Description))
				{
					var cachedPoint = new GeographicPoint
					{
						Name = location,
						Latitude = cachedValue.Latitude,
						Longitude = cachedValue.Longitude,
						IsRealLocation = true
					};

					_logger.LogDebug($"Retrieved location {location} from cache");
					activity?.SetTag("FromCache", true);
					return cachedPoint;
				}
			}

			// Always add to queue first
			var request = new GeocodeRequest(location);
			_LocationQueue.Enqueue(request);
			
			var currentQueueSize = _LocationQueue.Count;
			_logger.LogDebug($"Queued geocoding request for location: {location} (queue size: {currentQueueSize})");

			// If queue is very long, return delayed result immediately without waiting
			if (currentQueueSize > 20) // Configurable threshold
			{
				_logger.LogWarning($"Geocoding queue is very long ({currentQueueSize}) - returning delayed result immediately for {location}");
				return new GeographicPoint 
				{ 
					Name = location, 
					IsDelayed = true 
				};
			}

			// Wait for the result with a shorter timeout when queue is getting large
			var timeout = currentQueueSize switch
			{
				> 10 => TimeSpan.FromSeconds(20), // Medium timeout for moderate queues
				> 5 => TimeSpan.FromSeconds(10), // Very short timeout for large queues
				_ => TimeSpan.FromMinutes(1)     // Full timeout for small queues
			};

			using var resultCts = new CancellationTokenSource(timeout);
			var point = await request.CompletionSource.Task.WaitAsync(resultCts.Token);

			// Cache the result if valid
			if (point.IsValid)
			{
				await _locationDb.AddLocationToTable(location, point.Latitude, point.Longitude);
				_logger.LogInformation($"Cached location {location} in Db");
			}

			activity?.SetTag("FromCache", false);
			activity?.SetTag("IsValid", point.IsValid);
			return point;
		}
		catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
		{
			activity?.AddException(ex);
			_logger.LogWarning(ex, $"Timeout geocoding location: {location} - returning delayed result");
			// Return delayed result instead of throwing
			return new GeographicPoint 
			{ 
				Name = location, 
				IsDelayed = true 
			};
		}
		catch (OperationCanceledException ex)
		{
			activity?.AddException(ex);
			_logger.LogWarning(ex, $"Geocoding request cancelled for location: {location} - returning delayed result");
			// Return delayed result instead of throwing
			return new GeographicPoint 
			{ 
				Name = location, 
				IsDelayed = true 
			};
		}
		catch (HttpRequestException ex)
		{
			activity?.AddException(ex);
			_logger.LogError(ex, $"HTTP error geocoding location: {location}");
			throw new Exception($"Unable to find location '{location}' - network error");
		}
		catch (Exception ex)
		{
			activity?.AddException(ex);
			_logger.LogError(ex, $"Error geocoding location: {location}");
			throw new Exception(location);
		}
		finally
		{
			stopwatch.Stop();
			var tags = new Dictionary<string, object?>
			{
				{ "Location", location },
				{ "FromCache", activity?.GetTagItem("FromCache") ?? false }
			};
			MapFeatureHelper.MapLookupCount.Record(stopwatch.ElapsedMilliseconds, tags.ToArray());
		}
	}

	private async Task ProcessGeocodeRequestsAsync()
	{

		_logger.LogInformation("Starting geocoding request processing loop");
		var lastRequestTime = DateTime.MinValue;

		try
		{
			while (true)
			{
				GeocodeRequest? location = null;
				GeographicPoint? result = null;

				try
				{

					// Check for values
					if (_LocationQueue.IsEmpty)
					{
						await Task.Delay(500, _cancellationTokenSource.Token);
						continue;
					}

					if (!_LocationQueue.TryDequeue(out location))
					{
						await Task.Delay(100, _cancellationTokenSource.Token);
						continue;
					}

					if (location is null || string.IsNullOrWhiteSpace(location.Location))
					{
						await Task.Delay(100, _cancellationTokenSource.Token);
						continue;
					}

					// Rate limiting - ensure at least 1 second between requests
					var timeSinceLastRequest = DateTime.UtcNow - lastRequestTime;
					if (timeSinceLastRequest < RateLimitDelay)
					{
						var delay = RateLimitDelay - timeSinceLastRequest;
						_logger.LogDebug($"Rate limiting geocoding request for {location.Location} - delaying {delay}");
						await Task.Delay(delay, _cancellationTokenSource.Token);
					}

					lastRequestTime = DateTime.UtcNow;

					result = await FetchGeographicLocationFromNominatim(location.Location);

					_logger.LogInformation($"Successfully completed geocoding request for '{location.Location}' - IsValid: {result.IsValid}, Lat: {result.Latitude}, Lng: {result.Longitude}");
				}
				catch (OperationCanceledException)
				{
					// Service is shutting down
					break;
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, $"Error processing geocoding request for {location?.Location}");
					// Create an invalid result for the error case
					result = new GeographicPoint(); // Invalid point
				}
				finally
				{
					// Complete the request if we have one
					if (location?.CompletionSource is not null)
					{
						if (!_cancellationTokenSource.IsCancellationRequested)
						{
							if (result is not null)
							{
								// We have a result (valid or invalid)
								location.CompletionSource.TrySetResult(result);
							}
							else
							{
								// No result due to some error - return invalid point
								_logger.LogWarning($"No result available for geocoding request: {location.Location}");
								location.CompletionSource.TrySetResult(new GeographicPoint());
							}
						}
						else
						{
							location.CompletionSource.TrySetCanceled();
						}
					}
				}

				await Task.Delay(100, _cancellationTokenSource.Token);

			}
		}
		catch (OperationCanceledException)
		{
			_logger.LogInformation("Geocoding request processing loop cancelled");
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Unexpected error in geocoding request processing loop");
		}
		finally
		{
			_logger.LogInformation("Geocoding request processing loop ended");
		}
	}

	private async Task<GeographicPoint> FetchGeographicLocationFromNominatim(string location)
	{
		_logger.LogInformation($"Fetching location {location} from Nominatim");

		var client = _httpClientFactory.CreateClient();
		client.Timeout = TimeSpan.FromSeconds(15);
		var encodedLocation = Uri.EscapeDataString(location);
		var url = $"https://nominatim.openstreetmap.org/search?q={encodedLocation}&format=json&limit=3&addressdetails=1";

		client.DefaultRequestHeaders.Clear();
		client.DefaultRequestHeaders.Add("User-Agent", "TagzApp.net - Social Media aggregator");

		var response = await client.GetFromJsonAsync<NominatimLocation[]>(url, _cancellationTokenSource.Token);

		if (response == null || response.Length == 0)
		{
			_logger.LogWarning($"No results found for location: {location}");
			throw new Exception($"Unable to find location '{location}' - no geocoding results found");
		}

		// Get the most important result (highest importance score)
		var bestResult = response.OrderByDescending(r => r.importance).First();

		if (decimal.TryParse(bestResult.lat, out var latitude) &&
				decimal.TryParse(bestResult.lon, out var longitude))
		{
			var point = new GeographicPoint
			{
				Latitude = latitude,
				Longitude = longitude,
				Name = location,
				IsRealLocation = true
			};

			_logger.LogInformation($"Successfully geocoded {location} to {latitude}, {longitude}");
			return point;
		}
		else
		{
			_logger.LogError($"Invalid coordinates returned for location: {location}");
			return new GeographicPoint(); // Invalid point
		}
	}

	public void Dispose()
	{
		_logger.LogInformation("Disposing NominatimGeocoder");

		// Signal completion and wait for processing to finish
		_cancellationTokenSource.Cancel();

		try
		{
			_processingTask.Wait(TimeSpan.FromSeconds(10));
		}
		catch (Exception ex)
		{
			_logger.LogWarning(ex, "Error waiting for geocoding processing task to complete");
		}

		_cancellationTokenSource.Dispose();
		_logger.LogInformation("NominatimGeocoder disposed");
	}
}

public record GeocodeRequest(string Location)
{
	public TaskCompletionSource<GeographicPoint> CompletionSource { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
}


public class GeographicPoint
{
	public decimal Latitude { get; set; } = -200;
	public decimal Longitude { get; set; } = -200;
	public string Name { get; set; } = string.Empty;

	public bool IsRealLocation { get; set; } = true;

	public bool IsDelayed { get; set; } = false;

	// Is this a valid location?
	public bool IsValid => IsRealLocation && !IsDelayed && Latitude != -200 && Longitude != -200;

}

public class NominatimLocation
{
	public int place_id { get; set; }
	public string? licence { get; set; }
	public string? osm_type { get; set; }
	public long osm_id { get; set; }
	public string? lat { get; set; }
	public string? lon { get; set; }
	public string? _class { get; set; }
	public string? type { get; set; }
	public int place_rank { get; set; }
	public float importance { get; set; }
	public string? addresstype { get; set; }
	public string? name { get; set; }
	public string? display_name { get; set; }
	public string[]? boundingbox { get; set; }
}


public class NullIconProvider : IMapIconProvider
{

	public string? GetIconUrl(string userType, string service)
	{
		return string.Empty;
	}
}