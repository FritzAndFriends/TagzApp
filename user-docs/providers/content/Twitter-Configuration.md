# Twitter/X Provider Configuration Guide

This guide will help you configure the Twitter/X (formerly Twitter) content provider for TagzApp to monitor hashtags and display tweets in real-time.

## Overview

The Twitter/X provider allows TagzApp to search and display tweets containing specific hashtags. You'll need a Twitter/X Developer account and API credentials to use this provider.

## Prerequisites

- A Twitter/X account
- Access to the Twitter/X Developer Portal
- A Twitter/X app with appropriate API access level

## Step 1: Create a Twitter/X Developer Account

1. Go to the [Twitter Developer Portal](https://developer.twitter.com/en/portal/dashboard)
2. Sign in with your Twitter/X account
3. If you don't have a developer account yet, click **"Sign up"** or **"Apply for a developer account"**
4. Fill out the application form:
   - Select your use case (typically "Building tools for Twitter users")
   - Provide a description of how you'll use the API
   - Accept the Terms of Service
5. Wait for approval (this can take a few minutes to a few hours)

## Step 2: Create a Twitter/X App

1. Once approved, go to the [Developer Portal Dashboard](https://developer.twitter.com/en/portal/dashboard)
2. Click **"+ Create Project"** (if you don't have one already)
   - Enter a project name (e.g., "TagzApp")
   - Select a use case (e.g., "Explore the API")
   - Provide a project description
3. Click **"+ Add App"** or **"Create App"**
   - Enter an app name (e.g., "TagzApp Monitor")
   - The app will be created and you'll see the API keys

## Step 3: Get Your API Credentials

After creating your app, you'll need to obtain a Bearer Token:

1. In your app's dashboard, go to the **"Keys and tokens"** tab
2. Under **"Authentication Tokens"**, find **"Bearer Token"**
3. Click **"Generate"** if you haven't already
4. **Important:** Copy the Bearer Token immediately and store it securely. You won't be able to see it again!
5. If you lose the token, you can regenerate it (but the old token will stop working)

### API Access Levels

Twitter/X offers different API access levels:
- **Free tier**: Limited to 500,000 tweets/month
- **Basic tier**: $100/month, includes more features
- **Pro tier**: $5,000/month, includes advanced features
- **Enterprise**: Custom pricing

For basic hashtag monitoring with TagzApp, the **Free tier** is usually sufficient for small to medium events.

## Step 4: Configure TagzApp

### Option A: Using the Admin UI (Recommended)

1. Log in to TagzApp as an administrator
2. Navigate to **Admin > Providers**
3. Find the **"X (formerly Twitter)"** provider
4. Click **"Configure"**
5. Enter your credentials:
   - **Bearer Token**: Paste the Bearer Token you copied earlier
   - **Enabled**: Toggle to enable the provider
6. Click **"Save Configuration"**
7. The provider will start monitoring hashtags you configure

### Option B: Using Configuration Files

If you prefer to configure via files or environment variables:

**appsettings.json:**
```json
{
  "providers": {
    "twitter": {
      "Enabled": true,
      "BearerToken": "your-bearer-token-here",
      "BaseAddress": "https://api.twitter.com",
      "Timeout": "00:00:30"
    }
  }
}
```

**Environment Variables:**
```bash
providers__twitter__Enabled=true
providers__twitter__BearerToken=your-bearer-token-here
```

**Azure Key Vault:**
If using Azure Key Vault, store the configuration with the key:
```
TagzApp-providers-twitter
```

## Step 5: Test Your Configuration

1. In TagzApp, configure a hashtag to monitor (e.g., `#test`)
2. Post a tweet with that hashtag on Twitter/X
3. Check if the tweet appears in TagzApp (may take 30-60 seconds due to API polling)
4. If tweets don't appear, check the logs for errors

## Troubleshooting

### Common Issues

#### "401 Unauthorized" Error
- **Cause**: Invalid or expired Bearer Token
- **Solution**: 
  - Verify you copied the entire token correctly
  - Regenerate the token in the Developer Portal
  - Update the configuration with the new token

#### "429 Too Many Requests" Error
- **Cause**: Exceeded API rate limits
- **Solution**: 
  - Wait for the rate limit window to reset (typically 15 minutes)
  - Consider upgrading your Twitter/X API access tier
  - Reduce the frequency of hashtag polling in TagzApp

#### No Tweets Appearing
- **Cause**: Various reasons
- **Solution**:
  - Verify the hashtag exists and has recent tweets
  - Check if the provider is enabled in TagzApp
  - Verify your Bearer Token is correct
  - Check TagzApp logs for error messages
  - Ensure your Twitter/X app has appropriate permissions

#### "403 Forbidden" Error
- **Cause**: API access level doesn't support the feature
- **Solution**:
  - Verify your Twitter/X account has the correct API access level
  - Some features require paid API tiers
  - Check the Twitter/X Developer Portal for your access level

## Rate Limits

Twitter/X imposes rate limits on API requests:

- **Free tier**: 
  - 500,000 tweets/month
  - Search endpoint: ~180 requests per 15 minutes
- **Basic tier**: 
  - More generous limits, check Twitter/X documentation
- **Higher tiers**: 
  - Increased or unlimited access depending on plan

TagzApp automatically handles rate limiting and will pause requests when limits are reached.

## Security Best Practices

1. **Never commit API credentials to source control**
2. **Use environment variables or secure vaults** (Azure Key Vault, AWS Secrets Manager, etc.)
3. **Rotate credentials periodically**
4. **Regenerate tokens if compromised**
5. **Use the minimum required API access level**
6. **Monitor API usage** in the Developer Portal

## Additional Resources

- [Twitter/X API Documentation](https://developer.twitter.com/en/docs/twitter-api)
- [Twitter/X Developer Portal](https://developer.twitter.com/en/portal/dashboard)
- [API Access Levels and Pricing](https://developer.twitter.com/en/products/twitter-api)
- [Rate Limit Documentation](https://developer.twitter.com/en/docs/twitter-api/rate-limits)
- [Twitter/X API Support](https://developer.twitter.com/en/support/twitter-api)

## Need Help?

If you encounter issues not covered here:
1. Check the [TagzApp GitHub Issues](https://github.com/FritzAndFriends/TagzApp/issues)
2. Review the application logs for detailed error messages
3. Consult the Twitter/X API documentation
4. Open a new issue on GitHub with details about your problem
