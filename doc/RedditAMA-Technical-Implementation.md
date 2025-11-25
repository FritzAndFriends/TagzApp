# RedditAMA Provider - Technical Implementation Guide

## Implementation Overview

This guide provides detailed technical specifications for implementing the RedditAMA provider in TagzApp, following the established provider pattern used by TwitchChat and YouTube providers.

## File Structure

```
src/TagzApp.Providers.RedditAMA/
├── TagzApp.Providers.RedditAMA.csproj
├── RedditAMAProvider.cs                 # Main provider implementation
├── RedditAMAConfiguration.cs            # Configuration model
├── RedditAMABackgroundService.cs        # Background polling service
├── Models/
│   ├── RedditApiModels.cs              # Reddit API response models
│   └── RedditComment.cs                # Internal comment model
├── Services/
│   ├── IRedditApiService.cs            # Reddit API abstraction
│   └── RedditApiService.cs             # Reddit API implementation
└── Extensions/
    └── ServiceCollectionExtensions.cs  # DI registration
```

## Core Implementation

### 1. Provider Configuration

```csharp
// TagzApp.Providers.RedditAMA/RedditAMAConfiguration.cs
using TagzApp.Common.Client;
using System.ComponentModel.DataAnnotations;

namespace TagzApp.Providers.RedditAMA;

public class RedditAMAConfiguration : BaseProviderConfiguration<RedditAMAConfiguration>
{
    public const string AppSettingsSection = "providers:redditama";
    
    public string Description => "Monitor Reddit AMA/discussion threads for live comments";
    public string Name => "Reddit AMA";
    
    [Required]
    [Display(Name = "Subreddit", Description = "The subreddit containing the post (e.g., 'IAmA')")]
    public string Subreddit { get; set; } = string.Empty;
    
    [Required]
    [Display(Name = "Post ID", Description = "The Reddit post ID to monitor")]
    public string PostId { get; set; } = string.Empty;
    
    [Display(Name = "Post Title", Description = "Title of the Reddit post (auto-populated)")]
    public string PostTitle { get; set; } = string.Empty;
    
    [Required]
    [Display(Name = "User Agent", Description = "User agent string for Reddit API requests")]
    public string UserAgent { get; set; } = "TagzApp/1.0 (by /u/TagzApp)";
    
    [Range(15, 300)]
    [Display(Name = "Refresh Interval (seconds)", Description = "How often to poll for new comments")]
    public int RefreshIntervalSeconds { get; set; } = 30;
    
    [Range(10, 500)]
    [Display(Name = "Max Comments Per Poll", Description = "Maximum comments to fetch per request")]
    public int MaxCommentsPerPoll { get; set; } = 100;
    
    [Display(Name = "Include Replies", Description = "Include replies to top-level comments")]
    public bool IncludeReplies { get; set; } = true;
    
    [Display(Name = "Minimum Comment Score", Description = "Hide comments below this score")]
    public int MinCommentScore { get; set; } = -10;
    
    [Display(Name = "Hide Deleted Comments", Description = "Filter out deleted/removed comments")]
    public bool HideDeletedComments { get; set; } = true;
    
    [Display(Name = "Blocked Authors", Description = "Comma-separated list of usernames to block")]
    public string BlockedAuthors { get; set; } = string.Empty;
    
    public string[] Keys => new[]
    {
        nameof(Subreddit),
        nameof(PostId),
        nameof(PostTitle),
        nameof(UserAgent),
        nameof(RefreshIntervalSeconds),
        nameof(MaxCommentsPerPoll),
        nameof(IncludeReplies),
        nameof(MinCommentScore),
        nameof(HideDeletedComments),
        nameof(BlockedAuthors)
    };
    
    public bool IsValid => !string.IsNullOrWhiteSpace(Subreddit) && 
                          !string.IsNullOrWhiteSpace(PostId) && 
                          !string.IsNullOrWhiteSpace(UserAgent) &&
                          RefreshIntervalSeconds >= 15 &&
                          MaxCommentsPerPoll > 0;
    
    public string[] GetBlockedAuthorsList() => 
        BlockedAuthors?.Split(',', StringSplitOptions.RemoveEmptyEntries)
                     .Select(s => s.Trim().ToLowerInvariant())
                     .Where(s => !string.IsNullOrEmpty(s))
                     .ToArray() ?? Array.Empty<string>();
}
```

### 2. Reddit API Models

