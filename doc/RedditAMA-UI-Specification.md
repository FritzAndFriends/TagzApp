# RedditAMA Provider - UI/UX Configuration Specification

## Overview

This document specifies the user interface design for the RedditAMA provider configuration panel in TagzApp's admin interface. The design follows TagzApp's existing provider configuration patterns while addressing the unique requirements of Reddit AMA monitoring.

## Design Principles

### 1. Consistency with Existing Providers
- Follow the same layout patterns as TwitchChat and YouTube provider configs
- Use consistent form styling and validation patterns
- Maintain the same navigation structure and breadcrumbs

### 2. Simplicity First
- Primary workflow: paste Reddit URL → auto-configure → save
- Advanced settings are collapsible/secondary
- Clear visual feedback for configuration state

### 3. Error Prevention
- Real-time URL validation and parsing
- Clear error messages with actionable solutions
- Test connection functionality before saving

## Page Structure

### Location and Navigation
```
Admin Dashboard
└── Providers
    ├── Twitter
    ├── YouTube
    ├── TwitchChat
    ├── Mastodon
    ├── Bluesky
    └── Reddit AMA ← New provider
```

**URL**: `/admin/providers/redditama`
**Page Title**: "Reddit AMA Provider Configuration"
**Breadcrumb**: Admin > Providers > Reddit AMA

## Layout Specification

### Main Configuration Panel

