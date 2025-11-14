# Microsoft OAuth Configuration Guide

This guide will help you configure Microsoft OAuth authentication for TagzApp, allowing users to sign in with their Microsoft accounts (including Office 365 and Azure AD accounts).

## Overview

The Microsoft OAuth provider enables users to authenticate to TagzApp using their Microsoft accounts. This includes:
- Personal Microsoft accounts (outlook.com, hotmail.com, live.com)
- Work or school accounts (Office 365, Azure AD)
- Azure B2C accounts

## Prerequisites

- An Azure account (free tier available)
- Access to Azure Active Directory (included with Azure account)
- Administrator permissions to register applications

## Step 1: Sign In to Azure Portal

1. Go to the [Azure Portal](https://portal.azure.com)
2. Sign in with your Microsoft account
3. If you don't have an Azure account, create one (free tier available)

## Step 2: Register an Application

1. In the Azure Portal, search for **"Azure Active Directory"** or **"Microsoft Entra ID"**
2. Click on **Azure Active Directory** in the search results
3. In the left menu, click **"App registrations"**
4. Click **"+ New registration"**
5. Fill in the application details:
   - **Name**: "TagzApp" (or your preferred name)
   - **Supported account types**: Choose based on your needs:
     - **Single tenant**: Only your organization
     - **Multi-tenant**: Any organization's directory
     - **Multi-tenant and personal**: Any organization + personal Microsoft accounts (Recommended)
     - **Personal only**: Only personal Microsoft accounts
   - **Redirect URI**: 
     - Platform: **Web**
     - URI: `https://yourdomain.com/signin-microsoft`
     - For development: `https://localhost:5001/signin-microsoft` (adjust port as needed)
6. Click **"Register"**

## Step 3: Get Your Application Credentials

After registration, you'll see the application overview page:

1. Copy the **Application (client) ID** - this is your Client ID
2. Copy the **Directory (tenant) ID** (you may need this for certain configurations)
3. Keep this page open - you'll need it for the next step

## Step 4: Create a Client Secret

1. In your app registration, click **"Certificates & secrets"** in the left menu
2. Click **"+ New client secret"**
3. Configure the secret:
   - **Description**: "TagzApp Secret" (or your preferred description)
   - **Expires**: Choose an expiration period (6 months, 12 months, 24 months, or custom)
   - **Recommendation**: Choose 12 or 24 months and set a reminder to rotate
4. Click **"Add"**
5. **Important:** Copy the secret **Value** immediately - you won't be able to see it again!
6. Store the Client ID and Client Secret securely

**⚠️ Security Warning:** The Client Secret is sensitive. Never share it, commit it to source control, or expose it in client-side code.

## Step 5: Configure API Permissions (Optional)

By default, your app has basic sign-in permissions. Additional permissions can be added if needed:

1. In your app registration, click **"API permissions"**
2. You'll see default permissions like:
   - `User.Read` (sign users in and read their profile)
3. For basic authentication, these default permissions are sufficient
4. If you need additional permissions, click **"+ Add a permission"**

## Step 6: Add Additional Redirect URIs

If you have multiple environments (development, staging, production):

1. In your app registration, click **"Authentication"**
2. Under **"Platform configurations"** > **"Web"**
3. Click **"Add URI"**
4. Add URIs for each environment:
   - Development: `https://localhost:5001/signin-microsoft`
   - Staging: `https://staging.yourdomain.com/signin-microsoft`
   - Production: `https://yourdomain.com/signin-microsoft`
5. Click **"Save"**

## Step 7: Configure TagzApp

### Using the Admin UI (Recommended)

1. Log in to TagzApp as an administrator
2. Navigate to **Admin > External Authentication**
3. Find the **"Microsoft"** provider
4. Enter your credentials:
   - **Client ID**: Your Application (client) ID from Azure
   - **Client Secret**: Your Client Secret value
   - **Enabled**: Toggle to enable the provider
5. Click **"Save Configuration"**

### Using Configuration Files

**appsettings.json:**
```json
{
  "Authentication": {
    "Microsoft": {
      "ClientID": "your-application-client-id",
      "ClientSecret": "your-client-secret",
      "Enabled": "true"
    }
  }
}
```

**Environment Variables:**
```bash
Authentication__Microsoft__ClientID=your-application-client-id
Authentication__Microsoft__ClientSecret=your-client-secret
Authentication__Microsoft__Enabled=true
```

**Azure Key Vault:**
Store with the key:
```
TagzApp-Authentication-Microsoft
```

## Step 8: Test Your Configuration

1. Log out of TagzApp (if logged in)
2. Navigate to the login page
3. You should see a **"Sign in with Microsoft"** button
4. Click the button
5. You'll be redirected to Microsoft's login page
6. Sign in with a Microsoft account
7. Grant permissions if prompted
8. You should be redirected back to TagzApp and logged in

## Troubleshooting

### Common Issues

#### "Redirect URI Mismatch" Error
- **Cause**: The redirect URI in your app registration doesn't match the one TagzApp is using
- **Solution**: 
  - Check the exact URI TagzApp is redirecting to
  - Add or correct the redirect URI in Azure Portal
  - Ensure the protocol (http/https) and port match exactly
  - Common format: `https://yourdomain.com/signin-microsoft`

#### "Invalid Client Secret" Error
- **Cause**: Client Secret is incorrect or expired
- **Solution**: 
  - Verify you copied the entire secret value
  - Check if the secret has expired in Azure Portal
  - Generate a new secret if needed
  - Update TagzApp configuration with the new secret

#### "AADSTS50011: Reply URL Mismatch" Error
- **Cause**: Same as redirect URI mismatch
- **Solution**: Add the exact redirect URI to your app registration

#### "Cannot Authenticate" Error
- **Cause**: Client ID or Client Secret is invalid
- **Solution**: 
  - Verify Client ID is correct (check Azure Portal)
  - Verify Client Secret is correct
  - Regenerate credentials if needed
  - Clear browser cache and cookies

#### "User Not Authorized" Error
- **Cause**: User's account type not supported by your app configuration
- **Solution**: 
  - Check your app's supported account types in Azure Portal
  - Change to "Accounts in any organizational directory and personal Microsoft accounts" for broadest support
  - Ensure user meets any conditional access policies

### Account Type Issues

#### Personal Accounts Not Working
- **Cause**: App registration only supports organizational accounts
- **Solution**: 
  - In Azure Portal, go to app registration > Authentication
  - Change supported account types to include personal accounts
  - Save changes and test again

#### Work/School Accounts Not Working
- **Cause**: App registration only supports personal accounts
- **Solution**: 
  - Change supported account types to include organizational accounts
  - May require admin consent for organizational accounts

## Security Best Practices

1. **Rotate secrets regularly**: Set expiration reminders and rotate before expiry
2. **Use shortest necessary expiration**: Balance between security and maintenance
3. **Never commit secrets**: Use environment variables or Key Vault
4. **Limit redirect URIs**: Only add URIs you actually use
5. **Monitor sign-ins**: Review Azure AD sign-in logs for suspicious activity
6. **Enable conditional access**: Add MFA or other policies if needed
7. **Use managed identity**: When running in Azure, use managed identity instead of secrets

## Advanced Configuration

### Custom Token Claims

To include additional user information in tokens:

1. In Azure Portal, go to your app registration
2. Click **"Token configuration"**
3. Click **"+ Add optional claim"**
4. Choose token type (ID, Access) and select claims
5. Common claims:
   - `email` - User's email address
   - `family_name`, `given_name` - User's name
   - `upn` - User Principal Name

### Conditional Access

For additional security in enterprise scenarios:

1. In Azure Portal, go to **Azure Active Directory**
2. Click **"Security"** > **"Conditional Access"**
3. Create policies requiring:
   - Multi-factor authentication
   - Compliant devices
   - Specific locations
   - Risk-based access

### Multi-Tenant Configuration

For apps supporting multiple organizations:

1. Set account type to "Multi-tenant"
2. Tenant admin must consent for their users
3. Provide admin consent URL:
```
https://login.microsoftonline.com/{tenant}/adminconsent?client_id={client-id}
```

## Monitoring and Management

### View Sign-In Logs

1. In Azure Portal, go to **Azure Active Directory**
2. Click **"Sign-in logs"**
3. Filter by your application name
4. Review successful and failed sign-ins
5. Investigate any suspicious activity

### Application Usage

1. Go to your app registration
2. Click **"Usage and insights"** (if available)
3. View authentication metrics and trends

### Secret Expiration

1. Set calendar reminders for secret expiration
2. Rotate secrets before they expire to avoid downtime
3. Generate new secret, update TagzApp, then delete old secret

## Cost Considerations

- **Azure AD Free**: Sufficient for basic OAuth (included with Azure account)
- **Azure AD Premium P1/P2**: Required for advanced features like conditional access
- **No per-authentication charges**: OAuth authentication is free

## Compliance and Privacy

### Data Access

Microsoft OAuth for TagzApp accesses:
- User's name
- User's email address
- Basic profile information
- Unique identifier

### GDPR Compliance

- Users can revoke access at [Microsoft account settings](https://account.microsoft.com/privacy)
- Data is processed according to Microsoft's privacy policy
- TagzApp only stores necessary user information

## Additional Resources

- [Microsoft Identity Platform Documentation](https://docs.microsoft.com/azure/active-directory/develop/)
- [Register an App in Azure AD](https://docs.microsoft.com/azure/active-directory/develop/quickstart-register-app)
- [Microsoft Authentication Libraries (MSAL)](https://docs.microsoft.com/azure/active-directory/develop/msal-overview)
- [Azure AD Sign-In Logs](https://docs.microsoft.com/azure/active-directory/reports-monitoring/concept-sign-ins)
- [Conditional Access](https://docs.microsoft.com/azure/active-directory/conditional-access/)

## Need Help?

If you encounter issues not covered here:
1. Check the [TagzApp GitHub Issues](https://github.com/FritzAndFriends/TagzApp/issues)
2. Review Azure AD sign-in logs for detailed error messages
3. Consult Microsoft Identity Platform documentation
4. Open a new issue on GitHub with details about your problem