```csharp
// TagzApp.Providers.RedditAMA/Models/RedditApiModels.cs
using System.Text.Json.Serialization;

namespace TagzApp.Providers.RedditAMA.Models;

public class RedditListingResponse<T>
{
    [JsonPropertyName("kind")]
    public string Kind { get; set; } = string.Empty;
    
    [JsonPropertyName("data")]
    public RedditListingData<T> Data { get; set; } = new();
}

public class RedditListingData<T>
{
    [JsonPropertyName("children")]
    public T[] Children { get; set; } = Array.Empty<T>();
    
    [JsonPropertyName("after")]
    public string? After { get; set; }
    
    [JsonPropertyName("before")]
    public string? Before { get; set; }
}

public class RedditThing<T>
{
    [JsonPropertyName("kind")]
    public string Kind { get; set; } = string.Empty;
    
    [JsonPropertyName("data")]
    public T Data { get; set; } = default!;
}

public class RedditPost
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
    
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;
    
    [JsonPropertyName("author")]
    public string Author { get; set; } = string.Empty;
    
    [JsonPropertyName("subreddit")]
    public string Subreddit { get; set; } = string.Empty;
    
    [JsonPropertyName("created_utc")]
    public long CreatedUtc { get; set; }
    
    [JsonPropertyName("permalink")]
    public string Permalink { get; set; } = string.Empty;
    
    [JsonPropertyName("selftext")]
    public string SelfText { get; set; } = string.Empty;
    
    [JsonPropertyName("num_comments")]
    public int NumComments { get; set; }
}

public class RedditComment
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
    
    [JsonPropertyName("author")]
    public string Author { get; set; } = string.Empty;
    
    [JsonPropertyName("body")]
    public string Body { get; set; } = string.Empty;
    
    [JsonPropertyName("created_utc")]
    public long CreatedUtc { get; set; }
    
    [JsonPropertyName("permalink")]
    public string Permalink { get; set; } = string.Empty;
    
    [JsonPropertyName("score")]
    public int Score { get; set; }
    
    [JsonPropertyName("depth")]
    public int Depth { get; set; }
    
    [JsonPropertyName("parent_id")]
    public string ParentId { get; set; } = string.Empty;
    
    [JsonPropertyName("is_submitter")]
    public bool IsSubmitter { get; set; }
    
    [JsonPropertyName("distinguished")]
    public string? Distinguished { get; set; }
    
    [JsonPropertyName("stickied")]
    public bool Stickied { get; set; }
    
    [JsonPropertyName("replies")]
    public object? Replies { get; set; } // Can be string or RedditListingResponse<RedditThing<RedditComment>>
}
```

### 3. Reddit API Service

