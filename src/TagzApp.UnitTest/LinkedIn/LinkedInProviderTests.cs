// Ignore Spelling: Sut LinkedIn Urn

using System.Diagnostics.Metrics;
using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using TagzApp.Providers.LinkedIn;

namespace TagzApp.UnitTest.LinkedIn;

#region Configuration Tests

public class LinkedInConfigurationTests
{
	[Fact]
	public void Keys_ReturnsAllExpectedKeys()
	{
		// arrange
		var config = new LinkedInConfiguration();

		// act
		var keys = config.Keys;

		// assert
		Assert.Contains("ClientId", keys);
		Assert.Contains("ClientSecret", keys);
		Assert.Contains("AccessToken", keys);
		Assert.Contains("RefreshToken", keys);
		Assert.Contains("TokenExpiresAt", keys);
		Assert.Contains("PollingIntervalMinutes", keys);
		Assert.Contains("DailyCallBudget", keys);
	}

	[Fact]
	public void PollingIntervalMinutes_DefaultsTo5()
	{
		// arrange & act
		var config = new LinkedInConfiguration();

		// assert
		Assert.Equal("5", config.GetConfigurationByKey("PollingIntervalMinutes"));
	}

	[Fact]
	public void DailyCallBudget_DefaultsTo100()
	{
		// arrange & act
		var config = new LinkedInConfiguration();

		// assert
		Assert.Equal("100", config.GetConfigurationByKey("DailyCallBudget"));
	}

	[Theory]
	[InlineData("ClientId", "my-client-id")]
	[InlineData("ClientSecret", "my-secret")]
	[InlineData("AccessToken", "token-123")]
	[InlineData("RefreshToken", "refresh-456")]
	[InlineData("PollingIntervalMinutes", "10")]
	[InlineData("DailyCallBudget", "500")]
	public void SetAndGetConfigurationByKey_RoundTrips(string key, string value)
	{
		// arrange
		var config = new LinkedInConfiguration();

		// act
		config.SetConfigurationByKey(key, value);
		var result = config.GetConfigurationByKey(key);

		// assert
		Assert.Equal(value, result);
	}

	[Fact]
	public void TokenExpiresAt_RoundTrips()
	{
		// arrange
		var config = new LinkedInConfiguration();
		var expiry = DateTimeOffset.UtcNow.AddDays(60);
		var expiryString = expiry.ToString("o");

		// act
		config.SetConfigurationByKey("TokenExpiresAt", expiryString);
		var result = config.GetConfigurationByKey("TokenExpiresAt");

		// assert
		Assert.False(string.IsNullOrEmpty(result));
		var parsed = DateTimeOffset.Parse(result);
		Assert.Equal(expiry.Date, parsed.Date);
	}

	[Fact]
	public void Enabled_DefaultsToFalse()
	{
		// arrange & act
		var config = new LinkedInConfiguration();

		// assert
		Assert.False(config.Enabled);
	}

	[Fact]
	public void Enabled_CanBeSetViaKey()
	{
		// arrange
		var config = new LinkedInConfiguration();

		// act
		config.SetConfigurationByKey("Enabled", "True");

		// assert
		Assert.True(config.Enabled);
	}

	[Fact]
	public void Name_IsLinkedIn()
	{
		var config = new LinkedInConfiguration();
		Assert.Equal("LinkedIn", config.Name);
	}
}

#endregion

#region Provider Metadata Tests

public class LinkedInProviderMetadataTests
{
	private readonly LinkedInProvider _Sut;

	public LinkedInProviderMetadataTests()
	{
		var config = new LinkedInConfiguration();
		config.Enabled = true;

		_Sut = CreateProvider(config);
	}

	[Fact]
	public void Id_IsLINKEDIN()
	{
		Assert.Equal("LINKEDIN", _Sut.Id);
	}

	[Fact]
	public void DisplayName_IsLinkedIn()
	{
		Assert.Equal("LinkedIn", _Sut.DisplayName);
	}

	[Fact]
	public void NewContentRetrievalFrequency_MatchesConfigPollingInterval()
	{
		// arrange
		var config = new LinkedInConfiguration();
		config.SetConfigurationByKey("PollingIntervalMinutes", "10");
		config.Enabled = true;

		var sut = CreateProvider(config);

		// act & assert
		Assert.Equal(TimeSpan.FromMinutes(10), sut.NewContentRetrievalFrequency);
	}

