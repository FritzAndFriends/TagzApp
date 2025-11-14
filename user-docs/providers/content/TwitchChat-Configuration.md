# Twitch Chat Provider Configuration Guide

This guide will help you configure the Twitch Chat content provider for TagzApp to monitor chat messages from Twitch streams in real-time.

## Overview

The Twitch Chat provider allows TagzApp to connect to a Twitch channel's chat and display messages containing specific hashtags or keywords. This is perfect for live streaming events where you want to display viewer interactions.

## Prerequisites

- A Twitch account (can be a separate bot account)
- A Twitch application registered in the Twitch Developer Console
- The Twitch channel name you want to monitor
- Basic understanding of Twitch chat and IRC

## Step 1: Create a Twitch Developer Application

1. Go to the [Twitch Developer Console](https://dev.twitch.tv/console)
2. Sign in with your Twitch account
3. Click **"Register Your Application"** or **"Applications"** > **"Register"**
4. Fill out the application form:
   - **Name**: Enter a name (e.g., "TagzApp Bot")
   - **OAuth Redirect URLs**: Add `https://localhost` (or your TagzApp domain)
   - **Category**: Select "Application Integration" or "Chat Bot"
5. Complete the CAPTCHA and click **"Create"**

## Step 2: Get Your Client ID and Client Secret

After creating your application:

1. Click **"Manage"** on your application in the Developer Console
2. You'll see your **Client ID** - copy this
3. Click **"New Secret"** to generate a **Client Secret**
4. **Important:** Copy the Client Secret immediately! You won't be able to see it again
5. Store both values securely

## Step 3: Create a Chat Bot Account (Recommended)

For best practices, create a separate Twitch account for your bot:

1. Create a new Twitch account (e.g., "YourEventBot")
2. Set up a profile picture and description
3. This account will appear in chat as the bot monitoring messages

**Why use a bot account?**
- Keeps your personal account separate
- Makes it clear a bot is monitoring chat
- Easier to manage permissions and OAuth tokens

## Step 4: Generate an OAuth Token

You need an OAuth token for the bot account to access chat:

### Method A: Using Twitch Token Generator (Easiest)

1. Go to [Twitch Token Generator](https://twitchtokengenerator.com/)
2. Log in with your **bot account** (not your main account)
3. Select scopes:
   - `chat:read` - Read chat messages
   - `chat:edit` - Send chat messages (optional)
4. Click **"Generate Token"**
5. Copy the **OAuth Token** (starts with `oauth:`)
6. Store it securely

### Method B: Using Twitch CLI

1. Install the [Twitch CLI](https://dev.twitch.tv/docs/cli)
2. Run: `twitch token`
3. Follow the authentication flow
4. Copy the OAuth token

### Method C: Manual OAuth Flow

1. Use this URL (replace `YOUR_CLIENT_ID` and `YOUR_REDIRECT_URI`):
```
https://id.twitch.tv/oauth2/authorize?client_id=YOUR_CLIENT_ID&redirect_uri=YOUR_REDIRECT_URI&response_type=code&scope=chat:read%20chat:edit
```
2. Authorize the application
3. Exchange the authorization code for an access token using the Twitch API
4. Prepend `oauth:` to the token

## Step 5: Configure TagzApp

### Option A: Using the Admin UI (Recommended)

1. Log in to TagzApp as an administrator
2. Navigate to **Admin > Providers**
3. Find the **"TwitchChat"** provider
4. Click **"Configure"**
5. Enter your credentials:
   - **Client ID**: Your Twitch application Client ID
   - **Client Secret**: Your Twitch application Client Secret
   - **Chat Bot Name**: Your bot account's Twitch username (e.g., "youreventbot")
   - **OAuth Token**: The OAuth token for your bot account (include `oauth:` prefix)
   - **Channel Name**: The Twitch channel to monitor (without the `#`, e.g., "csharpfritz")
   - **Enabled**: Toggle to enable the provider
6. Click **"Save Configuration"**
7. The provider will connect to Twitch chat and start monitoring

### Option B: Using Configuration Files

If you prefer to configure via files or environment variables:

**appsettings.json:**
```json
{
  "provider-twitch": {
    "Enabled": true,
    "ClientId": "your-client-id",
    "ClientSecret": "your-client-secret",
    "ChatBotName": "youreventbot",
    "OAuthToken": "oauth:your-oauth-token",
    "ChannelName": "csharpfritz"
  }
}
```

**Environment Variables:**
```bash
provider__twitch__Enabled=true
provider__twitch__ClientId=your-client-id
provider__twitch__ClientSecret=your-client-secret
provider__twitch__ChatBotName=youreventbot
provider__twitch__OAuthToken=oauth:your-oauth-token
provider__twitch__ChannelName=csharpfritz
```

**Azure Key Vault:**
If using Azure Key Vault, store the configuration with the key:
```
TagzApp-provider-twitch
```

## Step 6: Test Your Configuration

1. Start TagzApp and verify the Twitch provider connects successfully
2. Go to the Twitch channel you're monitoring
3. Send a test message in chat with your configured hashtag
4. The message should appear in TagzApp within a few seconds
5. Check the logs if messages don't appear

## Understanding Twitch Chat Integration

### How It Works

The Twitch Chat provider:
1. Connects to Twitch IRC servers
2. Joins the specified channel's chat
3. Listens for all messages in real-time
4. Filters messages based on your hashtag configuration
5. Displays matching messages in TagzApp

### Message Processing

- **Real-time**: Messages appear instantly (no polling delay)
- **Filtering**: Only messages with configured hashtags/keywords are shown
- **Metadata**: Includes username, timestamp, and badges
- **Emotes**: Twitch emotes are captured but displayed as text codes

## Troubleshooting

### Common Issues

#### "Authentication Failed" Error
- **Cause**: Invalid OAuth token or wrong bot account
- **Solution**: 
  - Regenerate the OAuth token
  - Ensure the token is for the correct bot account
  - Verify the token includes `oauth:` prefix
  - Check that the token has necessary scopes

#### "Unable to Connect" Error
- **Cause**: Network issues or invalid credentials
- **Solution**: 
  - Verify your internet connection
  - Check that Twitch IRC is operational
  - Verify all credentials are correct
  - Check firewall settings (allow IRC connections)

#### No Messages Appearing
- **Cause**: Various reasons
- **Solution**:
  - Verify the channel name is correct (no `#` prefix)
  - Check if the provider is enabled
  - Ensure hashtag configuration matches messages
  - Verify the bot has joined the channel (check Twitch chat user list)
  - Check TagzApp logs for errors

#### "Rate Limited" or "Message Throttled"
- **Cause**: Sending too many messages too quickly
- **Solution**: 
  - This shouldn't affect reading messages
  - If posting replies, slow down the rate
  - Twitch limits: 20 messages per 30 seconds for non-moderators

#### Bot Not Appearing in Chat
- **Cause**: Connection issue or authentication problem
- **Solution**:
  - Check the OAuth token is valid
  - Verify the bot account is not banned from the channel
  - Restart TagzApp to reconnect
  - Check if the channel exists and is live

#### "Invalid OAuth Token" Error
- **Cause**: Token expired or revoked
- **Solution**: 
  - OAuth tokens don't expire unless revoked
  - Regenerate the token in the Developer Console
  - Update the configuration with the new token

## Advanced Configuration

### Monitoring Multiple Channels

To monitor multiple Twitch channels:
1. Deploy multiple TagzApp instances, each configured for a different channel
2. Or modify the configuration to support multiple channels (requires code changes)

### Bot Commands (Future Feature)

The Twitch provider can potentially support:
- Responding to commands in chat
- Posting messages to chat
- Moderator actions

### Custom Badge Support

Twitch chat includes badges (subscriber, moderator, VIP, etc.):
- TagzApp captures badge information
- Can be used for filtering or display customization
- Check the Twitch API documentation for badge details

## Rate Limits and Performance

### Twitch IRC Limits

- **Read messages**: No limit (can read all messages)
- **Send messages**: 
  - Non-moderators: 20 messages per 30 seconds
  - Moderators: 100 messages per 30 seconds
- **Join channels**: Up to 50 channels per connection

### Performance Tips

- Single channel monitoring has no performance issues
- Real-time connection is more efficient than polling
- Messages are processed as they arrive
- No delay compared to other providers

## Security Best Practices

1. **Use a dedicated bot account** - Don't use your personal account
2. **Secure OAuth tokens** - Never commit tokens to source control
3. **Rotate credentials** - Regenerate tokens periodically
4. **Limit scopes** - Only request necessary permissions
5. **Monitor usage** - Watch for unusual activity
6. **Revoke tokens** - Revoke tokens if compromised

## Integration with Twitch Features

### Channel Points

TagzApp can potentially integrate with:
- Channel point redemptions
- Custom rewards triggering messages

### Subscriber-Only Chat

- Works with any chat mode (sub-only, followers-only, etc.)
- Bot must meet the requirements to read messages

### Emotes

- Twitch emotes are captured
- Displayed as text codes (e.g., `:Kappa:`)
- Future enhancement: Render emote images

## Additional Resources

- [Twitch Developer Documentation](https://dev.twitch.tv/docs/)
- [Twitch IRC Guide](https://dev.twitch.tv/docs/irc/)
- [Twitch API Reference](https://dev.twitch.tv/docs/api/)
- [Twitch Authentication Guide](https://dev.twitch.tv/docs/authentication/)
- [Twitch Chat Badges](https://dev.twitch.tv/docs/irc/tags/#privmsg-tags)
- [Twitch Token Generator](https://twitchtokengenerator.com/)

## Need Help?

If you encounter issues not covered here:
1. Check the [TagzApp GitHub Issues](https://github.com/FritzAndFriends/TagzApp/issues)
2. Review the application logs for detailed error messages
3. Consult the Twitch Developer documentation
4. Test your credentials with other Twitch IRC tools
5. Open a new issue on GitHub with details about your problem
