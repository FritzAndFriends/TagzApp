# Bluesky Provider Configuration Guide

This guide will help you configure the Bluesky content provider for TagzApp to monitor hashtags on the Bluesky social network powered by the AT Protocol.

## Overview

The Bluesky provider allows TagzApp to search and display posts containing specific hashtags from the Bluesky social network. Bluesky uses the AT Protocol, a decentralized social networking protocol.

## Prerequisites

- A Bluesky account is **not required** for basic hashtag monitoring
- Bluesky's public API allows reading posts without authentication
- Understanding of Bluesky's decentralized architecture (optional but helpful)

## Understanding Bluesky

Bluesky is built on the AT Protocol (Authenticated Transfer Protocol):
- **Decentralized**: Data is distributed across multiple servers (PDSs - Personal Data Servers)
- **Public by default**: Most posts are publicly accessible
- **No API keys needed**: Public search doesn't require authentication
- **Open protocol**: Anyone can build clients and tools

## Step 1: Understanding Bluesky's Public API

Bluesky's public API allows you to:
- Search for posts by hashtag
- Read public posts without authentication
- Access user profiles and feeds
- Monitor real-time content

**Important:** For basic hashtag monitoring with TagzApp, you don't need any credentials or setup on Bluesky itself!

## Step 2: Configure TagzApp

### Option A: Using the Admin UI (Recommended)

1. Log in to TagzApp as an administrator
2. Navigate to **Admin > Providers**
3. Find the **"Bluesky"** provider
4. Click **"Configure"**
5. Toggle **"Enabled"** to enable the provider
6. Click **"Save Configuration"**
7. The provider will start monitoring hashtags you configure

That's it! No API keys or credentials needed.

### Option B: Using Configuration Files

If you prefer to configure via files or environment variables:

**appsettings.json:**
```json
{
  "providers": {
    "bluesky": {
      "Enabled": true
    }
  }
}
```

**Environment Variables:**
```bash
providers__bluesky__Enabled=true
```

**Azure Key Vault:**
If using Azure Key Vault, store the configuration with the key:
```
TagzApp-providers-bluesky
```

## Step 3: Test Your Configuration

1. In TagzApp, configure a hashtag to monitor (e.g., `#bluesky`)
2. Post on Bluesky with that hashtag, or wait for others to post
3. Check if posts appear in TagzApp (may take 30-60 seconds due to API polling)
4. If posts don't appear, check the logs for errors

## How Hashtags Work in Bluesky

### Hashtag Format

Bluesky supports hashtags similar to other platforms:
- Format: `#hashtag` or `#MultiWordHashtag`
- Case-insensitive: `#TagzApp` matches `#tagzapp`
- Can include numbers: `#dotnet9`
- Unicode supported: `#社交媒体`

### Hashtag Discovery

Posts with hashtags on Bluesky are:
- Indexed automatically
- Searchable across the network
- Visible to all users (if public)
- Federated across Personal Data Servers

## Advanced Topics

### Understanding the AT Protocol

The AT Protocol provides:
- **Decentralized identity**: Users own their identity
- **Repository-based**: Content stored in signed repositories
- **Federated**: Data distributed across multiple servers
- **Interoperable**: Multiple clients can access the same data

### App Passwords (Future Feature)

While not currently required for TagzApp's public monitoring, Bluesky supports app-specific passwords for authenticated operations:
- Used for posting or advanced API features
- Generated in Bluesky Settings > App Passwords
- More secure than using your main password
- Can be revoked individually

**Note:** TagzApp currently only reads public posts and doesn't require authentication.

### Future Authentication Support

If you need authenticated features in the future (like posting or accessing private content):