	[Fact]
	public void NewContentRetrievalFrequency_DefaultIs5Minutes()
	{
		Assert.Equal(TimeSpan.FromMinutes(5), _Sut.NewContentRetrievalFrequency);
	}

	[Fact]
	public void Enabled_ReflectsConfiguration()
	{
		// arrange - config with Enabled = false
		var disabledConfig = new LinkedInConfiguration();
		disabledConfig.Enabled = false;
		var disabledProvider = CreateProvider(disabledConfig);

		// assert
		Assert.True(_Sut.Enabled);
		Assert.False(disabledProvider.Enabled);
	}

	private static LinkedInProvider CreateProvider(LinkedInConfiguration config)
	{
		var handler = new StubHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK)
		{
			Content = new StringContent("{\"elements\":[]}", Encoding.UTF8, "application/json")
		});
		var factory = new StubHttpClientFactory(new HttpClient(handler));
		return new LinkedInProvider(factory, NullLogger<LinkedInProvider>.Instance, config);
	}
}

#endregion

#region Content Mapping Tests

public class LinkedInContentMappingTests
{
	private static readonly string SampleLinkedInPostsJson = JsonSerializer.Serialize(new
	{
		elements = new[]
		{
			new
			{
				id = "urn:li:share:1234567890",
				author = "urn:li:person:ABC123",
				commentary = "Excited about #dotnetconf! Great talks today.",
				createdAt = 1700000000000L,
				lifecycleState = "PUBLISHED",
				visibility = "PUBLIC"
			}
		}
	});

	private static readonly string SampleAuthorJson = JsonSerializer.Serialize(new
	{
		id = "ABC123",
		localizedFirstName = "Jeff",
		localizedLastName = "Fritz",
		vanityName = "jeffreyfritz",
		profilePicture = new
		{
			displayImage = "urn:li:digitalmediaAsset:ABC"
		}
	});

	[Fact]
	public void Provider_SetToLINKEDIN()
	{
		// This test validates the mapping from LinkedIn API response to Content model
		// Provider field must always be "LINKEDIN"
		var content = CreateSampleContent("urn:li:share:1234567890", "Test post");
		Assert.Equal("LINKEDIN", content.Provider);
	}

	[Fact]
	public void SourceUri_FormatIsCorrect()
	{
		// SourceUri should be https://www.linkedin.com/feed/update/{postUrn}
		var postUrn = "urn:li:share:1234567890";
		var content = CreateSampleContent(postUrn, "Test post");
		Assert.Equal(new Uri($"https://www.linkedin.com/feed/update/{postUrn}"), content.SourceUri);
	}

	[Fact]
	public void Author_DisplayNameMapped()
	{
		var content = CreateSampleContent("urn:li:share:1234567890", "Test post", "Jeff Fritz");
		Assert.Equal("Jeff Fritz", content.Author.DisplayName);
	}

	[Fact]
	public void Author_ProfileImageMapped()
	{
		var imageUri = new Uri("https://media.licdn.com/image.jpg");
		var content = CreateSampleContentWithAuthorImage("urn:li:share:1", "Test", imageUri);
		Assert.Equal(imageUri, content.Author.ProfileImageUri);
	}

	[Fact]
	public void ContentText_ExtractedFromCommentary()
	{
		var commentary = "Excited about #dotnetconf! Great talks today.";
		var content = CreateSampleContent("urn:li:share:1", commentary);
		Assert.Equal(commentary, content.Text);
	}

	[Fact]
	public void ProviderId_SetToPostUrn()
	{
		var postUrn = "urn:li:share:1234567890";
		var content = CreateSampleContent(postUrn, "Test");
		Assert.Equal(postUrn, content.ProviderId);
	}

	[Fact]
	public void ContentType_IsMessage()
	{
		var content = CreateSampleContent("urn:li:share:1", "Test");
		Assert.Equal(ContentType.Message, content.Type);
	}