```html
<div class="provider-configuration reddit-ama-provider">
  <!-- Header Section -->
  <div class="provider-header">
    <div class="provider-icon">
      <img src="/img/providers/reddit-icon.svg" alt="Reddit" />
    </div>
    <div class="provider-info">
      <h2>Reddit AMA Provider</h2>
      <p class="provider-description">
        Monitor specific Reddit threads for live comments during AMAs, Q&A sessions, 
        and community discussions.
      </p>
    </div>
    <div class="provider-status">
      <span class="status-indicator @(IsEnabled ? "enabled" : "disabled")">
        @(IsEnabled ? "Enabled" : "Disabled")
      </span>
    </div>
  </div>

  <!-- Configuration Form -->
  <form class="provider-config-form" @onsubmit="SaveConfiguration">
    <!-- Enable/Disable Toggle -->
    <div class="form-section">
      <div class="form-group toggle-group">
        <label class="toggle-label">
          <input type="checkbox" @bind="Config.Enabled" class="toggle-input" />
          <span class="toggle-slider"></span>
          <span class="toggle-text">Enable Reddit AMA Provider</span>
        </label>
        <p class="form-help">
          When enabled, TagzApp will monitor the specified Reddit thread for new comments.
        </p>
      </div>
    </div>

    <!-- Primary Configuration -->
    <div class="form-section" disabled="@(!Config.Enabled)">
      <h3>Thread Configuration</h3>
      
      <!-- Reddit URL Input -->
      <div class="form-group url-input-group">
        <label for="reddit-url">Reddit Post URL *</label>
        <div class="input-with-button">
          <input type="url" 
                 id="reddit-url"
                 @bind="RedditUrl" 
                 @onblur="ParseRedditUrl"
                 placeholder="https://reddit.com/r/IAmA/comments/abc123/..."
                 class="form-control @(UrlParseError ? "error" : "")"
                 required />
          <button type="button" 
                  @onclick="ParseRedditUrl" 
                  class="btn btn-secondary parse-btn"
                  disabled="@string.IsNullOrEmpty(RedditUrl)">
            Parse URL
          </button>
        </div>
        @if (UrlParseError)
        {
          <div class="form-error">
            <i class="icon-warning"></i>
            @UrlParseErrorMessage
          </div>
        }
        <p class="form-help">
          Paste the complete URL of the Reddit post you want to monitor. 
          Example: https://reddit.com/r/IAmA/comments/abc123/i_am_john_doe_ama/
        </p>
      </div>

      <!-- Parsed Information Display -->
      @if (!string.IsNullOrEmpty(Config.Subreddit) && !string.IsNullOrEmpty(Config.PostId))
      {
        <div class="parsed-info-panel">
          <h4>Detected Thread Information</h4>
          <div class="info-grid">
            <div class="info-item">
              <label>Subreddit</label>
              <span class="info-value">r/@Config.Subreddit</span>
            </div>
            <div class="info-item">
              <label>Post ID</label>
              <span class="info-value">@Config.PostId</span>
            </div>
            @if (!string.IsNullOrEmpty(Config.PostTitle))
            {
              <div class="info-item full-width">
                <label>Post Title</label>
                <span class="info-value">@Config.PostTitle</span>
              </div>
            }
          </div>
          @if (!string.IsNullOrEmpty(RedditUrl))
          {
            <div class="thread-preview">
              <a href="@RedditUrl" target="_blank" class="btn btn-link">
                <i class="icon-external-link"></i>
                View on Reddit
              </a>
            </div>
          }
        </div>
      }
    </div>

    <!-- Advanced Settings (Collapsible) -->
    <div class="form-section advanced-settings" disabled="@(!Config.Enabled)">
      <div class="section-header" @onclick="ToggleAdvancedSettings">
        <h3>
          <i class="icon-chevron @(ShowAdvancedSettings ? "expanded" : "collapsed")"></i>
          Advanced Settings
        </h3>
        <span class="section-toggle">@(ShowAdvancedSettings ? "Hide" : "Show")</span>
      </div>

      @if (ShowAdvancedSettings)
      {
        <div class="advanced-settings-content">
          <!-- Polling Configuration -->
          <div class="form-row">
            <div class="form-group">
              <label for="refresh-interval">Refresh Interval (seconds)</label>
              <input type="number" 
                     id="refresh-interval"
                     @bind="Config.RefreshIntervalSeconds" 
                     min="15" max="300" 
                     class="form-control" />
              <p class="form-help">
                How often to check for new comments (15-300 seconds). 
                Lower values provide faster updates but use more API calls.
              </p>
            </div>
            <div class="form-group">
              <label for="max-comments">Max Comments Per Check</label>
              <input type="number" 
                     id="max-comments"
                     @bind="Config.MaxCommentsPerPoll" 
                     min="10" max="500" 
                     class="form-control" />
              <p class="form-help">
                Maximum number of comments to fetch in each API call.
              </p>
            </div>
          </div>

          <!-- Comment Filtering -->
          <div class="form-group">
            <label>Comment Filtering Options</label>
            <div class="checkbox-group">
              <label class="checkbox-label">
                <input type="checkbox" @bind="Config.IncludeReplies" />
                <span class="checkbox-text">Include comment replies</span>
              </label>
              <label class="checkbox-label">
                <input type="checkbox" @bind="Config.HideDeletedComments" />
                <span class="checkbox-text">Hide deleted/removed comments</span>
              </label>
            </div>
          </div>

          <!-- Score Filtering -->
          <div class="form-group">
            <label for="min-score">Minimum Comment Score</label>
            <input type="number" 
                   id="min-score"
                   @bind="Config.MinCommentScore" 
                   class="form-control" />
            <p class="form-help">
              Hide comments with scores below this value. 
              Use -10 to hide heavily downvoted comments while keeping most content.
            </p>
          </div>

          <!-- User Blocking -->
          <div class="form-group">
            <label for="blocked-authors">Blocked Authors</label>
            <textarea id="blocked-authors"
                      @bind="BlockedAuthorsText" 
                      rows="3" 
                      class="form-control"
                      placeholder="Enter usernames to block, one per line:&#10;spamuser1&#10;trollaccount&#10;bot_account"></textarea>
            <p class="form-help">
              Block comments from specific Reddit users. Enter one username per line.
            </p>
          </div>

          <!-- API Configuration -->
          <div class="form-group">
            <label for="user-agent">User Agent String</label>
            <input type="text" 
                   id="user-agent"
                   @bind="Config.UserAgent" 
                   class="form-control" 
                   required />
            <p class="form-help">
              Required by Reddit's API. Should identify your application 
              (e.g., "TagzApp/1.0 by /u/yourusername").
            </p>
          </div>
        </div>
      }
    </div>

    <!-- Actions Section -->
    <div class="form-actions">
      <div class="action-group">
        <button type="button" 
                @onclick="TestConnection" 
                class="btn btn-secondary"
                disabled="@(!CanTestConnection)">
          <i class="icon-@(TestInProgress ? "spinner" : "test")"></i>
          @(TestInProgress ? "Testing..." : "Test Connection")
        </button>
        
        <button type="button" 
                @onclick="ResetConfiguration" 
                class="btn btn-outline">
          <i class="icon-reset"></i>
          Reset
        </button>
      </div>
      
      <div class="action-group primary">
        <button type="submit" 
                class="btn btn-primary"
                disabled="@(!CanSaveConfiguration)">
          <i class="icon-save"></i>
          Save Configuration
        </button>
      </div>
    </div>
  </form>

  <!-- Test Results Panel -->
  @if (!string.IsNullOrEmpty(TestResult))
  {
    <div class="test-results @(TestSuccess ? "success" : "error")">
      <div class="result-header">
        <i class="icon-@(TestSuccess ? "check-circle" : "error-circle")"></i>
        <h4>@(TestSuccess ? "Connection Test Successful" : "Connection Test Failed")</h4>
      </div>
      <div class="result-content">
        <p>@TestResult</p>
        @if (TestSuccess && TestMetadata != null)
        {
          <div class="test-metadata">
            <div class="metadata-item">
              <label>Post Found</label>
              <span>@TestMetadata.PostTitle</span>
            </div>
            <div class="metadata-item">
              <label>Comments Available</label>
              <span>@TestMetadata.CommentCount</span>
            </div>
            <div class="metadata-item">
              <label>Last Activity</label>
              <span>@TestMetadata.LastActivity.ToString("MMM dd, yyyy HH:mm")</span>
            </div>
          </div>
        }
      </div>
    </div>
  }
</div>
```

