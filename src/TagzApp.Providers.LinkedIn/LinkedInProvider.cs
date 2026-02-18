using System.Collections.Concurrent;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Web;
using Microsoft.Extensions.Logging;
using TagzApp.Common.Telemetry;

namespace TagzApp.Providers.LinkedIn;

public class LinkedInProvider : ISocialMediaProvider, IDisposable
{
	private const string LinkedInApiVersion = "202401";
	private const string LinkedInApiBase = "https://api.linkedin.com";

	private readonly HttpClient _httpClient;
	private readonly ILogger<LinkedInProvider> _logger;
	private readonly LinkedInConfiguration _configuration;
	private readonly ProviderInstrumentation? _instrumentation;
	private readonly ConcurrentDictionary<string, LinkedInAuthor> _authorCache = new();

	private SocialMediaStatus _status = SocialMediaStatus.Unhealthy;
	private string _statusMessage = "Not started";
	private int _dailyCallCount;
	private DateTimeOffset _dailyResetTime = DateTimeOffset.UtcNow.Date.AddDays(1);

	public LinkedInProvider(
		IHttpClientFactory httpClientFactory,
		ILogger<LinkedInProvider> logger,
		LinkedInConfiguration configuration,
		ProviderInstrumentation? instrumentation = null)
	{
		_httpClient = httpClientFactory.CreateClient(nameof(LinkedInProvider));
		_logger = logger;
		_configuration = configuration;
		_instrumentation = instrumentation;
		Enabled = configuration.Enabled;
	}

	public string Id => "LINKEDIN";
	public string DisplayName => "LinkedIn";
	public string Description { get; init; } = "LinkedIn professional network hashtag search";
	public bool Enabled { get; }

	public TimeSpan NewContentRetrievalFrequency =>
		TimeSpan.FromMinutes(Math.Max(_configuration.PollingIntervalMinutes, 5));

	public void Dispose()
	{
		// HttpClient lifetime managed by IHttpClientFactory
	}

	public async Task<IProviderConfiguration> GetConfiguration(IConfigureTagzApp configure)
	{
		return await configure.GetConfigurationById<LinkedInConfiguration>(LinkedInConfiguration.AppSettingsSection);
	}

	public async Task SaveConfiguration(IConfigureTagzApp configure, IProviderConfiguration providerConfiguration)
	{
		await configure.SetConfigurationById(LinkedInConfiguration.AppSettingsSection, (LinkedInConfiguration)providerConfiguration);
	}

	public async Task<IEnumerable<Content>> GetContentForHashtag(Hashtag tag, DateTimeOffset since)
	{
		ResetDailyBudgetIfNeeded();

		if (_dailyCallCount >= _configuration.DailyCallBudget)
		{
			_status = SocialMediaStatus.Degraded;
			_statusMessage = $"Daily API call budget exhausted ({_configuration.DailyCallBudget} calls)";
			_logger.LogWarning("LinkedIn daily API call budget exhausted: {Budget}", _configuration.DailyCallBudget);
			return [];
		}

		if (string.IsNullOrWhiteSpace(_configuration.AccessToken))
		{
			_status = SocialMediaStatus.Unhealthy;
			_statusMessage = "No access token configured";
			return [];
		}

		var hashtag = Hashtag.ClearFormatting(tag.Text);
		var encodedHashtag = HttpUtility.UrlEncode(hashtag);
		var requestUri = $"{LinkedInApiBase}/rest/posts?q=hashtag&hashtag={encodedHashtag}";

		try
		{
			using var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
			request.Headers.Add("Authorization", $"Bearer {_configuration.AccessToken}");
			request.Headers.Add("LinkedIn-Version", LinkedInApiVersion);
			request.Headers.Add("X-Restli-Protocol-Version", "2.0.0");

			var response = await _httpClient.SendAsync(request);
			Interlocked.Increment(ref _dailyCallCount);

			if (!response.IsSuccessStatusCode)
			{
				_status = SocialMediaStatus.Unhealthy;
				_statusMessage = $"API returned {(int)response.StatusCode} {response.ReasonPhrase}";
				_logger.LogError("LinkedIn API error: {StatusCode} {Reason}", (int)response.StatusCode, response.ReasonPhrase);
				return [];
			}

			var responseBody = await response.Content.ReadFromJsonAsync<LinkedInPostsResponse>();
			if (responseBody?.Elements is null || responseBody.Elements.Length == 0)
			{
				_status = SocialMediaStatus.Healthy;
				_statusMessage = "OK - No new posts found";
				return [];
			}

			var results = new List<Content>();
			foreach (var post in responseBody.Elements)
			{
				if (post.CreatedAt <= 0) continue;

				var postTimestamp = DateTimeOffset.FromUnixTimeMilliseconds(post.CreatedAt);
				if (postTimestamp <= since) continue;

				var author = await ResolveAuthor(post.Author);

				var postUrn = post.Id ?? string.Empty;
				var sourceUri = new Uri($"https://www.linkedin.com/feed/update/{postUrn}");

				results.Add(new Content
				{
					Provider = "LINKEDIN",
					ProviderId = postUrn,
					HashtagSought = tag.Text,
					Type = ContentType.Message,
					SourceUri = sourceUri,
					Timestamp = postTimestamp,
					Author = new Creator
					{
						ProviderId = post.Author ?? string.Empty,
						UserName = author.VanityName,
						DisplayName = author.DisplayName,
						ProfileUri = string.IsNullOrEmpty(author.VanityName)
							? new Uri("https://www.linkedin.com")
							: new Uri($"https://www.linkedin.com/in/{author.VanityName}"),
						ProfileImageUri = string.IsNullOrEmpty(author.ProfileImageUrl)
							? new Uri("/img/user.jpg", UriKind.Relative)
							: new Uri(author.ProfileImageUrl)
					},
					Text = post.Commentary ?? string.Empty
				});
			}

			_status = SocialMediaStatus.Healthy;
			_statusMessage = $"OK - {results.Count} new post(s), {_dailyCallCount}/{_configuration.DailyCallBudget} API calls today";
			return results;
		}
		catch (TaskCanceledException ex)
		{
			_status = SocialMediaStatus.Unhealthy;
			_statusMessage = "Request timed out";
			_logger.LogError(ex, "LinkedIn API request timed out");
			return [];
		}
		catch (Exception ex)
		{
			_status = SocialMediaStatus.Unhealthy;
			_statusMessage = $"Error: {ex.Message}";
			_logger.LogError(ex, "Error fetching LinkedIn content for hashtag {Hashtag}", tag.Text);
			return [];
		}
	}

