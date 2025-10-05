# TagzApp Documentation

This folder contains documentation for TagzApp.

## Available Documentation

### User Manuals

- **[Moderator User Manual](Moderator_User_Manual.md)** - Comprehensive guide for content moderators
  - Interface overview and navigation
  - Moderation actions and workflows
  - Content filtering and organization
  - User management and blocking
  - Real-time features and troubleshooting

### Content Provider Configuration Guides

These guides explain how to obtain API credentials and configure social media content providers:

- **[Twitter/X Configuration](providers/content/Twitter-Configuration.md)** - Configure Twitter/X for hashtag monitoring
  - Twitter Developer account setup
  - Creating Twitter applications
  - Obtaining Bearer Tokens
  - Rate limits and best practices

- **[Mastodon Configuration](providers/content/Mastodon-Configuration.md)** - Configure Mastodon federation monitoring
  - Understanding Mastodon federation
  - Choosing Mastodon instances
  - No API keys required
  - Public API access

- **[Bluesky Configuration](providers/content/Bluesky-Configuration.md)** - Configure Bluesky (AT Protocol) monitoring
  - Understanding the AT Protocol
  - Public API access
  - No credentials required
  - Decentralized social networking

- **[Twitch Chat Configuration](providers/content/TwitchChat-Configuration.md)** - Configure Twitch live chat monitoring
  - Twitch Developer Console setup
  - Creating OAuth applications
  - Bot account configuration
  - Real-time chat integration

- **[YouTube Chat Configuration](providers/content/YouTubeChat-Configuration.md)** - Configure YouTube live stream chat
  - Google Cloud Platform setup
  - YouTube Data API v3 configuration
  - OAuth 2.0 authentication
  - Quota management

- **[Blazot Configuration](providers/content/Blazot-Configuration.md)** - Configure Blazot social network monitoring
  - Blazot account creation
  - User vs Site accounts
  - API key generation
  - Rate limit management

### Authentication Provider Configuration Guides

These guides explain how to set up external login providers for TagzApp:

- **[Microsoft OAuth Configuration](providers/authentication/Microsoft-OAuth-Configuration.md)** - Enable Sign in with Microsoft
  - Azure Active Directory setup
  - App registration
  - Client credentials
  - Multi-tenant support

- **[GitHub OAuth Configuration](providers/authentication/GitHub-OAuth-Configuration.md)** - Enable Sign in with GitHub
  - GitHub Developer Console
  - OAuth app creation
  - Scope management
  - Organization apps

- **[Google OAuth Configuration](providers/authentication/Google-OAuth-Configuration.md)** - Enable Sign in with Google
  - Google Cloud Console setup
  - OAuth consent screen
  - Client credentials
  - Publishing apps

- **[Twitch OAuth Configuration](providers/authentication/Twitch-OAuth-Configuration.md)** - Enable Sign in with Twitch
  - Twitch Developer Portal
  - Application registration
  - OAuth redirect configuration

- **[LinkedIn OAuth Configuration](providers/authentication/LinkedIn-OAuth-Configuration.md)** - Enable Sign in with LinkedIn
  - LinkedIn Developer Portal
  - App creation
  - LinkedIn Page requirement
  - Product approval

- **[Apple Sign In Configuration](providers/authentication/Apple-OAuth-Configuration.md)** - Enable Sign in with Apple
  - Apple Developer account
  - Services ID configuration
  - JWT client secret generation
  - Privacy features

## Screenshots

The `images/` folder contains screenshots and visual references used in the documentation:

- `aspire-dashboard.png` - .NET Aspire development dashboard
- `tagzapp-login-page.png` - Authentication page with external providers
- `moderation-interface-overview.png` - Main moderation interface
- `moderation-hover-controls.png` - Interactive moderation controls
- `moderation-header-controls.png` - Header area and filter controls
- `moderation-filter-dropdown.png` - Filter dropdown options
- `moderation-approved-only-filter.png` - Filtered view showing approved content
- `moderation-after-approval.png` - Message state after approval action

## Contributing

When adding new documentation:
1. Place markdown files in the `docs/` folder
2. Place images in the `docs/images/` folder
3. Use relative paths for image references: `images/filename.png`
4. Update this README with new documentation links
