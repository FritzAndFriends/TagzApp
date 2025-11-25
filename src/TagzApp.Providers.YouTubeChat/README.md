# TagzApp YouTube Chat Provider

This provider integrates with YouTube Data API v3 to monitor live chat messages during YouTube live streams.

## YouTube API Quota Management

YouTube Data API v3 has daily quota limits (default: 10,000 units per day). This provider tracks API usage to help monitor quota consumption.

### API Quota Costs

Different API operations have different quota costs:

| API Call | Quota Cost | Usage |
|----------|-----------|--------|
| `LiveChatMessages.list` | 5 units | Fetching live chat messages (main polling operation) |
| `Search.list` | 100 units | Searching for broadcasts and channels |
| `Videos.list` | 1 unit | Getting video details and live chat IDs |

### Telemetry and Logging

The provider includes comprehensive telemetry to track API usage:

#### Structured Logging

Every API call logs:
- API operation name
- Quota cost for the call
- Cumulative quota used
- Additional context (e.g., message count, video ID)

Example log entry:
```
YouTube API call: LiveChatMessages.list - Quota cost: 5, Total quota used: 125, Messages retrieved: 42
```

#### Metrics (via ProviderInstrumentation)

The provider emits counter metrics with the following dimensions:
- `provider` - Set to "YOUTUBE-CHAT"
- `api_call` - The API operation (e.g., "LiveChatMessages.list", "Search.list", "Videos.list")
- `quota_used` - Cumulative quota consumed

These metrics can be monitored using OpenTelemetry exporters (e.g., Prometheus, Application Insights).

#### Health Check

The provider's health status includes current quota usage:
```
Status: Healthy
Message: OK -- adding (42) messages for chatid 'xyz' at 2025-01-15T10:30:00Z | API Quota Used: 125 units
```

### Monitoring Recommendations

1. **Set up alerts** when quota usage approaches the daily limit (e.g., 8,000 units)
2. **Monitor the quota_used metric** to track consumption trends
3. **Review logs** to identify high-cost operations (especially Search.list calls)
4. **Consider quota reset timing** - YouTube quotas reset at midnight Pacific Time

### Reducing Quota Usage

If you're hitting quota limits:
1. Increase polling interval (`NewContentRetrievalFrequency`)
2. Reduce broadcast search frequency
3. Request a quota increase from Google Cloud Console
4. Cache broadcast and channel information

## Configuration

See `YouTubeChatConfiguration.cs` for available configuration options including:
- `YouTubeApiKey` - Your YouTube Data API key
- `ChannelId` - The YouTube channel to monitor
- `BroadcastId` - Specific broadcast to monitor
- `LiveChatId` - Live chat ID for the stream
- `Enabled` - Enable/disable the provider

## Resources

- [YouTube Data API v3 Quota Calculator](https://developers.google.com/youtube/v3/determine_quota_cost)
- [YouTube Data API v3 Documentation](https://developers.google.com/youtube/v3)
