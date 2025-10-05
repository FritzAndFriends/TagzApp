# GitHub OAuth Configuration Guide

This guide will help you configure GitHub OAuth authentication for TagzApp, allowing users to sign in with their GitHub accounts.

## Overview

The GitHub OAuth provider enables users to authenticate to TagzApp using their GitHub accounts. This is particularly useful for developer-focused events and communities.

## Prerequisites

- A GitHub account
- Repository or organization admin access (for organizational apps)

## Step 1: Create a GitHub OAuth App

1. Go to [GitHub.com](https://github.com) and sign in
2. Click on your profile picture (top right) and select **"Settings"**
3. Scroll down in the left sidebar and click **"Developer settings"**
4. Click **"OAuth Apps"** in the left sidebar
5. Click **"New OAuth App"** or **"Register a new application"**

## Step 2: Configure Your OAuth Application

Fill in the application registration form:

### Application Details

- **Application name**: "TagzApp" (or your preferred name)
- **Homepage URL**: Your TagzApp URL
  - Production: `https://yourdomain.com`
  - Development: `https://localhost:5001`
- **Application description**: (Optional) "TagzApp social media aggregator authentication"
- **Authorization callback URL**: Your TagzApp OAuth callback URL
  - Production: `https://yourdomain.com/signin-github`
  - Development: `https://localhost:5001/signin-github`
  - **Important:** This must match exactly what TagzApp uses

### Important Notes

- The callback URL must match exactly (protocol, domain, port, path)
- You can only specify one callback URL per OAuth app
- For multiple environments, create separate OAuth apps

Click **"Register application"**

## Step 3: Get Your Client Credentials

After creating your OAuth app, you'll see the application page:

1. Copy the **Client ID** - this is visible on the page
2. Click **"Generate a new client secret"**
3. **Important:** Copy the **Client Secret** immediately - you won't be able to see it again!
4. Store both the Client ID and Client Secret securely

**⚠️ Security Warning:** The Client Secret is sensitive. Never share it, commit it to source control, or expose it in client-side code.

## Step 4: Configure TagzApp

### Using the Admin UI (Recommended)

1. Log in to TagzApp as an administrator
2. Navigate to **Admin > External Authentication**
3. Find the **"GitHub"** provider
4. Enter your credentials:
   - **Client ID**: Your GitHub OAuth App Client ID
   - **Client Secret**: Your GitHub OAuth App Client Secret
   - **Enabled**: Toggle to enable the provider
5. Click **"Save Configuration"**

### Using Configuration Files

**appsettings.json:**
```json
{
  "Authentication": {
    "GitHub": {
      "ClientID": "your-github-client-id",
      "ClientSecret": "your-github-client-secret",
      "Enabled": "true"
    }
  }
}
```

**Environment Variables:**
```bash
Authentication__GitHub__ClientID=your-github-client-id
Authentication__GitHub__ClientSecret=your-github-client-secret
Authentication__GitHub__Enabled=true
```

**Azure Key Vault:**
Store with the key:
```
TagzApp-Authentication-GitHub
```

## Step 5: Test Your Configuration

1. Log out of TagzApp (if logged in)
2. Navigate to the login page
3. You should see a **"Sign in with GitHub"** button
4. Click the button
5. You'll be redirected to GitHub's authorization page
6. Click **"Authorize [your-app-name]"** to grant permissions
7. You'll be redirected back to TagzApp and logged in

## Understanding GitHub Scopes

GitHub OAuth uses scopes to control what data your application can access:

### Default Scopes

The GitHub OAuth provider in TagzApp uses minimal scopes by default:
- `read:user` - Read basic user profile information
- `user:email` - Read user email addresses

### User Data Accessed

With these scopes, TagzApp can access:
- GitHub username
- Display name
- Public email address
- Avatar/profile picture
- Unique user ID

### Privacy Considerations

- Only public profile information is accessed
- Private repositories are not accessed
- Organization memberships may be visible if public
- Users control what email addresses are public

## Troubleshooting

### Common Issues

#### "Redirect URI Mismatch" Error
- **Cause**: The callback URL doesn't match the one configured in your GitHub OAuth app
- **Solution**: 
  - Verify the Authorization callback URL in your GitHub OAuth app settings
  - Ensure it matches: `https://yourdomain.com/signin-github`
  - Check protocol (http vs https) and port match exactly
  - No trailing slashes should be added

#### "Bad Verification Code" Error
- **Cause**: OAuth flow failed or expired
- **Solution**: 
  - Try authenticating again from the start
  - Clear browser cache and cookies
  - Verify Client ID and Client Secret are correct
  - Check that the OAuth app is not suspended

#### "Application Suspended" Error
- **Cause**: Your GitHub OAuth app has been suspended (rare)
- **Solution**: 
  - Check your GitHub account for notifications
  - Review GitHub's OAuth App guidelines
  - Contact GitHub Support if needed

#### "Invalid Client" Error
- **Cause**: Client ID or Client Secret is incorrect
- **Solution**: 
  - Verify you copied the full Client ID
  - Regenerate Client Secret if needed
  - Update TagzApp configuration
  - Ensure there are no extra spaces or characters

#### Sign In Button Not Appearing
- **Cause**: Provider not enabled or configuration not saved
- **Solution**: 
  - Check that GitHub provider is enabled in TagzApp admin
  - Verify configuration was saved successfully
  - Restart TagzApp if needed
  - Check application logs for errors

## Security Best Practices

1. **Use separate OAuth apps** for development and production
2. **Rotate client secrets** periodically (every 6-12 months)
3. **Never commit secrets** to source control
4. **Use environment variables** or secure vaults for credentials
5. **Monitor OAuth app usage** in GitHub settings
6. **Revoke unused OAuth apps** regularly
7. **Review authorized applications** in your GitHub account settings

## Multiple Environments

### Best Practice: Separate OAuth Apps

Create different OAuth apps for each environment:

1. **Development OAuth App**
   - Callback URL: `https://localhost:5001/signin-github`
   - Name: "TagzApp (Development)"

2. **Staging OAuth App**
   - Callback URL: `https://staging.yourdomain.com/signin-github`
   - Name: "TagzApp (Staging)"

3. **Production OAuth App**
   - Callback URL: `https://yourdomain.com/signin-github`
   - Name: "TagzApp"

### Managing Multiple Apps

- Use different configuration per environment
- Each environment uses its own Client ID and Secret
- Prevents development OAuth from affecting production

## Organization OAuth Apps vs Personal OAuth Apps

### Personal OAuth Apps
- Created under your personal account
- Suitable for small teams or personal projects
- You control the OAuth app settings

### Organization OAuth Apps
- Created under a GitHub organization
- Better for team projects and enterprises
- Organization admins can manage the app
- Can integrate with organization SSO

To create an organization OAuth app:
1. Go to your organization's settings
2. Click "Developer settings" > "OAuth Apps"
3. Follow the same steps as personal OAuth apps

## Revoking Access

### For Users

Users can revoke TagzApp's access to their GitHub account:
1. Go to [GitHub Settings > Applications](https://github.com/settings/applications)
2. Find your TagzApp application in "Authorized OAuth Apps"
3. Click "Revoke" to remove access

### For Admins

To revoke or regenerate credentials:
1. Go to your OAuth app settings in GitHub
2. Regenerate client secret (old secret stops working immediately)
3. Update TagzApp configuration with new secret
4. Or delete the OAuth app entirely to revoke all access

## Monitoring OAuth App Usage

### View OAuth App Activity

1. Go to your GitHub OAuth app settings
2. No built-in analytics in GitHub OAuth apps
3. Monitor through TagzApp's own logs and analytics
4. Check GitHub account notification for security alerts

### Security Alerts

GitHub may notify you if:
- Unusual authorization patterns detected
- OAuth app policy violations
- Security issues with your app

## Rate Limits

GitHub OAuth has rate limits:
- **Authenticated requests**: 5,000 requests per hour (per user)
- **OAuth authorizations**: No specific limit for OAuth flow
- TagzApp's authentication only uses a few requests per login

## GitHub Enterprise Support

TagzApp can work with GitHub Enterprise:

### Configuration Changes

For GitHub Enterprise Server (self-hosted):

```json
{
  "Authentication": {
    "GitHub": {
      "ClientID": "your-client-id",
      "ClientSecret": "your-client-secret",
      "AuthorizationEndpoint": "https://github.yourcompany.com/login/oauth/authorize",
      "TokenEndpoint": "https://github.yourcompany.com/login/oauth/access_token",
      "UserInformationEndpoint": "https://github.yourcompany.com/api/v3/user"
    }
  }
}
```

**Note:** This may require custom configuration in TagzApp's authentication setup.

## Privacy and Data Handling

### What Data is Collected

TagzApp stores:
- GitHub username
- Display name
- Email address (if public)
- GitHub user ID
- Avatar URL (for display)

### What Data is NOT Collected

- Private repository information
- Code or file contents
- Organization secrets
- SSH keys or personal access tokens
- Private organization memberships

## Compliance

### GDPR

- Users can view and export their data through TagzApp
- Users can delete their account and data
- Users can revoke OAuth access at any time

### Data Retention

- User profile data stored as long as account exists
- Authentication tokens refreshed automatically
- No unnecessary data retention

## Additional Resources

- [GitHub OAuth Documentation](https://docs.github.com/en/developers/apps/building-oauth-apps)
- [Creating an OAuth App](https://docs.github.com/en/developers/apps/building-oauth-apps/creating-an-oauth-app)
- [Authorizing OAuth Apps](https://docs.github.com/en/developers/apps/building-oauth-apps/authorizing-oauth-apps)
- [OAuth App Scopes](https://docs.github.com/en/developers/apps/building-oauth-apps/scopes-for-oauth-apps)
- [GitHub Authentication Best Practices](https://docs.github.com/en/developers/apps/getting-started-with-apps/about-apps)

## Need Help?

If you encounter issues not covered here:
1. Check the [TagzApp GitHub Issues](https://github.com/FritzAndFriends/TagzApp/issues)
2. Review GitHub OAuth app settings for configuration errors
3. Check TagzApp application logs for detailed error messages
4. Consult GitHub's OAuth documentation
5. Open a new issue on GitHub with details about your problem