## Status Dashboard Component

### Provider Status Panel
Located in the main providers dashboard, shows real-time status:

```html
<div class="provider-status-card reddit-ama-status">
  <div class="status-header">
    <div class="provider-icon">
      <img src="/img/providers/reddit-icon.svg" alt="Reddit AMA" />
    </div>
    <div class="provider-name">
      <h4>Reddit AMA</h4>
      <span class="status-badge @(Status.IsConnected ? "connected" : "disconnected")">
        @(Status.IsConnected ? "Active" : "Inactive")
      </span>
    </div>
  </div>

  <div class="status-metrics">
    <div class="metric">
      <label>Monitoring</label>
      <span class="metric-value">
        @if (!string.IsNullOrEmpty(Config.PostTitle))
        {
          <text>@Config.PostTitle.Truncate(40)</text>
        }
        else
        {
          <text>r/@Config.Subreddit</text>
        }
      </span>
    </div>
    
    <div class="metric">
      <label>Last Updated</label>
      <span class="metric-value">@Status.LastUpdated.ToString("HH:mm:ss")</span>
    </div>
    
    <div class="metric">
      <label>Queued Comments</label>
      <span class="metric-value">@Status.ExtendedInfo["queuedComments"]</span>
    </div>
  </div>

  <div class="status-actions">
    <a href="/admin/providers/redditama" class="btn btn-sm btn-secondary">
      Configure
    </a>
    @if (Config.Enabled)
    {
      <button @onclick="DisableProvider" class="btn btn-sm btn-outline">
        Disable
      </button>
    }
  </div>
</div>
```

## Interactive Behavior Specification

### URL Parsing Flow
1. **User pastes Reddit URL** into the URL field
2. **On blur or Parse button click**:
   - Validate URL format
   - Extract subreddit and post ID
   - Show loading state
   - Fetch post title from Reddit API
   - Update parsed info panel
   - Show success/error feedback