	public Task<(SocialMediaStatus Status, string Message)> GetHealth()
	{
		if (!Enabled)
		{
			return Task.FromResult((SocialMediaStatus.Disabled, "Provider is disabled"));
		}

		if (string.IsNullOrWhiteSpace(_configuration.AccessToken))
		{
			return Task.FromResult((SocialMediaStatus.Unhealthy, "No access token configured"));
		}

		// Check token expiry
		if (DateTimeOffset.TryParse(_configuration.TokenExpiresAt, out var expiresAt))
		{
			var daysUntilExpiry = (expiresAt - DateTimeOffset.UtcNow).TotalDays;
			if (daysUntilExpiry <= 0)
			{
				return Task.FromResult((SocialMediaStatus.Unhealthy, "Access token has expired"));
			}

			if (daysUntilExpiry <= 7)
			{
				return Task.FromResult((SocialMediaStatus.Degraded, $"Access token expires in {daysUntilExpiry:F0} day(s)"));
			}
		}

		ResetDailyBudgetIfNeeded();
		if (_dailyCallCount >= _configuration.DailyCallBudget)
		{
			return Task.FromResult((SocialMediaStatus.Degraded, $"Daily API call budget exhausted ({_dailyCallCount}/{_configuration.DailyCallBudget})"));
		}

		// All checks passed — provider is healthy
		return Task.FromResult((SocialMediaStatus.Healthy, _status == SocialMediaStatus.Unhealthy && _statusMessage == "Not started" ? "OK" : _statusMessage));
	}

	public Task StartAsync()
	{
		_status = SocialMediaStatus.Healthy;
		_statusMessage = "OK";
		return Task.CompletedTask;
	}

	public Task StopAsync()
	{
		return Task.CompletedTask;
	}