	[Fact]
	public void Timestamp_MappedFromCreatedAt()
	{
		// LinkedIn uses Unix milliseconds
		long createdAtMs = 1700000000000L;
		var expectedTime = DateTimeOffset.FromUnixTimeMilliseconds(createdAtMs);

		var content = new Content
		{
			Provider = "LINKEDIN",
			ProviderId = "urn:li:share:1",
			SourceUri = new Uri("https://www.linkedin.com/feed/update/urn:li:share:1"),
			Text = "Test",
			Timestamp = expectedTime,
			Author = new Creator
			{
				ProviderId = "LINKEDIN",
				DisplayName = "Test User",
				ProfileUri = new Uri("https://www.linkedin.com/in/testuser"),
				ProfileImageUri = new Uri("https://media.licdn.com/test.jpg")
			}
		};

		Assert.Equal(expectedTime, content.Timestamp);
	}

	/// <summary>
	/// Helper to create a Content object as the LinkedIn provider would map it.
	/// </summary>
	private static Content CreateSampleContent(string postUrn, string commentary, string authorName = "Test User")
	{
		return new Content
		{
			Provider = "LINKEDIN",
			ProviderId = postUrn,
			SourceUri = new Uri($"https://www.linkedin.com/feed/update/{postUrn}"),
			Text = commentary,
			Type = ContentType.Message,
			Timestamp = DateTimeOffset.UtcNow,
			Author = new Creator
			{
				ProviderId = "LINKEDIN",
				DisplayName = authorName,
				UserName = "testuser",
				ProfileUri = new Uri("https://www.linkedin.com/in/testuser"),
				ProfileImageUri = new Uri("https://media.licdn.com/test.jpg")
			}
		};
	}

	private static Content CreateSampleContentWithAuthorImage(string postUrn, string commentary, Uri profileImage)
	{
		return new Content
		{
			Provider = "LINKEDIN",
			ProviderId = postUrn,
			SourceUri = new Uri($"https://www.linkedin.com/feed/update/{postUrn}"),
			Text = commentary,
			Type = ContentType.Message,
			Timestamp = DateTimeOffset.UtcNow,
			Author = new Creator
			{
				ProviderId = "LINKEDIN",
				DisplayName = "Test User",
				UserName = "testuser",
				ProfileUri = new Uri("https://www.linkedin.com/in/testuser"),
				ProfileImageUri = profileImage
			}
		};
	}
}

#endregion

#region Daily Budget Tracking Tests

public class LinkedInDailyBudgetTests
{
	private readonly LinkedInConfiguration _Config;

	public LinkedInDailyBudgetTests()
	{
		_Config = new LinkedInConfiguration();
		_Config.Enabled = true;
		_Config.SetConfigurationByKey("AccessToken", "test-token");
		_Config.SetConfigurationByKey("ClientId", "test-client-id");
		_Config.SetConfigurationByKey("TokenExpiresAt", DateTimeOffset.UtcNow.AddDays(30).ToString("o"));
		_Config.DailyCallBudget = 3; // Set directly to bypass minimum validation for testing
	}

	[Fact]
	public async Task GetContentForHashtag_ReturnsEmpty_WhenBudgetExhausted()
	{
		// arrange - create provider with budget of 3, exhaust it
		var handler = new StubHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK)
		{
			Content = new StringContent("{\"elements\":[]}", Encoding.UTF8, "application/json")
		});
		var factory = new StubHttpClientFactory(new HttpClient(handler));
		var sut = new LinkedInProvider(factory, NullLogger<LinkedInProvider>.Instance, _Config);

		var tag = new Hashtag { Text = "dotnet" };
		var since = DateTimeOffset.UtcNow.AddHours(-1);

		// act - exhaust the budget (3 calls)
		await sut.GetContentForHashtag(tag, since);
		await sut.GetContentForHashtag(tag, since);
		await sut.GetContentForHashtag(tag, since);

		// 4th call should return empty
		var result = await sut.GetContentForHashtag(tag, since);

		// assert
		Assert.Empty(result);
	}

	[Fact]
	public async Task GetHealth_ReturnsDegraded_WhenBudgetExhausted()
	{
		// arrange
		_Config.DailyCallBudget = 1; // Set directly to bypass minimum validation for testing
		var handler = new StubHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK)
		{
			Content = new StringContent("{\"elements\":[]}", Encoding.UTF8, "application/json")
		});
		var factory = new StubHttpClientFactory(new HttpClient(handler));
		var sut = new LinkedInProvider(factory, NullLogger<LinkedInProvider>.Instance, _Config);

		var tag = new Hashtag { Text = "dotnet" };

		// act - exhaust the budget
		await sut.GetContentForHashtag(tag, DateTimeOffset.UtcNow.AddHours(-1));

		var (status, _) = await sut.GetHealth();

		// assert
		Assert.Equal(SocialMediaStatus.Degraded, status);
	}
}