### Validation States

#### URL Validation
```typescript
interface URLValidationState {
  isValid: boolean;
  errorMessage?: string;
  parsedData?: {
    subreddit: string;
    postId: string;
    postTitle?: string;
  };
}

// Valid URL patterns:
// - https://reddit.com/r/IAmA/comments/abc123/title/
// - https://www.reddit.com/r/dotnet/comments/xyz789/announcement/
// - https://old.reddit.com/r/programming/comments/123abc/discussion/
```

#### Form Validation Rules
```csharp
public class RedditURLValidator
{
    public ValidationResult ValidateURL(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return ValidationResult.Error("Reddit URL is required");
        
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            return ValidationResult.Error("Invalid URL format");
        
        if (!IsRedditDomain(uri.Host))
            return ValidationResult.Error("URL must be from reddit.com");
        
        var pathParts = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (pathParts.Length < 4 || pathParts[0] != "r" || pathParts[2] != "comments")
            return ValidationResult.Error("Invalid Reddit post URL format");
        
        return ValidationResult.Success(new ParsedRedditURL
        {
            Subreddit = pathParts[1],
            PostId = pathParts[3]
        });
    }
}
```

### Connection Testing Flow
1. **User clicks "Test Connection"**
2. **Show loading state** with spinner
3. **Perform API calls**:
   - Test Reddit API connectivity
   - Verify post exists and is accessible
   - Fetch sample comments (if any)
4. **Display results**:
   - Success: Show post title, comment count, last activity
   - Failure: Show specific error message with suggested fix

### Save Configuration Flow
1. **Validate all required fields**
2. **Show confirmation dialog** if changing from enabled to disabled
3. **Save to configuration**
4. **Update provider status**
5. **Show success message**
6. **Redirect to provider dashboard** (optional)

## Visual Design Specifications

### Color Scheme
Following TagzApp's design system:
- **Primary**: `#FF4500` (Reddit orange)
- **Success**: `#28a745`
- **Error**: `#dc3545`
- **Warning**: `#ffc107`
- **Info**: `#17a2b8`

### Typography
- **Headers**: `font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif`
- **Body text**: `font-size: 14px; line-height: 1.5`
- **Help text**: `font-size: 12px; color: #666`
- **Code/URLs**: `font-family: 'Consolas', 'Monaco', monospace`

### Spacing and Layout
- **Section margins**: `margin-bottom: 2rem`
- **Form group margins**: `margin-bottom: 1rem`
- **Input padding**: `padding: 0.5rem 0.75rem`
- **Button padding**: `padding: 0.5rem 1rem`

### Icons
- **Provider icon**: Reddit logo (SVG)
- **Status indicators**: Check circle, error circle, warning triangle
- **UI icons**: Chevron (expand/collapse), external link, spinner (loading)

### Responsive Behavior
```css
/* Desktop (default) */
.form-row {
  display: flex;
  gap: 1rem;
}

.form-row .form-group {
  flex: 1;
}

/* Tablet */
@media (max-width: 768px) {
  .form-row {
    flex-direction: column;
  }
  
  .input-with-button {
    flex-direction: column;
  }
  
  .parse-btn {
    margin-top: 0.5rem;
  }
}

/* Mobile */
@media (max-width: 480px) {
  .provider-header {
    flex-direction: column;
    text-align: center;
  }
  
  .form-actions {
    flex-direction: column;
  }
  
  .action-group {
    width: 100%;
    margin-bottom: 0.5rem;
  }
}
```

## Accessibility Requirements

### WCAG 2.1 AA Compliance
- **Keyboard navigation**: All interactive elements accessible via keyboard
- **Screen readers**: Proper ARIA labels and descriptions
- **Color contrast**: Minimum 4.5:1 ratio for text
- **Focus indicators**: Clear visual focus states