	private async Task<LinkedInAuthor> ResolveAuthor(string? authorUrn)
	{
		if (string.IsNullOrEmpty(authorUrn))
		{
			return new LinkedInAuthor
			{
				PersonUrn = string.Empty,
				DisplayName = "Unknown",
				VanityName = string.Empty
			};
		}

		if (_authorCache.TryGetValue(authorUrn, out var cached) &&
			(DateTimeOffset.UtcNow - cached.CachedAt).TotalHours < 24)
		{
			return cached;
		}

		// Extract person ID from URN (e.g., "urn:li:person:ABC123" -> "ABC123")
		var personId = authorUrn.Replace("urn:li:person:", "").Replace("urn:li:organization:", "");

		try
		{
			ResetDailyBudgetIfNeeded();
			if (_dailyCallCount >= _configuration.DailyCallBudget)
			{
				return CacheAuthor(authorUrn, personId, "LinkedIn User", string.Empty);
			}

			var isOrg = authorUrn.Contains("urn:li:organization:");
			var profileUri = isOrg
				? $"{LinkedInApiBase}/rest/organizations/{personId}"
				: $"{LinkedInApiBase}/rest/people/(id:{personId})";

			using var request = new HttpRequestMessage(HttpMethod.Get, profileUri);
			request.Headers.Add("Authorization", $"Bearer {_configuration.AccessToken}");
			request.Headers.Add("LinkedIn-Version", LinkedInApiVersion);
			request.Headers.Add("X-Restli-Protocol-Version", "2.0.0");

			var response = await _httpClient.SendAsync(request);
			Interlocked.Increment(ref _dailyCallCount);

			if (!response.IsSuccessStatusCode)
			{
				_logger.LogWarning("Failed to resolve LinkedIn author {Urn}: {Status}", authorUrn, response.StatusCode);
				return CacheAuthor(authorUrn, personId, "LinkedIn User", string.Empty);
			}

			var profile = await response.Content.ReadFromJsonAsync<JsonElement>();

			var firstName = profile.TryGetProperty("localizedFirstName", out var fn) ? fn.GetString() ?? "" : "";
			var lastName = profile.TryGetProperty("localizedLastName", out var ln) ? ln.GetString() ?? "" : "";
			var displayName = isOrg && profile.TryGetProperty("localizedName", out var orgName)
				? orgName.GetString() ?? personId
				: $"{firstName} {lastName}".Trim();
			var vanityName = profile.TryGetProperty("vanityName", out var vn) ? vn.GetString() ?? "" : "";
			var profileImageUrl = ExtractProfileImageUrl(profile);

			if (string.IsNullOrWhiteSpace(displayName)) displayName = "LinkedIn User";

			return CacheAuthor(authorUrn, vanityName, displayName, profileImageUrl);
		}
		catch (Exception ex)
		{
			_logger.LogWarning(ex, "Error resolving LinkedIn author {Urn}", authorUrn);
			return CacheAuthor(authorUrn, personId, "LinkedIn User", string.Empty);
		}
	}

	private LinkedInAuthor CacheAuthor(string urn, string vanityName, string displayName, string profileImageUrl)
	{
		var author = new LinkedInAuthor
		{
			PersonUrn = urn,
			DisplayName = displayName,
			VanityName = vanityName,
			ProfileImageUrl = profileImageUrl,
			CachedAt = DateTimeOffset.UtcNow
		};
		_authorCache[urn] = author;
		return author;
	}

	private static string ExtractProfileImageUrl(JsonElement profile)
	{
		try
		{
			if (profile.TryGetProperty("profilePicture", out var pic) &&
				pic.TryGetProperty("displayImage~", out var displayImage) &&
				displayImage.TryGetProperty("elements", out var elements) &&
				elements.GetArrayLength() > 0)
			{
				var lastElement = elements[elements.GetArrayLength() - 1];
				if (lastElement.TryGetProperty("identifiers", out var identifiers) &&
					identifiers.GetArrayLength() > 0 &&
					identifiers[0].TryGetProperty("identifier", out var identifier))
				{
					return identifier.GetString() ?? string.Empty;
				}
			}
		}
		catch
		{
			// Gracefully return empty on parse failure
		}

		return string.Empty;
	}

	private void ResetDailyBudgetIfNeeded()
	{
		if (DateTimeOffset.UtcNow >= _dailyResetTime)
		{
			Interlocked.Exchange(ref _dailyCallCount, 0);
			_dailyResetTime = DateTimeOffset.UtcNow.Date.AddDays(1);
		}
	}

	// JSON models for LinkedIn API responses
	internal class LinkedInPostsResponse
	{
		[JsonPropertyName("elements")]
		public LinkedInPost[]? Elements { get; set; }
	}

	internal class LinkedInPost
	{
		[JsonPropertyName("id")]
		public string? Id { get; set; }

		[JsonPropertyName("author")]
		public string? Author { get; set; }

		[JsonPropertyName("commentary")]
		public string? Commentary { get; set; }

		[JsonPropertyName("createdAt")]
		public long CreatedAt { get; set; }

		[JsonPropertyName("lifecycleState")]
		public string? LifecycleState { get; set; }

		[JsonPropertyName("visibility")]
		public string? Visibility { get; set; }
	}
}