1. Log in to [Bluesky](https://bsky.app)
2. Go to **Settings > App Passwords**
3. Click **"Add App Password"**
4. Enter a name (e.g., "TagzApp")
5. Copy the generated password
6. Store it securely for future use

## Troubleshooting

### Common Issues

#### "Connection Error" or "Cannot Connect"
- **Cause**: Network issues or Bluesky API unavailable
- **Solution**: 
  - Check your internet connection
  - Verify Bluesky services are operational at [status.bsky.app](https://status.bsky.app)
  - Wait a few minutes and try again

#### No Posts Appearing
- **Cause**: Various reasons
- **Solution**:
  - Verify the hashtag has recent posts on Bluesky
  - Check if the provider is enabled in TagzApp
  - Check TagzApp logs for error messages
  - Ensure posts with the hashtag are public
  - Try a more popular hashtag (e.g., `#bluesky`)

#### "Rate Limited" Messages
- **Cause**: Too many API requests in a short time
- **Solution**: 
  - TagzApp will automatically back off and retry
  - Wait a few minutes for rate limits to reset
  - Consider reducing polling frequency if it happens frequently

#### "Invalid Response" Errors
- **Cause**: API response format changed or unexpected data
- **Solution**:
  - Check if TagzApp is up to date
  - Report the issue on GitHub with log details
  - Bluesky's API may have changed (check for updates)

## Rate Limits

Bluesky has rate limiting on public API access:

- **Public endpoints**: Generous limits for read operations
- **Unauthenticated**: Lower limits than authenticated requests
- **Auto-recovery**: TagzApp handles rate limits automatically

Best practices:
- Don't monitor too many hashtags simultaneously
- Use reasonable polling intervals (30-60 seconds)
- TagzApp respects rate limits and backs off automatically

## Performance Considerations

### Response Times

Bluesky's decentralized architecture means:
- Posts may take a few seconds to be indexed
- Search results are eventually consistent
- Some delays in cross-server federation

### Data Freshness

- New posts may take 30-60 seconds to appear in search
- Very high-volume hashtags may have paged results
- TagzApp polls regularly to catch new posts

## Privacy and Security

### What Data is Accessed?

TagzApp's Bluesky provider only accesses:
- Public posts with matching hashtags
- Public profile information (username, display name)
- Post metadata (timestamp, likes, reposts)

### What is NOT Accessed?

- Private or followers-only posts
- Direct messages
- Account credentials
- Personal information beyond public profiles

### Security Best Practices

1. **No credentials needed** - Public monitoring requires no authentication
2. **Respect user privacy** - Only public posts are monitored
3. **Follow terms of service** - Comply with Bluesky's acceptable use policies
4. **Don't abuse the API** - Use reasonable polling rates

## Bluesky vs Twitter/Mastodon

### Comparison

| Feature | Bluesky | Twitter/X | Mastodon |
|---------|---------|-----------|----------|
| API Keys Required | No (for public) | Yes | No (for public) |
| Decentralized | Yes (AT Protocol) | No | Yes (ActivityPub) |
| Rate Limits | Moderate | Strict (paid tiers) | Per instance |
| Setup Complexity | Very Easy | Moderate | Easy |
| Hashtag Search | Built-in | Built-in | Built-in |

### When to Use Bluesky Provider

- Your audience is active on Bluesky
- You want decentralized social monitoring
- You need easy setup without API credentials
- You're exploring alternative social platforms

## Additional Resources

- [Bluesky Official Site](https://bsky.app)
- [AT Protocol Documentation](https://atproto.com)
- [Bluesky API Docs](https://docs.bsky.app)
- [AT Protocol Specifications](https://atproto.com/specs/atp)
- [Bluesky Status Page](https://status.bsky.app)
- [Bluesky Community](https://bsky.app/profile/bsky.app)

## Future Enhancements

Potential future features for the Bluesky provider:
- Authenticated access for posting
- Following specific accounts
- Custom feed integration
- Reply threading support
- Embed support (images, videos, links)

## Need Help?

If you encounter issues not covered here:
1. Check the [TagzApp GitHub Issues](https://github.com/FritzAndFriends/TagzApp/issues)
2. Review the application logs for detailed error messages
3. Check [Bluesky Status](https://status.bsky.app) for service issues
4. Consult the AT Protocol documentation
5. Open a new issue on GitHub with details about your problem