### Specific Implementation
```html
<!-- Form labels properly associated -->
<label for="reddit-url">Reddit Post URL *</label>
<input type="url" id="reddit-url" aria-describedby="url-help" required />
<p id="url-help" class="form-help">Paste the complete URL...</p>

<!-- Error states announced -->
<div class="form-error" role="alert" aria-live="polite">
  <i class="icon-warning" aria-hidden="true"></i>
  Invalid URL format
</div>

<!-- Status updates announced -->
<div class="test-results success" role="status" aria-live="polite">
  Connection test successful
</div>

<!-- Collapsible sections -->
<button type="button" 
        aria-expanded="@ShowAdvancedSettings"
        aria-controls="advanced-settings-content">
  Advanced Settings
</button>
<div id="advanced-settings-content" aria-hidden="@(!ShowAdvancedSettings)">
  <!-- Advanced settings content -->
</div>
```

## Error States and Messages

### Common Error Scenarios

#### URL Parsing Errors
- **Empty URL**: "Please enter a Reddit post URL"
- **Invalid format**: "Please enter a valid URL (e.g., https://reddit.com/r/IAmA/comments/...)"
- **Not a Reddit URL**: "URL must be from reddit.com"
- **Wrong URL structure**: "This doesn't appear to be a Reddit post URL. Please use the format: reddit.com/r/subreddit/comments/postid/"
- **Post not found**: "This Reddit post could not be found. Please check the URL and try again."
- **Private subreddit**: "This subreddit is private and cannot be monitored."

#### Configuration Errors
- **Missing subreddit**: "Subreddit is required"
- **Missing post ID**: "Reddit post ID is required"
- **Invalid refresh interval**: "Refresh interval must be between 15 and 300 seconds"
- **Invalid user agent**: "User agent is required for Reddit API access"

#### Connection Test Errors
- **API unreachable**: "Unable to connect to Reddit API. Please check your internet connection."
- **Rate limited**: "Reddit API rate limit exceeded. Please try again in a few minutes."
- **Post deleted**: "This Reddit post has been deleted and cannot be monitored."
- **Subreddit banned**: "This subreddit has been banned or made private."

### Success Messages
- **Configuration saved**: "Reddit AMA provider configuration saved successfully"
- **Connection test passed**: "Successfully connected to Reddit and found the post"
- **Provider enabled**: "Reddit AMA provider is now monitoring comments"
- **Provider disabled**: "Reddit AMA provider has been disabled"

## Performance Considerations

### Loading States
- **URL parsing**: Show spinner next to Parse button
- **Connection testing**: Replace button text with "Testing..." and spinner
- **Configuration saving**: Show loading overlay on form
- **Page loading**: Skeleton loading for configuration form

### Debouncing
- **URL input**: Debounce parsing by 500ms after user stops typing
- **Configuration changes**: Debounce auto-save by 1000ms
- **API calls**: Prevent duplicate requests while one is in progress

### Error Recovery
- **Network failures**: Automatic retry with exponential backoff
- **Invalid configurations**: Clear error states when user corrects input
- **Session timeouts**: Graceful handling with re-authentication prompt

## Testing Checklist

### Functional Testing
- [ ] URL parsing works with various Reddit URL formats
- [ ] Configuration validation prevents invalid settings
- [ ] Connection test accurately reports Reddit API status
- [ ] Save/load configuration persists correctly
- [ ] Enable/disable provider works as expected

### UI/UX Testing
- [ ] Form is accessible via keyboard navigation
- [ ] Error messages are clear and actionable
- [ ] Success states provide appropriate feedback
- [ ] Responsive design works on mobile devices
- [ ] Loading states don't block user interaction

### Integration Testing
- [ ] Provider configuration integrates with main dashboard
- [ ] Status updates reflect actual provider state
- [ ] Configuration changes trigger provider restart
- [ ] Error handling doesn't crash the admin interface

This specification provides comprehensive guidance for implementing a user-friendly, accessible, and robust configuration interface for the RedditAMA provider.