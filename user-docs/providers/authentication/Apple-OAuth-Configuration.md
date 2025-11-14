# Apple Sign In Configuration Guide

This guide will help you configure Sign in with Apple authentication for TagzApp, allowing users to sign in with their Apple IDs.

## Overview

Sign in with Apple enables users to authenticate to TagzApp using their Apple ID accounts. Apple requires this for iOS apps but can also be used for web applications.

## Prerequisites

- An Apple Developer account ($99/year for full access)
- Access to Apple Developer Portal
- For web: Ability to verify your domain

## Quick Start

### Step 1: Create an App ID

1. Go to [Apple Developer Portal](https://developer.apple.com/account)
2. Sign in with your Apple Developer account
3. Navigate to **"Certificates, Identifiers & Profiles"**
4. Click **"Identifiers"** > **"+"** to create new
5. Select **"App IDs"** and click **"Continue"**
6. Select **"App"** and click **"Continue"**
7. Configure:
   - **Description**: "TagzApp"
   - **Bundle ID**: Choose "Explicit" and enter (e.g., `com.yourdomain.tagzapp`)
   - **Capabilities**: Check **"Sign in with Apple"**
8. Click **"Continue"** and **"Register"**

### Step 2: Create a Services ID

1. In **"Identifiers"**, click **"+"** again
2. Select **"Services IDs"** and click **"Continue"**
3. Configure:
   - **Description**: "TagzApp Web"
   - **Identifier**: (e.g., `com.yourdomain.tagzapp.web`)
   - Check **"Sign in with Apple"**
4. Click **"Continue"** and **"Register"**

### Step 3: Configure Sign in with Apple

1. Select your Services ID from the list
2. Check **"Sign in with Apple"**
3. Click **"Configure"**
4. Configure settings:
   - **Primary App ID**: Select the App ID you created
   - **Domains**: Add `yourdomain.com`
   - **Return URLs**: Add `https://yourdomain.com/signin-apple`
5. Click **"Save"** and **"Continue"**
6. Click **"Register"**

### Step 4: Create a Key for Sign in with Apple

1. Navigate to **"Keys"** > **"+"**
2. Configure:
   - **Key Name**: "TagzApp Sign in with Apple Key"
   - Check **"Sign in with Apple"**
   - Click **"Configure"**
   - Select your Primary App ID
3. Click **"Continue"** and **"Register"**
4. **Download the key file (.p8)** - you can't download it again!
5. Note the **Key ID** shown on the confirmation page

### Step 5: Get Your Credentials

You'll need:
- **Services ID (Client ID)**: The identifier you created (e.g., `com.yourdomain.tagzapp.web`)
- **Team ID**: Found in the top right of Apple Developer Portal
- **Key ID**: From the key you created
- **Private Key (.p8 file)**: The file you downloaded

### Step 6: Configure TagzApp

**Admin UI:**
1. Navigate to **Admin > External Authentication**
2. Find **"Apple"** provider
3. Enter the configuration:
   - **Client ID**: Your Services ID
   - **Client Secret**: This is complex - see "Generating Client Secret" below
   - **Enabled**: Toggle to enable
4. Click **"Save"**

### Generating the Client Secret

Apple uses JWT tokens instead of simple client secrets. You need to:

1. Use your Team ID, Key ID, and private key (.p8)
2. Generate a JWT token programmatically or use a tool
3. The JWT must be signed with ES256 algorithm
4. Token is valid for up to 6 months

**Example using a JWT library:**
```csharp
var privateKey = "-----BEGIN PRIVATE KEY-----\n[your key]\n-----END PRIVATE KEY-----";
var token = CreateAppleJWT(
    teamId: "YOUR_TEAM_ID",
    clientId: "com.yourdomain.tagzapp.web",
    keyId: "YOUR_KEY_ID",
    privateKey: privateKey
);
```

**Note:** This is complex. Consider using Apple's official SDKs or existing libraries.

**Configuration File:**
```json
{
  "Authentication": {
    "Apple": {
      "ClientID": "com.yourdomain.tagzapp.web",
      "ClientSecret": "your-jwt-client-secret",
      "Enabled": "true"
    }
  }
}
```

## Testing

1. Log out of TagzApp
2. Click **"Sign in with Apple"**
3. Sign in with your Apple ID
4. Choose whether to share or hide your email
5. You should be signed in to TagzApp

## Troubleshooting

### "Invalid Client"
- Verify Services ID (Client ID) is correct
- Check that JWT client secret is valid and not expired
- Regenerate JWT if it's been more than 6 months

### "Invalid Request"
- Verify return URL in Apple Developer Portal matches exactly
- Ensure domain is verified
- Check Services ID configuration

### "Domain Not Verified"
- Apple requires domain verification
- Ensure your domain is added and verified in Services ID config

### Client Secret Expired
- Apple JWT tokens expire after 6 months maximum
- Generate a new JWT token with the same key
- Update TagzApp configuration

## Privacy Features

Apple Sign In includes privacy-focused features:
- **Hide My Email**: Users can choose to hide their actual email
- **Private relay**: Apple provides a relay email (e.g., xyz@privaterelay.appleid.com)
- **Name control**: Users can choose what name to share

## Security Notes

1. **Protect your private key**: Never commit the .p8 file to source control
2. **Rotate keys periodically**: Generate new keys annually
3. **Limit key access**: Only authorized personnel should have access
4. **JWT expiration**: Set appropriate expiration (up to 6 months)

## Advanced Configuration

### Handling Private Relay Emails

When users hide their email, Apple provides a relay address:
- Forward emails to this relay address
- Apple forwards to the user's real email
- Relay remains active even if user changes actual email

### Revoking Access

Users can revoke access at:
1. Apple ID website or iOS Settings
2. Navigate to Security > Apps Using Your Apple ID
3. Select TagzApp and click "Stop Using Apple ID"

## Limitations

- **Requires Apple Developer account** ($99/year)
- **Complex setup** compared to other OAuth providers
- **JWT secret management** adds complexity
- **iOS-first design** may not integrate as smoothly with web apps

## Additional Resources

- [Sign in with Apple Documentation](https://developer.apple.com/sign-in-with-apple/)
- [Apple Developer Portal](https://developer.apple.com/account)
- [Configuring Your Webpage](https://developer.apple.com/documentation/sign_in_with_apple/sign_in_with_apple_js/configuring_your_webpage_for_sign_in_with_apple)
- [Generate and Validate Tokens](https://developer.apple.com/documentation/sign_in_with_apple/generate_and_validate_tokens)

## Need Help?

Check the [TagzApp GitHub Issues](https://github.com/FritzAndFriends/TagzApp/issues) or consult Apple's Sign in with Apple documentation.

**Note:** Due to the complexity of Apple Sign In, consider starting with simpler OAuth providers (Google, GitHub, Microsoft) first.