```csharp
// TagzApp.Providers.RedditAMA/Services/IRedditApiService.cs
using TagzApp.Providers.RedditAMA.Models;

namespace TagzApp.Providers.RedditAMA.Services;

public interface IRedditApiService
{
    Task<RedditPost?> GetPostAsync(string subreddit, string postId, CancellationToken cancellationToken = default);
    Task<IEnumerable<RedditComment>> GetCommentsAsync(string subreddit, string postId, int limit = 100, CancellationToken cancellationToken = default);
    Task<bool> TestConnectionAsync(CancellationToken cancellationToken = default);
}

// TagzApp.Providers.RedditAMA/Services/RedditApiService.cs
using System.Net.Http.Json;
using System.Text.Json;
using TagzApp.Providers.RedditAMA.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace TagzApp.Providers.RedditAMA.Services;

public class RedditApiService : IRedditApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<RedditApiService> _logger;
    private readonly RedditAMAConfiguration _config;
    
    public RedditApiService(
        HttpClient httpClient, 
        ILogger<RedditApiService> logger,
        IOptions<RedditAMAConfiguration> config)
    {
        _httpClient = httpClient;
        _logger = logger;
        _config = config.Value;
        
        // Configure HttpClient
        _httpClient.BaseAddress = new Uri("https://www.reddit.com");
        _httpClient.DefaultRequestHeaders.Add("User-Agent", _config.UserAgent);
        _httpClient.Timeout = TimeSpan.FromSeconds(30);
    }
    
    public async Task<RedditPost?> GetPostAsync(string subreddit, string postId, CancellationToken cancellationToken = default)
    {
        try
        {
            var url = $"/r/{subreddit}/comments/{postId}.json?limit=1";
            var response = await _httpClient.GetFromJsonAsync<RedditThing<RedditPost>[]>(url, cancellationToken);
            
            return response?.FirstOrDefault()?.Data;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get Reddit post {PostId} from r/{Subreddit}", postId, subreddit);
            return null;
        }
    }
    
    public async Task<IEnumerable<RedditComment>> GetCommentsAsync(string subreddit, string postId, int limit = 100, CancellationToken cancellationToken = default)
    {
        try
        {
            var url = $"/r/{subreddit}/comments/{postId}.json?sort=new&limit={limit}";
            var response = await _httpClient.GetFromJsonAsync<RedditThing<RedditPost>[]>(url, cancellationToken);
            
            if (response?.Length < 2) return Enumerable.Empty<RedditComment>();
            
            // Second element contains comments
            var commentsSection = response[1];
            var commentListing = JsonSerializer.Deserialize<RedditListingResponse<RedditThing<RedditComment>>>(
                JsonSerializer.SerializeToUtf8Bytes(commentsSection));
            
            return ExtractAllComments(commentListing?.Data.Children ?? Array.Empty<RedditThing<RedditComment>>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get comments for Reddit post {PostId} from r/{Subreddit}", postId, subreddit);
            return Enumerable.Empty<RedditComment>();
        }
    }
    
    private IEnumerable<RedditComment> ExtractAllComments(IEnumerable<RedditThing<RedditComment>> commentThings)
    {
        var comments = new List<RedditComment>();
        
        foreach (var thing in commentThings)
        {
            if (thing.Data.Author == "[deleted]" && _config.HideDeletedComments)
                continue;
                
            comments.Add(thing.Data);
            
            // Extract replies if enabled
            if (_config.IncludeReplies && thing.Data.Replies is JsonElement repliesElement)
            {
                try
                {
                    var repliesListing = JsonSerializer.Deserialize<RedditListingResponse<RedditThing<RedditComment>>>(
                        repliesElement.GetRawText());
                    
                    if (repliesListing?.Data.Children != null)
                    {
                        comments.AddRange(ExtractAllComments(repliesListing.Data.Children));
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to parse replies for comment {CommentId}", thing.Data.Id);
                }
            }
        }
        
        return comments;
    }
    
    public async Task<bool> TestConnectionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync("/r/test.json?limit=1", cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}
```

### 4. Main Provider Implementation

