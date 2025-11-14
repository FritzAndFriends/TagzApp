# Google OAuth Configuration Guide

This guide will help you configure Google OAuth authentication for TagzApp, allowing users to sign in with their Google accounts.

## Overview

The Google OAuth provider enables users to authenticate to TagzApp using their Google accounts (Gmail, Google Workspace, etc.).

## Prerequisites

- A Google account
- Access to Google Cloud Console

## Quick Start

### Step 1: Create a Google Cloud Project

1. Go to [Google Cloud Console](https://console.cloud.google.com/)
2. Click **"Select a project"** > **"New Project"**
3. Enter project name (e.g., "TagzApp Authentication")
4. Click **"Create"**

### Step 2: Configure OAuth Consent Screen

1. Navigate to **"APIs & Services"** > **"OAuth consent screen"**
2. Select **"External"** user type
3. Click **"Create"**
4. Fill in required information:
   - **App name**: "TagzApp"
   - **User support email**: Your email
   - **Developer contact email**: Your email
5. Click **"Save and Continue"** through remaining screens

### Step 3: Create OAuth Credentials

1. Go to **"APIs & Services"** > **"Credentials"**
2. Click **"+ Create Credentials"** > **"OAuth client ID"**
3. Select **"Web application"**
4. Configure:
   - **Name**: "TagzApp Web Client"
   - **Authorized redirect URIs**: 
     - Add: `https://yourdomain.com/signin-google`
     - Development: `https://localhost:5001/signin-google`
5. Click **"Create"**
6. **Copy the Client ID and Client Secret immediately!**

### Step 4: Configure TagzApp

**Admin UI:**
1. Navigate to **Admin > External Authentication**
2. Find **"Google"** provider
3. Enter **Client ID** and **Client Secret**
4. Toggle **Enabled**
5. Click **"Save"**

**Configuration File:**
```json
{
  "Authentication": {
    "Google": {
      "ClientID": "your-client-id.apps.googleusercontent.com",
      "ClientSecret": "your-client-secret",
      "Enabled": "true"
    }
  }
}
```

## Testing

1. Log out of TagzApp
2. Click **"Sign in with Google"**
3. Select your Google account
4. Grant permissions
5. You should be signed in to TagzApp

## Troubleshooting

### "Redirect URI Mismatch"
- Verify the redirect URI in Google Cloud Console matches exactly: `https://yourdomain.com/signin-google`

### "Invalid Client"
- Regenerate credentials in Google Cloud Console
- Update TagzApp configuration

### "Access Denied"
- Check OAuth consent screen is configured
- Verify user is added as test user (if app is in testing mode)

## Publishing Your App (Optional)

For production use with any Google account:

1. In OAuth consent screen, click **"Publish App"**
2. Submit for verification if prompted
3. Or keep in testing mode and add users manually

## Additional Resources

- [Google OAuth Documentation](https://developers.google.com/identity/protocols/oauth2)
- [Google Cloud Console](https://console.cloud.google.com/)
- [OAuth Consent Screen Guide](https://support.google.com/cloud/answer/10311615)

## Need Help?

Check the [TagzApp GitHub Issues](https://github.com/FritzAndFriends/TagzApp/issues) or consult Google's OAuth documentation.
