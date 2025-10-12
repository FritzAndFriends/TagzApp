# Blazot Provider Configuration Guide

This guide will help you configure the Blazot content provider for TagzApp to monitor posts from the Blazot social network.

## Overview

The Blazot provider allows TagzApp to search and display posts containing specific hashtags from Blazot (https://blazot.com), a social networking platform. Blazot is designed for developers and technology enthusiasts.

## Prerequisites

- A Blazot user account
- API Key and Secret Auth Key from Blazot
- Understanding of Blazot's account types (User vs Site)

## Understanding Blazot Account Types

Blazot has two types of accounts:

### User Accounts
- Personal accounts for individuals
- Can create posts and interact with content
- API keys are available in user settings
- **Create this first!**

### Site Accounts
- Organizational accounts (like company pages)
- Created after logging in with your user account
- Have separate API keys
- Can represent brands, projects, or organizations

**⚠️ Important:** Create a **user account** first, not a Site account. If you want to represent a company, create your personal user account first, then create a Site within that account. If you use the desired company username as a user, it won't be available as a Site later.

## Step 1: Create a Blazot Account

1. Go to [https://blazot.com](https://blazot.com)
2. Click **"Sign Up"** or **"Register"**
3. Fill in your personal information:
   - Username (your personal username, not your company name)
   - Email address
   - Password
4. Verify your email address if required
5. Complete your profile setup

## Step 2: Create a Site (Optional)

If you want to represent an organization or company:

1. Log in to Blazot with your personal user account
2. Click on your profile menu (top right)
3. Select **"Create Site"** or navigate to Site management
4. Fill in the site information:
   - Site name/username (e.g., your company name)
   - Description
   - Site type/category
5. Complete the site setup
6. You can now manage the site from your user account

## Step 3: Get Your API Credentials

### For User Account

1. Log in to [https://blazot.com](https://blazot.com)
2. Click on your profile picture or menu (top right)
3. Select **"Configuration"** or **"Settings"**
4. Navigate to the **"API"** or **"Developer"** section
5. You'll see:
   - **API Key**: Your public API key
   - **Secret Auth Key**: Your private authentication key
6. Copy both keys and store them securely

### For Site Account

1. Log in to Blazot with your user account
2. Switch to your Site context (if you have multiple)
3. Navigate to the Site's **"Configuration"** page
4. Go to the **"API"** section
5. Copy the **API Key** and **Secret Auth Key** for the Site
6. Store them securely

**⚠️ Security Note:** The Secret Auth Key is sensitive. Never share it or commit it to source control. It's used only to obtain access tokens.

## Step 4: Understand Blazot API Access

### API Key Types

- **API Key**: Public identifier for your application
- **Secret Auth Key**: Private key used to authenticate and get access tokens
- **Access Token**: Temporary token used for API requests (managed by TagzApp)

### How Authentication Works

1. TagzApp uses your API Key and Secret Auth Key
2. Requests an access token from Blazot
3. Uses the access token for all API requests
4. Automatically renews the token when it expires

**You only need to provide the API Key and Secret Auth Key** - TagzApp handles token management.

## Step 5: Configure TagzApp

### Option A: Using the Admin UI (Recommended)

1. Log in to TagzApp as an administrator
2. Navigate to **Admin > Providers**
3. Find the **"Blazot"** provider
4. Click **"Configure"**
5. Enter your credentials:
   - **API Key**: Your Blazot API Key
   - **Secret Auth Key**: Your Blazot Secret Auth Key
   - **Window Seconds**: Rate limit window duration (default: 300 seconds / 5 minutes)
   - **Window Requests**: Number of requests allowed per window (default: 5)
   - **Enabled**: Toggle to enable the provider
6. Click **"Save Configuration"**
7. The provider will authenticate and start monitoring

### Option B: Using Configuration Files

If you prefer to configure via files or environment variables:

**appsettings.json:**
```json
{
  "Blazot": {
    "Enabled": true,
    "ApiKey": "your-api-key",
    "SecretAuthKey": "your-secret-auth-key",
    "BaseAddress": "https://blazot.com",
    "WindowSeconds": 300,
    "WindowRequests": 5,
    "Timeout": "00:00:30",
    "UseHttp2": true
  }
}
```

**Environment Variables:**
```bash
Blazot__Enabled=true
Blazot__ApiKey=your-api-key
Blazot__SecretAuthKey=your-secret-auth-key
Blazot__WindowSeconds=300
Blazot__WindowRequests=5
```

**Azure Key Vault:**
If using Azure Key Vault, store the configuration with the key:
```
TagzApp-Blazot
```

## Step 6: Test Your Configuration

1. In TagzApp, configure a hashtag to monitor (e.g., `#blazot`)
2. Post on Blazot with that hashtag
3. Check if the post appears in TagzApp (may take 30-60 seconds)
4. If posts don't appear, check the logs for errors

## Rate Limits and Quotas

### Default Rate Limits

- **Free users**: 5 requests per 15-minute window
- **With subscription**: Higher limits (check Blazot subscription details)
- **Rate limits will increase** as the platform grows

### Understanding Rate Limits

TagzApp automatically manages rate limits:
- Tracks request counts per time window
- Pauses when limits are reached
- Resumes after the window resets
- Logs rate limit events

**For most use cases, 5 requests per 15 minutes is sufficient** since TagzApp caches results and doesn't need to poll frequently.

### Optimizing for Rate Limits

1. **Increase polling interval**: Check every 3+ minutes instead of every minute
2. **Monitor fewer hashtags**: Focus on key hashtags
3. **Consider subscription**: If you need higher limits
4. **Use caching**: TagzApp caches results to reduce API calls

## Regenerating API Keys

If your keys are compromised or you need to rotate them:

1. Log in to Blazot
2. Go to Configuration > API
3. Click **"Regenerate Keys"** or **"Reset Keys"**
4. Copy the new API Key and Secret Auth Key
5. Update your TagzApp configuration immediately
6. Old keys will stop working

## Troubleshooting

### Common Issues

#### "Authentication Failed" Error
- **Cause**: Invalid API Key or Secret Auth Key
- **Solution**: 
  - Verify you copied the keys correctly
  - Ensure there are no extra spaces or characters
  - Regenerate keys in Blazot if needed
  - Update TagzApp configuration

#### "Rate Limited" Error
- **Cause**: Exceeded your rate limit quota
- **Solution**: 
  - Wait for the rate limit window to reset (15 minutes)
  - Reduce polling frequency in TagzApp
  - Consider upgrading your Blazot account
  - Check if multiple TagzApp instances are using the same keys

#### No Posts Appearing
- **Cause**: Various reasons
- **Solution**:
  - Verify the hashtag has recent posts on Blazot
  - Check if the provider is enabled in TagzApp
  - Verify API credentials are correct
  - Check TagzApp logs for errors
  - Ensure you're not rate limited

#### "Invalid Response" Error
- **Cause**: API response format unexpected
- **Solution**: 
  - Check if TagzApp is up to date
  - Blazot API may have changed (check for updates)
  - Report the issue on GitHub with log details

#### "Connection Refused" Error
- **Cause**: Cannot connect to Blazot servers
- **Solution**: 
  - Check your internet connection
  - Verify Blazot is operational (visit blazot.com)
  - Check firewall settings
  - Verify the BaseAddress is correct

## Advanced Configuration

### Custom Base Address

If Blazot changes domains or you're using a development instance:

```json
{
  "Blazot": {
    "BaseAddress": "https://api.blazot.com"
  }
}
```

### Timeout Configuration

For slower connections or rate limiting:

```json
{
  "Blazot": {
    "Timeout": "00:01:00"
  }
}
```

### HTTP/2 Support

Blazot supports HTTP/2 for better performance:

```json
{
  "Blazot": {
    "UseHttp2": true
  }
}
```

## Security Best Practices

1. **Never commit API keys to source control**
2. **Use environment variables or Key Vault** for production
3. **Rotate keys periodically** (every 3-6 months)
4. **Regenerate keys if compromised** immediately
5. **Monitor API usage** for unusual activity
6. **Use separate keys for development and production**
7. **Limit key access** to only necessary team members

## Blazot API Features

### Current Support

- Hashtag search
- Post retrieval
- User information
- Real-time monitoring

### Future Enhancements

Potential future features:
- Post creation from TagzApp
- User mentions monitoring
- Direct message integration
- Analytics and insights

## Comparing Blazot to Other Providers

| Feature | Blazot | Twitter/X | Mastodon |
|---------|--------|-----------|----------|
| Setup Complexity | Easy | Moderate | Easy |
| API Keys Required | Yes | Yes | No |
| Rate Limits | 5/15min (free) | Strict | Per instance |
| Cost | Free (subscriptions available) | Free/Paid tiers | Free |
| Target Audience | Developers/Tech | General | General |

### When to Use Blazot Provider

- Your audience is on Blazot
- Tech/developer focused events
- Want to support developer-friendly platforms
- Need straightforward API setup

## Additional Resources

- [Blazot Official Site](https://blazot.com)
- [Blazot API Documentation](https://developers.blazot.com/docs/api/v1)
- [Blazot Support](https://blazot.com/support)
- [TagzApp Blazot Provider README](https://github.com/FritzAndFriends/TagzApp/blob/main/src/TagzApp.Providers.Blazot/ReadMe.md)

## Community and Support

- **Blazot Community**: Join discussions on Blazot itself
- **Developer Forum**: Check Blazot's developer resources
- **TagzApp Issues**: Report integration issues on GitHub

## Need Help?

If you encounter issues not covered here:
1. Check the [TagzApp GitHub Issues](https://github.com/FritzAndFriends/TagzApp/issues)
2. Review the application logs for detailed error messages
3. Consult [Blazot API Documentation](https://developers.blazot.com/docs/api/v1)
4. Check Blazot's status and announcements
5. Open a new issue on GitHub with details about your problem
