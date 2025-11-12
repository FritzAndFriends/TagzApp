# RedditAMA Provider Design Document

## Overview

The RedditAMA (Ask Me Anything) Provider is a specialized social media provider for TagzApp that monitors a specific Reddit post/thread for new comments in real-time. This provider is designed to support live events such as AMAs, Q&A sessions, product launches, and community discussions where Reddit serves as the primary interaction platform.

Unlike traditional hashtag-based providers, RedditAMA focuses on tracking a single conversation thread, making it ideal for event-based content aggregation similar to the existing TwitchChat provider.

## Architecture

### Provider Type
**Event-Based Provider** - Monitors a specific Reddit post/thread rather than searching across multiple posts.

### Implementation Pattern
Follows the existing `TwitchChatProvider` and `YouTubeChatProvider` pattern:
- Background polling service
- Concurrent queue for new comments
- Real-time content delivery to TagzApp waterfall

### Core Components

1. **RedditAMAProvider** - Main provider implementation
2. **RedditAMAConfiguration** - Configuration model and settings
3. **RedditAMAService** - Background polling service
4. **RedditCommentModel** - Reddit API response models
5. **Configuration UI** - Admin interface for setup
6. **User Documentation** - Setup and usage guides

## Technical Specifications

### API Integration

#### Reddit API Endpoints
```
Primary: https://www.reddit.com/r/{subreddit}/comments/{postId}.json?sort=new&limit=100
Fallback: https://oauth.reddit.com/r/{subreddit}/comments/{postId}
```

