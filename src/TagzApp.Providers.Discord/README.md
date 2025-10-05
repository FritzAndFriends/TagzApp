# Discord Provider for TagzApp

The Discord Provider enables real-time monitoring of Discord channels for live messages in TagzApp. This provider connects to Discord using the Discord Gateway API to receive messages as they are posted.

## Features

- **Real-time Message Monitoring**: Receives messages instantly via WebSocket connection
- **Channel-based Filtering**: Monitors specific Discord channels in specific servers
- **Message Type Filtering**: Options to include/exclude bot messages and system messages
- **Rich Content Support**: Includes Discord embeds and attachment information
- **User Blocking**: Block specific users from appearing in the feed
- **Automatic Reconnection**: Handles connection drops with exponential backoff retry logic

## Configuration

### Required Settings

- **Bot Token**: Discord bot token for API access
- **Guild ID**: The Discord server (guild) ID to monitor
- **Channel ID**: The specific channel ID to monitor

### Optional Settings

- **Include Bot Messages**: Whether to show messages from bots (default: false)
- **Include System Messages**: Whether to show join/leave messages (default: false)
- **Minimum Message Length**: Hide messages shorter than this length (default: 1)
- **Blocked Users**: Comma-separated list of user IDs to block
- **Max Queue Size**: Maximum messages to keep in memory (default: 1000)
- **Max Reconnect Attempts**: Maximum reconnection attempts (default: 5)
- **Enable Rich Embeds**: Include Discord embed content (default: true)

## Discord Bot Setup

### 1. Create a Discord Application

1. Go to the [Discord Developer Portal](https://discord.com/developers/applications)
2. Click "New Application" and give it a name
3. Go to the "Bot" section and click "Add Bot"
4. Copy the bot token and save it securely

### 2. Configure Bot Permissions

Required bot permissions:
- **View Channel**: To see the channel
- **Read Message History**: To receive messages

Required intents:
- **Message Content Intent**: To read message text (must be enabled in Discord Developer Portal)

### 3. Invite Bot to Server

1. In the Discord Developer Portal, go to OAuth2 > URL Generator
2. Select scopes: `bot`
3. Select permissions: `View Channel`, `Read Message History`
4. Use the generated URL to invite the bot to your server

### 4. Get Server and Channel IDs

1. Enable Developer Mode in Discord (Settings > Advanced > Developer Mode)
2. Right-click on the server name and select "Copy Server ID"
3. Right-click on the channel name and select "Copy Channel ID"

## Technical Implementation

### WebSocket Connection

The provider uses Discord's Gateway API v10 with WebSocket connection:
- Gateway URL: `wss://gateway.discord.gg/?v=10&encoding=json`
- Automatic heartbeat management
- Reconnection with exponential backoff
- Session resumption support

### Message Processing

Messages are processed in real-time and filtered based on:
- Channel membership
- Message type
- User preferences (bots, system messages)
- User blocking list
- Message length requirements

### Content Mapping

Discord messages are mapped to TagzApp's Content model:
- **Provider**: "DISCORD"
- **ProviderId**: Discord message ID
- **Type**: ContentType.Message
- **Author**: Discord user information with avatar
- **Text**: Message content with attachments and embeds
- **SourceUri**: Direct link to the Discord message
- **ExtendedMetadata**: Discord-specific information

## Status Monitoring

The provider reports status through the TagzApp status system:
- **Healthy**: Connected and receiving messages
- **Connecting**: Establishing connection
- **Unhealthy**: Connection failed or configuration invalid
- **Disabled**: Provider disabled in configuration

## Performance Considerations

- Messages are queued in memory with configurable size limits
- WebSocket connection is maintained with automatic heartbeat
- Reconnection uses exponential backoff to avoid overwhelming Discord's servers
- Message filtering happens before content creation to reduce memory usage

## Security

- Bot tokens should be stored securely and encrypted in production
- The bot only requires minimal permissions (View Channel, Read Message History)
- No sensitive user data is stored beyond what's necessary for display
- WebSocket connections use TLS encryption

## Limitations

- Cannot read message history before the bot joins the channel
- Requires Message Content Intent for full message text (may require verification for large bots)
- Limited to one channel per provider instance
- Rate limited by Discord's Gateway limits (no action required, handled automatically)