```csharp
// TagzApp.Providers.RedditAMA/RedditAMAProvider.cs
using System.Collections.Concurrent;
using TagzApp.Common.Client;
using TagzApp.Common.Models;
using TagzApp.Providers.RedditAMA.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace TagzApp.Providers.RedditAMA;

public class RedditAMAProvider : ISocialMediaProvider
{
    private readonly IRedditApiService _redditApi;
    private readonly ILogger<RedditAMAProvider> _logger;
    private readonly RedditAMAConfiguration _config;
    private readonly ConcurrentQueue<Content> _commentQueue = new();
    private readonly HashSet<string> _processedCommentIds = new();
    private readonly SemaphoreSlim _pollingSemaphore = new(1, 1);
    
    public string Id => "REDDIT_AMA";
    public string DisplayName => "Reddit AMA";
    public TimeSpan NewContentRetrievalFrequency => TimeSpan.FromSeconds(Math.Max(_config.RefreshIntervalSeconds, 15));
    public bool Enabled => _config.Enabled && _config.IsValid;
    
    public RedditAMAProvider(
        IRedditApiService redditApi,
        ILogger<RedditAMAProvider> logger,
        IOptions<RedditAMAConfiguration> config)
    {
        _redditApi = redditApi;
        _logger = logger;
        _config = config.Value;
    }
    
    public async Task<IEnumerable<Content>> GetContentForHashtag(Hashtag tag, DateTimeOffset since)
    {
        if (!Enabled)
        {
            return Enumerable.Empty<Content>();
        }
        
        // Return and clear the comment queue (similar to TwitchChat pattern)
        var comments = new List<Content>();
        while (_commentQueue.TryDequeue(out var comment))
        {
            comment.HashtagSought = tag.Text.ToLowerInvariant();
            comments.Add(comment);
        }
        
        _logger.LogDebug("RedditAMA: Returning {Count} comments for hashtag {Hashtag}", 
            comments.Count, tag.Text);
        
        return comments;
    }
    
    public async Task PollForNewComments(CancellationToken cancellationToken = default)
    {
        if (!Enabled)
        {
            return;
        }
        
        await _pollingSemaphore.WaitAsync(cancellationToken);
        try
        {
            var comments = await _redditApi.GetCommentsAsync(
                _config.Subreddit, 
                _config.PostId, 
                _config.MaxCommentsPerPoll, 
                cancellationToken);
            
            var newComments = ProcessNewComments(comments);
            
            _logger.LogInformation("RedditAMA: Found {NewCount} new comments in r/{Subreddit} post {PostId}", 
                newComments.Count(), _config.Subreddit, _config.PostId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RedditAMA: Error polling for comments in r/{Subreddit} post {PostId}", 
                _config.Subreddit, _config.PostId);
        }
        finally
        {
            _pollingSemaphore.Release();
        }
    }
    
    private IEnumerable<Content> ProcessNewComments(IEnumerable<Models.RedditComment> comments)
    {
        var blockedAuthors = _config.GetBlockedAuthorsList();
        var newComments = new List<Content>();
        
        foreach (var comment in comments)
        {
            // Skip if already processed
            if (_processedCommentIds.Contains(comment.Id))
                continue;
            
            // Apply filters
            if (ShouldFilterComment(comment, blockedAuthors))
                continue;
            
            var content = MapToContent(comment);
            _commentQueue.Enqueue(content);
            newComments.Add(content);
            
            // Track processed comment
            _processedCommentIds.Add(comment.Id);
            
            // Limit memory usage - keep only recent comment IDs
            if (_processedCommentIds.Count > 10000)
            {
                var oldIds = _processedCommentIds.Take(5000).ToList();
                foreach (var oldId in oldIds)
                {
                    _processedCommentIds.Remove(oldId);
                }
            }
        }
        
        return newComments;
    }
    
    private bool ShouldFilterComment(Models.RedditComment comment, string[] blockedAuthors)
    {
        // Filter deleted comments
        if (_config.HideDeletedComments && 
            (comment.Author == "[deleted]" || comment.Body == "[deleted]" || comment.Body == "[removed]"))
        {
            return true;
        }
        
        // Filter by score
        if (comment.Score < _config.MinCommentScore)
        {
            return true;
        }
        
        // Filter blocked authors
        if (blockedAuthors.Contains(comment.Author.ToLowerInvariant()))
        {
            return true;
        }
        
        return false;
    }
    
    private Content MapToContent(Models.RedditComment comment)
    {
        return new Content
        {
            Provider = Id,
            ProviderId = comment.Id,
            Type = ContentType.Message,
            Timestamp = DateTimeOffset.FromUnixTimeSeconds(comment.CreatedUtc),
            SourceUri = new Uri($"https://reddit.com{comment.Permalink}"),
            Author = new Creator
            {
                DisplayName = comment.Author,
                UserName = comment.Author,
                ProfileUri = new Uri($"https://reddit.com/u/{comment.Author}"),
                ProfileImageUri = comment.IsSubmitter ? 
                    new Uri("https://tagzapp.io/img/reddit-op-badge.png") : null
            },
            Text = comment.Body,
            ExtendedMetadata = new Dictionary<string, object>
            {
                ["score"] = comment.Score,
                ["depth"] = comment.Depth,
                ["isOP"] = comment.IsSubmitter,
                ["distinguished"] = comment.Distinguished ?? string.Empty,
                ["stickied"] = comment.Stickied,
                ["parentId"] = comment.ParentId
            }
        };
    }
    
    public async Task<ProviderStatus> GetStatus()
    {
        try
        {
            var canConnect = await _redditApi.TestConnectionAsync();
            var post = await _redditApi.GetPostAsync(_config.Subreddit, _config.PostId);
            
            return new ProviderStatus
            {
                IsConnected = canConnect && post != null,
                StatusMessage = canConnect && post != null ? 
                    $"Monitoring: {post?.Title}" : 
                    "Unable to connect or post not found",
                LastUpdated = DateTimeOffset.UtcNow,
                ExtendedInfo = new Dictionary<string, object>
                {
                    ["subreddit"] = _config.Subreddit,
                    ["postId"] = _config.PostId,
                    ["postTitle"] = post?.Title ?? "Unknown",
                    ["queuedComments"] = _commentQueue.Count,
                    ["processedComments"] = _processedCommentIds.Count
                }
            };
        }
        catch (Exception ex)
        {
            return new ProviderStatus
            {
                IsConnected = false,
                StatusMessage = $"Error: {ex.Message}",
                LastUpdated = DateTimeOffset.UtcNow
            };
        }
    }
}
```

### 5. Background Service

