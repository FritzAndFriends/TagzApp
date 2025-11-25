# YouTube Chat Provider Configuration Guide

This guide will help you configure the YouTube Chat content provider for TagzApp to monitor live chat messages from YouTube live streams.

## Overview

The YouTube Chat provider allows TagzApp to connect to a YouTube live stream's chat and display messages containing specific hashtags or keywords. This is ideal for live streaming events where you want to showcase viewer interactions from YouTube.

## Prerequisites

- A Google account with YouTube access
- A YouTube channel (required for API access)
- A Google Cloud Platform (GCP) project
- YouTube Data API v3 enabled
- OAuth 2.0 credentials configured

## Step 1: Create a Google Cloud Project

1. Go to the [Google Cloud Console](https://console.cloud.google.com/)
2. Sign in with your Google account
3. Click **"Select a project"** at the top, then **"New Project"**
4. Enter a project name (e.g., "TagzApp YouTube Integration")
5. Click **"Create"**
6. Wait for the project to be created (takes a few seconds)

## Step 2: Enable YouTube Data API v3

1. In the Google Cloud Console, select your project
2. Go to **"APIs & Services"** > **"Library"**
3. Search for **"YouTube Data API v3"**
4. Click on **"YouTube Data API v3"**
5. Click **"Enable"**
6. Wait for the API to be enabled

## Step 3: Create OAuth 2.0 Credentials

### Configure OAuth Consent Screen

1. Go to **"APIs & Services"** > **"OAuth consent screen"**
2. Select **"External"** user type (unless you have Google Workspace)
3. Click **"Create"**
4. Fill in the application information:
   - **App name**: "TagzApp"
   - **User support email**: Your email
   - **Developer contact email**: Your email
5. Click **"Save and Continue"**
6. On the **Scopes** page:
   - Click **"Add or Remove Scopes"**
   - Search for and select: `https://www.googleapis.com/auth/youtube.readonly`
   - Click **"Update"** and **"Save and Continue"**
7. On **Test users** page:
   - Click **"Add Users"**
   - Add the Google account that owns the YouTube channel you want to monitor
   - Click **"Save and Continue"**
8. Review and click **"Back to Dashboard"**

### Create OAuth Client ID

1. Go to **"APIs & Services"** > **"Credentials"**
2. Click **"+ Create Credentials"** > **"OAuth client ID"**
3. Select **"Web application"**
4. Configure the client:
   - **Name**: "TagzApp YouTube Client"
   - **Authorized redirect URIs**: Add your TagzApp URL(s):
     - Development: `https://localhost:5001/signin-google` (adjust port as needed)
     - Production: `https://yourdomain.com/signin-google`
5. Click **"Create"**
6. **Important:** Copy the **Client ID** and **Client Secret** immediately
7. Click **"OK"**

## Step 4: Get an API Key (Alternative Method)

If you prefer to use an API key instead of OAuth (limited functionality):

1. Go to **"APIs & Services"** > **"Credentials"**
2. Click **"+ Create Credentials"** > **"API key"**
3. Copy the generated API key
4. Click **"Close"**
5. (Optional) Click **"Restrict Key"** to add security restrictions

**Note:** API keys have more limitations than OAuth. OAuth is recommended for full functionality.

## Step 5: Configure TagzApp for OAuth

### Using the Admin UI

1. Log in to TagzApp as an administrator
2. Navigate to **Admin > External Authentication**
3. Find **"Google"** provider
4. Configure OAuth credentials:
   - **Client ID**: Your Google OAuth Client ID
   - **Client Secret**: Your Google OAuth Client Secret
   - **Enabled**: Toggle to enable
5. Click **"Save Configuration"**

## Step 6: Authenticate with YouTube

1. In TagzApp, navigate to **Admin > Providers**
2. Find the **"YouTubeChat"** provider
3. Click **"Configure"** or **"Authenticate with Google"**
4. You'll be redirected to Google's login page
5. Sign in with the Google account that has your YouTube channel
6. Grant the requested permissions:
   - View your YouTube account
   - View YouTube live chat messages
7. You'll be redirected back to TagzApp
8. TagzApp will store a refresh token for ongoing access

## Step 7: Configure YouTube Chat Provider

After authenticating, configure the provider:

### Using the Admin UI

1. Navigate to **Admin > Providers** > **YouTubeChat**
2. Configure the settings:
   - **Enabled**: Toggle to enable the provider
   - **Channel Email**: Your YouTube/Google account email (auto-filled)
   - **YouTube API Key**: (Optional) Your API key if not using OAuth
   - **Channel ID**: (Auto-detected from OAuth)
   - **Channel Title**: (Auto-detected from OAuth)
3. Select the broadcast to monitor:
   - TagzApp will show your active live streams
   - Select the stream you want to monitor
4. Click **"Save Configuration"**

### Using Configuration Files

**appsettings.json:**
```json
{
  "Authentication": {
    "Google": {
      "ClientId": "your-client-id.apps.googleusercontent.com",
      "ClientSecret": "your-client-secret"
    }
  },
  "providers": {
    "youtubechat": {
      "Enabled": true,
      "YouTubeApiKey": "your-api-key",
      "ChannelId": "UC...",
      "ChannelTitle": "Your Channel",
      "ChannelEmail": "your-email@gmail.com",
      "BroadcastId": "broadcast-id",
      "LiveChatId": "live-chat-id",
      "RefreshToken": "your-refresh-token"
    }
  }
}
```

**Note:** Most of these values are obtained automatically through OAuth authentication.

## Step 8: Test Your Configuration

1. Start a YouTube live stream on your channel
2. Ensure TagzApp is configured to monitor that stream
3. Send a test message in the YouTube chat with your hashtag
4. The message should appear in TagzApp within a few seconds
5. Check the logs if messages don't appear

## Understanding YouTube Chat Integration

### How It Works

The YouTube Chat provider:
1. Uses OAuth to authenticate with your YouTube account
2. Retrieves your active live streams
3. Connects to the selected stream's live chat
4. Polls for new messages every few seconds
5. Filters messages based on hashtag configuration
6. Displays matching messages in TagzApp

### Message Processing

- **Near real-time**: Messages appear with a few seconds delay (API polling)
- **Filtering**: Only messages with configured hashtags are shown
- **Metadata**: Includes username, timestamp, and author details
- **Emojis**: YouTube emojis and Super Chat are captured

## Troubleshooting

### Common Issues

#### "Authentication Failed" Error
- **Cause**: OAuth credentials are invalid or expired
- **Solution**: 
  - Re-authenticate with Google in the Admin UI
  - Verify Client ID and Client Secret are correct
  - Check that the YouTube Data API is enabled
  - Ensure your Google account has access to the YouTube channel

#### "Quota Exceeded" Error
- **Cause**: YouTube API quota limits reached
- **Solution**: 
  - YouTube API has daily quotas (10,000 units/day for free tier)
  - Each API call costs quota units
  - Wait for quota to reset (midnight Pacific Time)
  - Request quota increase in Google Cloud Console
  - Consider upgrading to a paid plan

#### No Messages Appearing
- **Cause**: Various reasons
- **Solution**:
  - Verify the live stream is active
  - Check if chat is enabled for the stream
  - Ensure the provider is enabled in TagzApp
  - Verify OAuth tokens are valid (re-authenticate if needed)
  - Check TagzApp logs for errors
  - Verify the broadcast ID and chat ID are correct

#### "Invalid Broadcast" Error
- **Cause**: Broadcast ID is wrong or stream ended
- **Solution**: 
  - Reconfigure the provider to select the correct stream
  - Start a new live stream on YouTube
  - Verify the stream has chat enabled

#### "Insufficient Permissions" Error
- **Cause**: OAuth scopes are missing
- **Solution**: 
  - Re-authenticate and ensure you grant all requested permissions
  - Verify the OAuth consent screen includes YouTube read scope
  - Check that the authenticated account owns the YouTube channel

### Rate Limits and Quotas

#### YouTube API Quotas

- **Default quota**: 10,000 units per day (free tier)
- **List messages**: ~5 units per request
- **Get broadcast**: ~1 unit per request
- **Polling frequency**: Affects quota usage

**Quota calculation example:**
- Polling every 5 seconds: 17,280 requests/day
- At 5 units per request: 86,400 units/day
- **Exceeds free quota!** Adjust polling frequency or request more quota.

#### Best Practices for Quota Management

1. **Increase polling interval**: Check every 10-15 seconds instead of 5
2. **Request quota increase**: In Google Cloud Console > Quotas
3. **Monitor usage**: Check quota usage in GCP Console
4. **Optimize filters**: Only poll when stream is active
5. **Consider paid plans**: Higher quotas available with billing enabled

## Advanced Configuration

### Multiple Streams

To monitor multiple YouTube channels:
1. Each channel requires separate OAuth authentication
2. Deploy multiple TagzApp instances or
3. Configure multiple provider instances (requires code changes)

### Super Chat Integration

YouTube Super Chat messages (paid highlights):
- Automatically included in chat messages
- Can be filtered or highlighted separately
- Includes payment amount and styling information

### Moderator Actions

If authenticated as a channel owner/moderator:
- Can potentially remove messages (future feature)
- Can view all messages including held for review

## OAuth Token Management

### Refresh Tokens

- TagzApp stores a refresh token after initial authentication
- Refresh tokens are long-lived (don't expire unless revoked)
- Used to get new access tokens automatically
- Securely stored in TagzApp configuration

### Token Security

1. **Never share OAuth tokens**
2. **Store in secure configuration** (Azure Key Vault, environment variables)
3. **Revoke if compromised**: Google Cloud Console > Credentials
4. **Rotate periodically**: Re-authenticate every few months

### Revoking Access

To revoke TagzApp's access to your YouTube:
1. Go to [Google Account Security](https://myaccount.google.com/permissions)
2. Find "TagzApp" in the list
3. Click **"Remove Access"**
4. Re-authenticate in TagzApp to restore access

## Security Best Practices

1. **Use OAuth instead of API keys** - More secure and full featured
2. **Restrict API keys** - Add HTTP referrer or IP restrictions
3. **Limit OAuth scopes** - Only request necessary permissions
4. **Monitor quota usage** - Watch for unexpected API usage
5. **Secure credentials** - Use Key Vault or secrets management
6. **Enable 2FA** - On the Google account used for authentication

## Additional Resources

- [YouTube Data API Documentation](https://developers.google.com/youtube/v3)
- [YouTube Live Streaming API](https://developers.google.com/youtube/v3/live/docs/)
- [Google OAuth 2.0 Documentation](https://developers.google.com/identity/protocols/oauth2)
- [YouTube API Quotas](https://developers.google.com/youtube/v3/getting-started#quota)
- [Google Cloud Console](https://console.cloud.google.com/)
- [YouTube Chat API Reference](https://developers.google.com/youtube/v3/live/docs/liveChatMessages)

## Cost Considerations

### Free Tier

- 10,000 quota units per day
- Sufficient for light usage
- Monitor quota carefully

### Paid Tier

- Billing must be enabled in GCP
- Higher quotas available
- Pay-as-you-go pricing
- [Check current pricing](https://cloud.google.com/pricing)

### Quota Increase Requests

- Can request quota increases in GCP Console
- Approval usually within 24-48 hours
- May require billing account
- Explain your use case in the request

## Need Help?

If you encounter issues not covered here:
1. Check the [TagzApp GitHub Issues](https://github.com/FritzAndFriends/TagzApp/issues)
2. Review the application logs for detailed error messages
3. Check [YouTube API Status](https://www.google.com/appsstatus)
4. Consult Google's YouTube API documentation
5. Open a new issue on GitHub with details about your problem
