# Mastodon Provider Configuration Guide

This guide will help you configure the Mastodon content provider for TagzApp to monitor hashtags across the Mastodon federated social network.

## Overview

The Mastodon provider allows TagzApp to search and display posts (toots) containing specific hashtags from Mastodon instances. Mastodon is a federated social network, meaning it consists of many independent servers (instances) that communicate with each other.

## Prerequisites

- A Mastodon account (optional, but recommended)
- Access to a Mastodon instance's public API (most instances allow public read access)
- No API keys required for public hashtag search!

## Understanding Mastodon Federation

Mastodon is federated, meaning:
- There are many independent Mastodon servers (instances)
- Each instance has its own domain (e.g., mastodon.social, fosstodon.org, etc.)
- Instances communicate with each other to share content
- You can search hashtags on any public instance

**Note:** By default, TagzApp searches `mastodon.social`, but you can configure it to search any public Mastodon instance.

## Step 1: Choose a Mastodon Instance

You need to decide which Mastodon instance to search:

### Popular Public Instances

- **mastodon.social** - The flagship instance (default in TagzApp)
- **fosstodon.org** - Technology and open-source focused
- **mas.to** - General purpose
- **techhub.social** - Technology focused
- **mastodon.online** - General purpose

### Finding Instances

- Browse instances at [joinmastodon.org/servers](https://joinmastodon.org/servers)
- Consider instances relevant to your event's topic
- Choose larger instances for broader hashtag coverage
- Verify the instance has public API access enabled

## Step 2: Verify Instance API Access

Most Mastodon instances allow public API access for searching hashtags without authentication:

1. Visit your chosen instance in a web browser (e.g., https://mastodon.social)
2. Navigate to the instance's API documentation: `https://[instance-domain]/api/v1/`
3. Check if you can access the API without authentication
4. Look for the hashtag search endpoint: `/api/v1/timelines/tag/:hashtag`

**Note:** Some private or restricted instances may require authentication. For TagzApp, choose instances with public API access.

## Step 3: Configure TagzApp

### Option A: Using the Admin UI (Recommended)

1. Log in to TagzApp as an administrator
2. Navigate to **Admin > Providers**
3. Find the **"Mastodon"** provider
4. Click **"Configure"**
5. Enter your configuration:
   - **Base Address**: The URL of the Mastodon instance (e.g., `https://mastodon.social`)
   - **Enabled**: Toggle to enable the provider
6. Click **"Save Configuration"**
7. The provider will start monitoring hashtags you configure

### Option B: Using Configuration Files

If you prefer to configure via files or environment variables:

**appsettings.json:**
```json
{
  "provider-mastodon": {
    "Enabled": true,
    "BaseAddress": "https://mastodon.social",
    "Timeout": "00:00:30",
    "UseHttp2": true
  }
}
```

**Environment Variables:**
```bash
provider__mastodon__Enabled=true
provider__mastodon__BaseAddress=https://mastodon.social
```

**Azure Key Vault:**
If using Azure Key Vault, store the configuration with the key:
```
TagzApp-provider-mastodon
```

## Step 4: Test Your Configuration

1. In TagzApp, configure a hashtag to monitor (e.g., `#fediverse`)
2. Post a toot with that hashtag on Mastodon (or wait for someone else to)
3. Check if the toot appears in TagzApp (may take 30-60 seconds due to API polling)
4. If posts don't appear, check the logs for errors

## Advanced Configuration

### Monitoring Multiple Instances

To monitor multiple Mastodon instances simultaneously:

1. Run multiple instances of the Mastodon provider with different configurations
2. Configure each with a different base address
3. Each will independently monitor the same hashtags on different instances

**Note:** This requires advanced configuration and may require code changes to support multiple provider instances.

### Custom Timeout Settings

If you experience timeout issues:

```json
{
  "provider-mastodon": {
    "Timeout": "00:01:00"
  }
}
```

### HTTP/2 Configuration

Most modern Mastodon instances support HTTP/2:

```json
{
  "provider-mastodon": {
    "UseHttp2": true
  }
}
```

## Troubleshooting

### Common Issues

#### "Connection Refused" or "Cannot Connect" Error
- **Cause**: Invalid instance URL or instance is down
- **Solution**: 
  - Verify the instance URL is correct (include `https://`)
  - Check if the instance is operational by visiting it in a browser
  - Try a different instance (e.g., mastodon.social)

#### "404 Not Found" Error
- **Cause**: API endpoint not available or instance API structure is different
- **Solution**: 
  - Verify the instance is running standard Mastodon API
  - Some forks (Pleroma, Akkoma) may have slightly different APIs
  - Try using a mainstream Mastodon instance

#### No Toots Appearing
- **Cause**: Various reasons
- **Solution**:
  - Verify the hashtag exists and has recent posts on that instance
  - Check if the provider is enabled in TagzApp
  - Verify the instance allows public API access
  - Check TagzApp logs for error messages
  - Try a more popular instance (e.g., mastodon.social)

#### "429 Too Many Requests" Error
- **Cause**: Exceeded instance rate limits
- **Solution**: 
  - Wait for the rate limit window to reset (typically 5 minutes)
  - Reduce polling frequency in TagzApp
  - Consider using a different instance
  - Contact the instance administrators about rate limits

#### "403 Forbidden" Error
- **Cause**: Instance requires authentication or blocks your IP
- **Solution**:
  - Switch to an instance that allows public access
  - Check if the instance has IP restrictions
  - Contact instance administrators if needed

## Rate Limits

Mastodon instances have their own rate limiting policies:

- **Default Mastodon**: 300 requests per 5 minutes per IP
- **Custom instances**: May have different limits
- **Public endpoints**: Generally more restrictive than authenticated endpoints

TagzApp automatically handles rate limiting and will pause requests when limits are reached.

## Federation Considerations

### What Posts Will You See?

When monitoring a hashtag on a Mastodon instance, you'll see:
- Posts from local users (on that instance)
- Posts from remote users that are federated to that instance
- Posts that the instance is aware of through follows and boosts

You **won't** see:
- Posts from instances that aren't federated with your chosen instance
- Posts from instances that are blocked or defederated
- Posts from very small instances with limited federation

### Best Practices for Hashtag Monitoring

1. **Use larger, well-connected instances** (mastodon.social, fosstodon.org) for broader coverage
2. **Choose topic-specific instances** if your event has a specific theme
3. **Consider monitoring multiple instances** if you need comprehensive coverage
4. **Ask users to tag your instance** in posts for better visibility

## Security Considerations

1. **No credentials required** - The Mastodon provider uses public APIs
2. **Respect instance rules** - Follow the instance's terms of service
3. **Don't abuse rate limits** - Configure reasonable polling intervals
4. **Be a good federation citizen** - Don't overload small instances

## Additional Resources

- [Mastodon Official Documentation](https://docs.joinmastodon.org/)
- [Mastodon API Documentation](https://docs.joinmastodon.org/api/)
- [List of Mastodon Instances](https://joinmastodon.org/servers)
- [Mastodon Apps and Tools](https://joinmastodon.org/apps)
- [Understanding Federation](https://docs.joinmastodon.org/user/network/)

## Need Help?

If you encounter issues not covered here:
1. Check the [TagzApp GitHub Issues](https://github.com/FritzAndFriends/TagzApp/issues)
2. Review the application logs for detailed error messages
3. Consult the Mastodon instance's documentation or support channels
4. Open a new issue on GitHub with details about your problem