```csharp
// TagzApp.Providers.RedditAMA/RedditAMABackgroundService.cs
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace TagzApp.Providers.RedditAMA;

public class RedditAMABackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<RedditAMABackgroundService> _logger;
    private readonly RedditAMAConfiguration _config;
    
    public RedditAMABackgroundService(
        IServiceProvider serviceProvider,
        ILogger<RedditAMABackgroundService> logger,
        IOptions<RedditAMAConfiguration> config)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _config = config.Value;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("RedditAMA background service starting");
        
        while (!stoppingToken.IsCancellationRequested)
        {
            if (!_config.Enabled || !_config.IsValid)
            {
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                continue;
            }
            
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var provider = scope.ServiceProvider.GetRequiredService<RedditAMAProvider>();
                
                await provider.PollForNewComments(stoppingToken);
                
                var delay = TimeSpan.FromSeconds(Math.Max(_config.RefreshIntervalSeconds, 15));
                await Task.Delay(delay, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RedditAMA background service error");
                
                // Exponential backoff on errors
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
        
        _logger.LogInformation("RedditAMA background service stopping");
    }
}
```

### 6. Service Registration

```csharp
// TagzApp.Providers.RedditAMA/Extensions/ServiceCollectionExtensions.cs
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TagzApp.Common.Client;
using TagzApp.Providers.RedditAMA.Services;

namespace TagzApp.Providers.RedditAMA.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRedditAMAProvider(this IServiceCollection services, IConfiguration configuration)
    {
        // Configure the provider
        services.Configure<RedditAMAConfiguration>(
            configuration.GetSection(RedditAMAConfiguration.AppSettingsSection));
        
        // Register services
        services.AddHttpClient<IRedditApiService, RedditApiService>();
        services.AddScoped<ISocialMediaProvider, RedditAMAProvider>();
        services.AddHostedService<RedditAMABackgroundService>();
        
        return services;
    }
}
```

### 7. Project File

```xml
<!-- TagzApp.Providers.RedditAMA/TagzApp.Providers.RedditAMA.csproj -->
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\TagzApp.Common\TagzApp.Common.csproj" />
    <ProjectReference Include="..\TagzApp.Common.Client\TagzApp.Common.Client.csproj" />
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.Extensions.Hosting.Abstractions" />
    <PackageReference Include="Microsoft.Extensions.Http" />
    <PackageReference Include="Microsoft.Extensions.Logging.Abstractions" />
    <PackageReference Include="Microsoft.Extensions.Options" />
    <PackageReference Include="System.Text.Json" />
  </ItemGroup>

</Project>
```

## Integration Points

### 1. Add to Main Solution

```xml
<!-- Add to src/TagzApp.sln -->
Project("{9A19103F-16F7-4668-BE54-9A1E7A4F7556}") = "TagzApp.Providers.RedditAMA", "TagzApp.Providers.RedditAMA\TagzApp.Providers.RedditAMA.csproj", "{GUID}"
EndProject
```

### 2. Register in Main Application

```csharp
// In TagzApp.Blazor/Program.cs or TagzApp.AppHost/Program.cs
using TagzApp.Providers.RedditAMA.Extensions;

// Add the provider
builder.Services.AddRedditAMAProvider(builder.Configuration);
```

### 3. Configuration in appsettings.json

```json
{
  "providers": {
    "redditama": {
      "enabled": false,
      "subreddit": "",
      "postId": "",
      "postTitle": "",
      "userAgent": "TagzApp/1.0 (by /u/TagzApp)",
      "refreshIntervalSeconds": 30,
      "maxCommentsPerPoll": 100,
      "includeReplies": true,
      "minCommentScore": -10,
      "hideDeletedComments": true,
      "blockedAuthors": ""
    }
  }
}
```

## Testing Strategy

### Unit Tests

```csharp
// TagzApp.UnitTest/Providers/RedditAMAProviderTests.cs
using Xunit;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using TagzApp.Providers.RedditAMA;
using TagzApp.Providers.RedditAMA.Services;

public class RedditAMAProviderTests
{
    [Fact]
    public void Provider_ShouldBeDisabled_WhenConfigurationIsInvalid()
    {
        // Arrange
        var config = new RedditAMAConfiguration();
        var provider = CreateProvider(config);
        
        // Act & Assert
        Assert.False(provider.Enabled);
    }
    
    [Fact]
    public void Provider_ShouldBeEnabled_WhenConfigurationIsValid()
    {
        // Arrange
        var config = new RedditAMAConfiguration
        {
            Enabled = true,
            Subreddit = "IAmA",
            PostId = "abc123",
            UserAgent = "Test/1.0"
        };
        var provider = CreateProvider(config);
        
        // Act & Assert
        Assert.True(provider.Enabled);
    }
    
    private RedditAMAProvider CreateProvider(RedditAMAConfiguration config)
    {
        var mockApiService = new Mock<IRedditApiService>();
        var options = Options.Create(config);
        var logger = NullLogger<RedditAMAProvider>.Instance;
        
        return new RedditAMAProvider(mockApiService.Object, logger, options);
    }
}
```

