# LinkedIn OAuth Configuration Guide

This guide will help you configure LinkedIn OAuth authentication for TagzApp, allowing users to sign in with their LinkedIn accounts.

## Overview

The LinkedIn OAuth provider enables users to authenticate to TagzApp using their LinkedIn accounts. Ideal for professional networking events and corporate use cases.

## Prerequisites

- A LinkedIn account
- Access to LinkedIn Developer Portal

## Quick Start

### Step 1: Create a LinkedIn App

1. Go to [LinkedIn Developers](https://www.linkedin.com/developers/)
2. Sign in with your LinkedIn account
3. Click **"Create app"**
4. Fill in the application details:
   - **App name**: "TagzApp"
   - **LinkedIn Page**: Select or create a LinkedIn page (required)
   - **Privacy policy URL**: Your privacy policy URL
   - **App logo**: Upload a logo (optional)
5. Check the legal agreement
6. Click **"Create app"**

### Step 2: Configure OAuth Settings

1. In your app, go to the **"Auth"** tab
2. Under **"OAuth 2.0 settings"**:
   - **Authorized redirect URLs**: Add `https://yourdomain.com/signin-linkedin`
     - For development: `https://localhost:5001/signin-linkedin`
3. Click **"Update"**

### Step 3: Get Your Credentials

1. In the **"Auth"** tab, find:
   - **Client ID** - Copy this
   - **Client Secret** - Click "Show" and copy
2. Store both securely

### Step 4: Request OAuth Scopes

1. Go to the **"Products"** tab
2. Request access to **"Sign In with LinkedIn using OpenID Connect"**
3. Wait for approval (usually immediate for Sign In product)

### Step 5: Configure TagzApp

**Admin UI:**
1. Navigate to **Admin > External Authentication**
2. Find **"LinkedIn"** provider
3. Enter **Client ID** and **Client Secret**
4. Toggle **Enabled**
5. Click **"Save"**

**Configuration File:**
```json
{
  "Authentication": {
    "LinkedIn": {
      "ClientID": "your-client-id",
      "ClientSecret": "your-client-secret",
      "Enabled": "true"
    }
  }
}
```

## Testing

1. Log out of TagzApp
2. Click **"Sign in with LinkedIn"**
3. Authorize the application
4. You should be signed in to TagzApp

## Troubleshooting

### "Redirect URI Mismatch"
- Verify the redirect URL in LinkedIn app settings matches: `https://yourdomain.com/signin-linkedin`

### "Invalid Redirect URI"
- LinkedIn requires HTTPS (except for localhost)
- Ensure no trailing slashes in the redirect URI

### "Product Not Approved"
- Request "Sign In with LinkedIn" product access
- Wait for approval (usually instant)

### "Invalid Client"
- Verify Client ID and Client Secret
- Ensure app is not in development restrictions

## LinkedIn Page Requirement

LinkedIn requires you to associate your app with a LinkedIn Page:
- Can be a personal or company page
- Used for verification and user trust
- Page doesn't need to be published or active

## Data Access

With Sign In with LinkedIn, you can access:
- Name
- Email address
- Profile picture
- Basic profile information

## Additional Resources

- [LinkedIn OAuth Documentation](https://docs.microsoft.com/linkedin/shared/authentication/authentication)
- [LinkedIn Developer Portal](https://www.linkedin.com/developers/)
- [Sign In with LinkedIn](https://docs.microsoft.com/linkedin/consumer/integrations/self-serve/sign-in-with-linkedin)

## Need Help?

Check the [TagzApp GitHub Issues](https://github.com/FritzAndFriends/TagzApp/issues) or consult LinkedIn's OAuth documentation.
