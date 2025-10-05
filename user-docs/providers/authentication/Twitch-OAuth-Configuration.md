# Twitch OAuth Configuration Guide

This guide will help you configure Twitch OAuth authentication for TagzApp, allowing users to sign in with their Twitch accounts.

## Overview

The Twitch OAuth provider enables users to authenticate to TagzApp using their Twitch accounts. Perfect for streaming and gaming communities.

## Prerequisites

- A Twitch account
- Access to Twitch Developer Console

## Quick Start

### Step 1: Create a Twitch Application

1. Go to [Twitch Developer Console](https://dev.twitch.tv/console)
2. Sign in with your Twitch account
3. Click **"Register Your Application"**
4. Fill in the details:
   - **Name**: "TagzApp"
   - **OAuth Redirect URLs**: `https://yourdomain.com/signin-twitch`
     - For development: `https://localhost:5001/signin-twitch`
   - **Category**: "Application Integration"
5. Click **"Create"**

### Step 2: Get Your Credentials

1. Click **"Manage"** on your application
2. Copy the **Client ID**
3. Click **"New Secret"** to generate a **Client Secret**
4. **Copy the Client Secret immediately** - you can't see it again!

### Step 3: Configure TagzApp

**Admin UI:**
1. Navigate to **Admin > External Authentication**
2. Find **"Twitch"** provider
3. Enter **Client ID** and **Client Secret**
4. Toggle **Enabled**
5. Click **"Save"**

**Configuration File:**
```json
{
  "Authentication": {
    "Twitch": {
      "ClientID": "your-client-id",
      "ClientSecret": "your-client-secret",
      "Enabled": "true"
    }
  }
}
```

## Testing

1. Log out of TagzApp
2. Click **"Sign in with Twitch"**
3. Authorize the application
4. You should be signed in to TagzApp

## Troubleshooting

### "Redirect URI Mismatch"
- Add the exact redirect URI to your Twitch app: `https://yourdomain.com/signin-twitch`

### "Invalid Client"
- Verify Client ID and Client Secret are correct
- Regenerate secret if needed

### "Application Suspended"
- Check for notifications in Twitch Developer Console
- Review Twitch's developer policies

## Security Notes

- Rotate Client Secrets periodically
- Never commit secrets to source control
- Use separate apps for dev/prod environments

## Additional Resources

- [Twitch Authentication Docs](https://dev.twitch.tv/docs/authentication)
- [Twitch Developer Console](https://dev.twitch.tv/console)
- [OAuth Authorization Flow](https://dev.twitch.tv/docs/authentication/getting-tokens-oauth)

## Need Help?

Check the [TagzApp GitHub Issues](https://github.com/FritzAndFriends/TagzApp/issues) or consult Twitch's authentication documentation.