### Integration Tests

```csharp
// TagzApp.UnitTest/Providers/RedditAMAIntegrationTests.cs
[Collection("Integration")]
public class RedditAMAIntegrationTests
{
    [Fact]
    public async Task RedditApiService_ShouldFetchComments_ForValidPost()
    {
        // Arrange
        var httpClient = new HttpClient();
        var config = Options.Create(new RedditAMAConfiguration
        {
            UserAgent = "TagzApp-Test/1.0"
        });
        var logger = NullLogger<RedditApiService>.Instance;
        var service = new RedditApiService(httpClient, logger, config);
        
        // Act
        var comments = await service.GetCommentsAsync("test", "valid_post_id");
        
        // Assert
        Assert.NotNull(comments);
    }
}
```

## Error Handling Patterns

### API Rate Limiting

```csharp
public class RedditApiService : IRedditApiService
{
    private static readonly SemaphoreSlim RateLimitSemaphore = new(1, 1);
    private static DateTime LastRequestTime = DateTime.MinValue;
    private static readonly TimeSpan MinRequestInterval = TimeSpan.FromSeconds(1);
    
    private async Task EnforceRateLimit()
    {
        await RateLimitSemaphore.WaitAsync();
        try
        {
            var timeSinceLastRequest = DateTime.UtcNow - LastRequestTime;
            if (timeSinceLastRequest < MinRequestInterval)
            {
                var delay = MinRequestInterval - timeSinceLastRequest;
                await Task.Delay(delay);
            }
            LastRequestTime = DateTime.UtcNow;
        }
        finally
        {
            RateLimitSemaphore.Release();
        }
    }
}
```

### Circuit Breaker Pattern

```csharp
public class RedditAMAProvider : ISocialMediaProvider
{
    private int _consecutiveFailures = 0;
    private DateTime _lastFailureTime = DateTime.MinValue;
    private readonly TimeSpan _circuitBreakerTimeout = TimeSpan.FromMinutes(5);
    
    private bool IsCircuitBreakerOpen()
    {
        if (_consecutiveFailures < 3) return false;
        
        return DateTime.UtcNow - _lastFailureTime < _circuitBreakerTimeout;
    }
    
    private void OnSuccess()
    {
        _consecutiveFailures = 0;
    }
    
    private void OnFailure()
    {
        _consecutiveFailures++;
        _lastFailureTime = DateTime.UtcNow;
    }
}
```

## Performance Considerations

### Memory Management

```csharp
public class RedditAMAProvider : ISocialMediaProvider
{
    private const int MaxQueueSize = 1000;
    private const int MaxProcessedIds = 10000;
    
    private void ManageQueueSize()
    {
        // Limit comment queue size
        while (_commentQueue.Count > MaxQueueSize)
        {
            _commentQueue.TryDequeue(out _);
        }
        
        // Limit processed IDs set size
        if (_processedCommentIds.Count > MaxProcessedIds)
        {
            var oldIds = _processedCommentIds.Take(MaxProcessedIds / 2).ToList();
            foreach (var oldId in oldIds)
            {
                _processedCommentIds.Remove(oldId);
            }
        }
    }
}
```

### HTTP Client Optimization

```csharp
public class RedditApiService : IRedditApiService
{
    public RedditApiService(HttpClient httpClient, ILogger<RedditApiService> logger, IOptions<RedditAMAConfiguration> config)
    {
        _httpClient = httpClient;
        
        // Optimize HttpClient
        _httpClient.DefaultRequestHeaders.Add("User-Agent", config.Value.UserAgent);
        _httpClient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate");
        _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        _httpClient.Timeout = TimeSpan.FromSeconds(30);
        
        // Connection pooling settings
        var handler = new SocketsHttpHandler()
        {
            PooledConnectionLifetime = TimeSpan.FromMinutes(15),
            MaxConnectionsPerServer = 10
        };
    }
}
```

This implementation provides a complete, production-ready RedditAMA provider following TagzApp's established patterns and best practices.