#### Authentication
- **Public API**: No authentication required for public posts
- **Rate Limiting**: 60 requests per minute (Reddit's standard limit)
- **User-Agent**: Required by Reddit API, configurable

#### Polling Strategy
- **Interval**: 30 seconds (configurable, minimum 15 seconds)
- **Comment Tracking**: Track last comment ID to avoid duplicates
- **Error Handling**: Exponential backoff on API failures
- **Queue Management**: Maximum 1000 comments in memory queue

### Data Models

#### RedditAMAConfiguration
```csharp
public class RedditAMAConfiguration : BaseProviderConfiguration<RedditAMAConfiguration>
{
    public const string AppSettingsSection = "providers:redditama";
    
    public string Description => "Monitor Reddit AMA/discussion threads for live comments";
    public string Name => "Reddit AMA";
    public bool Enabled { get; set; } = false;
    
    // Reddit-specific settings
    public string Subreddit { get; set; } = string.Empty;     // e.g., "IAmA", "dotnet"
    public string PostId { get; set; } = string.Empty;        // e.g., "16abc123"
    public string PostTitle { get; set; } = string.Empty;     // For display purposes
    public string UserAgent { get; set; } = "TagzApp/1.0";    // Required by Reddit
    
    // Polling settings
    public int RefreshIntervalSeconds { get; set; } = 30;     // Min 15, Max 300
    public int MaxCommentsPerPoll { get; set; } = 100;        // Max comments to fetch
    public bool IncludeReplies { get; set; } = true;          // Include comment replies
    
    // Filtering options
    public int MinCommentScore { get; set; } = -10;           // Hide heavily downvoted
    public bool HideDeletedComments { get; set; } = true;     // Filter [deleted] comments
    public string[] BlockedAuthors { get; set; } = Array.Empty<string>(); // Blocked usernames
    
    public string[] Keys => new[]
    {
        nameof(Subreddit),
        nameof(PostId),
        nameof(UserAgent),
        nameof(RefreshIntervalSeconds),
        nameof(MaxCommentsPerPoll),
        nameof(IncludeReplies),
        nameof(MinCommentScore),
        nameof(HideDeletedComments)
    };
    
    // Validation
    public bool IsValid => !string.IsNullOrEmpty(Subreddit) && 
                          !string.IsNullOrEmpty(PostId) && 
                          RefreshIntervalSeconds >= 15;
}
```

#### RedditComment Model
```csharp
public class RedditCommentData
{
    public string Id { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public long CreatedUtc { get; set; }
    public string Permalink { get; set; } = string.Empty;
    public int Score { get; set; }
    public int Depth { get; set; }
    public string ParentId { get; set; } = string.Empty;
    public bool IsSubmitter { get; set; }  // Is this the AMA host?
    public string Distinguished { get; set; } = string.Empty; // "moderator", "admin", etc.
}

public class RedditCommentWrapper
{
    public string Kind { get; set; } = string.Empty;
    public RedditCommentData Data { get; set; } = new();
}

public class RedditThreadResponse
{
    public RedditCommentWrapper[] Children { get; set; } = Array.Empty<RedditCommentWrapper>();
}
```

### Content Mapping

#### Content Properties
```csharp
var content = new Content
{
    Provider = "REDDIT_AMA",
    ProviderId = comment.Data.Id,
    Type = ContentType.Message,
    Timestamp = DateTimeOffset.FromUnixTimeSeconds(comment.Data.CreatedUtc),
    SourceUri = new Uri($"https://reddit.com{comment.Data.Permalink}"),
    Author = new Creator
    {
        DisplayName = comment.Data.Author,
        UserName = comment.Data.Author,
        ProfileUri = new Uri($"https://reddit.com/u/{comment.Data.Author}"),
        // Special handling for AMA host
        ProfileImageUri = comment.Data.IsSubmitter ? 
            new Uri("https://tagzapp.io/img/reddit-op-badge.png") : null
    },
    Text = comment.Data.Body,
    HashtagSought = tag.Text.ToLowerInvariant(),
    
    // Reddit-specific metadata
    ExtendedMetadata = new Dictionary<string, object>
    {
        ["score"] = comment.Data.Score,
        ["depth"] = comment.Data.Depth,
        ["isOP"] = comment.Data.IsSubmitter,
        ["distinguished"] = comment.Data.Distinguished
    }
};
```

## Database Storage

### Configuration Storage
Following the existing pattern used by other providers:

#### appsettings.json
```json
{
  "providers": {
    "redditama": {
      "enabled": false,
      "subreddit": "",
      "postId": "",
      "postTitle": "",
      "userAgent": "TagzApp/1.0",
      "refreshIntervalSeconds": 30,
      "maxCommentsPerPoll": 100,
      "includeReplies": true,
      "minCommentScore": -10,
      "hideDeletedComments": true,
      "blockedAuthors": []
    }
  }
}
```

#### Entity Framework Configuration
No additional database tables required - uses existing:
- `Hashtag` table for event tracking
- `Content` table for comment storage
- `Creator` table for Reddit user information

#### Configuration Encryption
Supports TagzApp's configuration encryption for sensitive settings:
```csharp
[EncryptedConfiguration]
public class RedditAMAConfiguration : BaseProviderConfiguration<RedditAMAConfiguration>
{
    // Configuration properties...
}
```

## User Interface

### Admin Configuration Panel

#### Location
- **Path**: `/admin/providers/redditama`
- **Navigation**: Admin → Providers → Reddit AMA
- **Permissions**: Requires `Admin` role

#### Configuration Form
```html
<div class="provider-config reddit-ama-config">
    <h3>Reddit AMA Provider Configuration</h3>
    
    <div class="form-group">
        <label>Enable Provider</label>
        <input type="checkbox" @bind="Config.Enabled" />
        <small>Enable real-time monitoring of Reddit AMA threads</small>
    </div>
    
    <div class="form-group">
        <label>Reddit Post URL *</label>
        <input type="url" @bind="RedditUrl" @onblur="ParseRedditUrl" 
               placeholder="https://reddit.com/r/IAmA/comments/abc123/..." />
        <small>Paste the full Reddit post URL here</small>
    </div>
    
    <div class="form-row">
        <div class="form-group">
            <label>Subreddit *</label>
            <input type="text" @bind="Config.Subreddit" readonly />
        </div>
        <div class="form-group">
            <label>Post ID *</label>
            <input type="text" @bind="Config.PostId" readonly />
        </div>
    </div>
    
    <div class="form-group">
        <label>Post Title</label>
        <input type="text" @bind="Config.PostTitle" readonly />
        <small>Auto-populated from Reddit API</small>
    </div>
    
    <div class="form-group">
        <label>User Agent</label>
        <input type="text" @bind="Config.UserAgent" />
        <small>Required by Reddit API (e.g., "TagzApp/1.0")</small>
    </div>
    
    <div class="form-row">
        <div class="form-group">
            <label>Refresh Interval (seconds)</label>
            <input type="number" @bind="Config.RefreshIntervalSeconds" min="15" max="300" />
            <small>How often to check for new comments (15-300 seconds)</small>
        </div>
        <div class="form-group">
            <label>Max Comments Per Check</label>
            <input type="number" @bind="Config.MaxCommentsPerPoll" min="10" max="500" />
        </div>
    </div>
    
    <div class="form-group">
        <label>Comment Filtering</label>
        <div class="checkbox-group">
            <label><input type="checkbox" @bind="Config.IncludeReplies" /> Include comment replies</label>
            <label><input type="checkbox" @bind="Config.HideDeletedComments" /> Hide deleted comments</label>
        </div>
    </div>
    
    <div class="form-group">
        <label>Minimum Comment Score</label>
        <input type="number" @bind="Config.MinCommentScore" />
        <small>Hide comments below this score (-10 recommended)</small>
    </div>
    
    <div class="form-group">
        <label>Blocked Authors</label>
        <textarea @bind="BlockedAuthorsText" rows="3" 
                  placeholder="username1&#10;username2&#10;..."></textarea>
        <small>One username per line</small>
    </div>
    
    <div class="form-actions">
        <button type="button" @onclick="TestConnection" class="btn btn-secondary">
            Test Connection
        </button>
        <button type="submit" class="btn btn-primary" disabled="@(!Config.IsValid)">
            Save Configuration
        </button>
    </div>
    
    @if (!string.IsNullOrEmpty(TestResult))
    {
        <div class="alert @(TestSuccess ? "alert-success" : "alert-danger")">
            @TestResult
        </div>
    }
</div>
```

#### URL Parsing Logic
```csharp
private void ParseRedditUrl()
{
    if (Uri.TryCreate(RedditUrl, UriKind.Absolute, out var uri))
    {
        // Parse: https://reddit.com/r/IAmA/comments/abc123/title/
        var segments = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length >= 4 && segments[0] == "r" && segments[2] == "comments")
        {
            Config.Subreddit = segments[1];
            Config.PostId = segments[3];
            
            // Fetch post title from Reddit API
            _ = FetchPostTitle();
        }
    }
}
```

### Provider Status Dashboard

#### Live Status Panel
```html
<div class="provider-status reddit-ama-status">
    <h4>Reddit AMA Status</h4>
    
    <div class="status-grid">
        <div class="status-item">
            <label>Connection Status</label>
            <span class="status @(IsConnected ? "connected" : "disconnected")">
                @(IsConnected ? "Connected" : "Disconnected")
            </span>
        </div>
        
        <div class="status-item">
            <label>Last Updated</label>
            <span>@LastUpdate.ToString("HH:mm:ss")</span>
        </div>
        
        <div class="status-item">
            <label>Comments Queued</label>
            <span>@QueuedComments</span>
        </div>
        
        <div class="status-item">
            <label>Total Comments</label>
            <span>@TotalComments</span>
        </div>
    </div>
    
    <div class="current-thread">
        <label>Monitoring Thread</label>
        <a href="@ThreadUrl" target="_blank">@ThreadTitle</a>
    </div>
</div>
```

## Service Integration

### Dependency Injection
```csharp
// Program.cs or ServiceCollectionExtensions
services.Configure<RedditAMAConfiguration>(
    builder.Configuration.GetSection(RedditAMAConfiguration.AppSettingsSection));

services.AddScoped<ISocialMediaProvider, RedditAMAProvider>();
services.AddHostedService<RedditAMABackgroundService>();
services.AddHttpClient<RedditAMAProvider>();
```

### Background Service
```csharp
public class RedditAMABackgroundService : BackgroundService
{
    private readonly RedditAMAProvider _provider;
    private readonly ILogger<RedditAMABackgroundService> _logger;
    private readonly RedditAMAConfiguration _config;
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested && _config.Enabled)
        {
            try
            {
                await _provider.PollForNewComments();
                await Task.Delay(TimeSpan.FromSeconds(_config.RefreshIntervalSeconds), stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error polling Reddit AMA comments");
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken); // Backoff on error
            }
        }
    }
}
```

## Error Handling

### API Error Scenarios
1. **Reddit API Down**: Exponential backoff, max 5 minutes between retries
2. **Post Not Found**: Disable provider, log error, notify admin
3. **Rate Limiting**: Respect Reddit's rate limits, increase polling interval
4. **Network Issues**: Retry with jitter, maintain queue during outages
5. **Invalid Configuration**: Validate settings, provide helpful error messages

### Logging Strategy
```csharp
_logger.LogInformation("RedditAMA: Polling thread {ThreadId} in r/{Subreddit}", postId, subreddit);
_logger.LogWarning("RedditAMA: Rate limited, increasing interval to {Interval}s", newInterval);
_logger.LogError(ex, "RedditAMA: Failed to fetch comments for thread {ThreadId}", postId);
```

## Performance Considerations

### Memory Management
- **Queue Size Limit**: Maximum 1000 comments in memory
- **Comment Deduplication**: Track processed comment IDs to avoid duplicates
- **Garbage Collection**: Clear old comment IDs after 24 hours

### Network Optimization
- **HTTP Client Reuse**: Single HttpClient instance with connection pooling
- **Compression**: Accept gzip encoding from Reddit API
- **Conditional Requests**: Use If-Modified-Since when supported

### Scalability
- **Multi-Thread Support**: Multiple AMA providers can run simultaneously
- **Resource Limits**: Configurable memory and network usage limits
- **Circuit Breaker**: Disable provider after consecutive failures

## Testing Strategy

### Unit Tests
- Configuration validation
- Comment parsing and mapping
- Queue management
- Error handling scenarios

### Integration Tests
- Reddit API integration
- Database storage
- Background service lifecycle
- Rate limiting behavior

### Manual Testing Scenarios
1. Configure provider with live AMA thread
2. Verify comments appear in TagzApp waterfall
3. Test configuration UI with various Reddit URLs
4. Verify error handling with invalid post IDs
5. Test performance with high-comment threads

## Security Considerations

### Data Privacy
- **No Authentication**: Provider only accesses public Reddit data
- **User Data**: Store minimal Reddit user information
- **Content Filtering**: Support blocking inappropriate usernames/content

### Rate Limiting
- **Respect Reddit TOS**: Stay within API rate limits
- **Configurable Intervals**: Allow users to reduce polling frequency
- **Backoff Strategy**: Increase intervals during high load

### Input Validation
- **URL Sanitization**: Validate Reddit URLs before parsing
- **Configuration Validation**: Ensure safe configuration values
- **Content Sanitization**: Filter malicious content from comments

## Deployment Considerations

### Configuration Migration
- **Default Settings**: Safe defaults for new installations
- **Upgrade Path**: Migrate from beta configurations
- **Environment Variables**: Support Docker/Kubernetes deployment

### Monitoring
- **Health Checks**: Provider health endpoint
- **Metrics**: Comment processing rates, API response times
- **Alerting**: Notify admins of provider failures

### Documentation Requirements
- **Admin Guide**: Configuration and troubleshooting
- **User Guide**: How to use RedditAMA for events  
- **API Documentation**: Technical integration details

## Future Enhancements

### Phase 2 Features
- **Multiple Thread Support**: Monitor multiple AMA threads simultaneously
- **Comment Threading**: Display Reddit comment hierarchy in TagzApp
- **Sentiment Analysis**: Analyze comment sentiment for AMA hosts
- **Auto-Discovery**: Automatically find AMA threads by host user

### Integration Opportunities
- **Twitch Integration**: Combine Reddit AMA with Twitch chat for live streams
- **YouTube Integration**: Sync with YouTube premiere chat
- **Discord Integration**: Cross-post questions to Discord servers
- **Analytics**: Track AMA engagement metrics and popular questions

## Success Criteria

### Technical Success
- ✅ Provider processes Reddit comments in real-time (< 60 second delay)
- ✅ Handles Reddit API rate limits gracefully
- ✅ Configuration UI is intuitive and validates inputs
- ✅ No memory leaks during extended operation
- ✅ Proper error handling and logging

### User Experience Success
- ✅ Event organizers can easily configure AMA monitoring
- ✅ Comments appear in TagzApp waterfall with proper formatting
- ✅ Admin can moderate inappropriate comments
- ✅ Performance remains stable during high-activity AMAs
- ✅ Clear documentation for setup and usage

### Business Success
- ✅ Enables new use cases for TagzApp (AMAs, Q&A sessions, product launches)
- ✅ Differentiates TagzApp from other social media aggregators
- ✅ Provides value for community events and live streaming
- ✅ Easy to demonstrate and explain to potential users

---

## Implementation Timeline

### Phase 1: Core Provider (2 weeks)
- [ ] Reddit API integration and comment parsing
- [ ] Basic provider implementation following TwitchChat pattern
- [ ] Configuration model and validation
- [ ] Unit tests for core functionality

### Phase 2: UI and Configuration (1 week)
- [ ] Admin configuration panel
- [ ] URL parsing and validation
- [ ] Provider status dashboard
- [ ] Configuration persistence

### Phase 3: Polish and Documentation (1 week)
- [ ] Error handling and logging
- [ ] Performance optimization
- [ ] User documentation
- [ ] Integration testing

### Phase 4: Testing and Deployment (1 week)
- [ ] Manual testing with live AMA threads
- [ ] Load testing and performance validation
- [ ] Documentation review
- [ ] Production deployment preparation

**Total Estimated Timeline: 5 weeks**

---

*This design document serves as the comprehensive specification for implementing the RedditAMA provider in TagzApp. It should be reviewed and approved by the development team before implementation begins.*