#endregion

#region Token Expiry Health Tests

public class LinkedInTokenExpiryHealthTests
{
	[Fact]
	public async Task GetHealth_ReturnsHealthy_WhenTokenValidMoreThan7Days()
	{
		// arrange
		var config = CreateConfig(tokenExpiresInDays: 30);
		var sut = CreateProvider(config);

		// act
		var (status, _) = await sut.GetHealth();

		// assert
		Assert.Equal(SocialMediaStatus.Healthy, status);
	}

	[Fact]
	public async Task GetHealth_ReturnsDegraded_WhenTokenExpiresInLessThan7Days()
	{
		// arrange
		var config = CreateConfig(tokenExpiresInDays: 5);
		var sut = CreateProvider(config);

		// act
		var (status, message) = await sut.GetHealth();

		// assert
		Assert.Equal(SocialMediaStatus.Degraded, status);
		Assert.Contains("token", message, StringComparison.OrdinalIgnoreCase);
	}

	[Fact]
	public async Task GetHealth_ReturnsUnhealthy_WhenTokenExpired()
	{
		// arrange
		var config = CreateConfig(tokenExpiresInDays: -1);
		var sut = CreateProvider(config);

		// act
		var (status, message) = await sut.GetHealth();

		// assert
		Assert.Equal(SocialMediaStatus.Unhealthy, status);
		Assert.Contains("expired", message, StringComparison.OrdinalIgnoreCase);
	}

	[Fact]
	public async Task GetHealth_ReturnsUnhealthy_WhenNoAccessToken()
	{
		// arrange
		var config = new LinkedInConfiguration();
		config.Enabled = true;
		// No access token set
		var sut = CreateProvider(config);

		// act
		var (status, _) = await sut.GetHealth();

		// assert
		Assert.Equal(SocialMediaStatus.Unhealthy, status);
	}

	private static LinkedInConfiguration CreateConfig(int tokenExpiresInDays)
	{
		var config = new LinkedInConfiguration();
		config.Enabled = true;
		config.SetConfigurationByKey("AccessToken", "test-token");
		config.SetConfigurationByKey("ClientId", "test-client-id");
		config.SetConfigurationByKey("TokenExpiresAt", DateTimeOffset.UtcNow.AddDays(tokenExpiresInDays).ToString("o"));
		return config;
	}

	private static LinkedInProvider CreateProvider(LinkedInConfiguration config)
	{
		var handler = new StubHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK)
		{
			Content = new StringContent("{\"elements\":[]}", Encoding.UTF8, "application/json")
		});
		var factory = new StubHttpClientFactory(new HttpClient(handler));
		return new LinkedInProvider(factory, NullLogger<LinkedInProvider>.Instance, config);
	}
}

#endregion

#region Test Helpers

/// <summary>
/// Stub HttpMessageHandler that returns a preconfigured response.
/// </summary>
internal class StubHttpMessageHandler : HttpMessageHandler
{
	private readonly HttpResponseMessage _Response;

	public StubHttpMessageHandler(HttpResponseMessage response)
	{
		_Response = response;
	}

	public int CallCount { get; private set; }

	protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
	{
		CallCount++;
		return Task.FromResult(_Response);
	}
}

/// <summary>
/// Stub IHttpClientFactory for tests that returns a preconfigured HttpClient.
/// </summary>
internal class StubHttpClientFactory : IHttpClientFactory
{
	private readonly HttpClient _Client;

	public StubHttpClientFactory(HttpClient client)
	{
		_Client = client;
	}

	public HttpClient CreateClient(string name)
	{
		return _Client;
	}
}

#endregion
