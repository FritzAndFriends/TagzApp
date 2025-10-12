# TagzApp Twitch Chat Provider

This provider connects to Twitch IRC chat and processes incoming messages.

## OpenTelemetry Integration

This provider includes OpenTelemetry tracing for message processing operations. The following activities are instrumented:

### Activity Sources

- **Activity Source Name**: `TagzApp.Providers.TwitchChat`
- **Activity Name**: `Process Message` - Traces the processing of incoming Twitch messages
- **Activity Name**: `Send Twitch Message` - Traces outgoing messages to Twitch chat
- **Activity Name**: `Send Twitch Whisper` - Traces whisper messages sent to specific users

### Telemetry Tags

The following tags are added to activities for enhanced observability:

#### Message Processing (`Process Message`)
- `tagzapp.provider` - Set to "twitch" to identify the provider
- `twitch.message.type` - Type of message (`ping`, `chat`)
- `tagzapp.message.username` - Username of the message sender
- `tagzapp.message.timestamp` - Message timestamp from Twitch
- `twitch.connection.welcome` - Set to true when connection welcome message is received

#### Message Sending (`Send Twitch Message`, `Send Twitch Whisper`)
- `twitch.channel` - The target channel
- `twitch.bot.name` - The bot's username
- `twitch.message.content` - The message being sent
- `twitch.message.type` - Type of message (`post`, `whisper`)
- `twitch.target.user` - Target user for whispers

### OpenTelemetry Configuration

To enable tracing for this provider, add the activity source to your OpenTelemetry configuration:

```csharp
services.AddOpenTelemetry()
    .WithTracing(tracing => 
        tracing.AddSource(ChatClient.ACTIVITY_NAME));
```

Or in the ServiceDefaults Extensions.cs:

```csharp
.WithTracing(tracing =>
{
    tracing.AddAspNetCoreInstrumentation()
           .AddHttpClientInstrumentation()
           .AddSource("TagzApp.Providers.TwitchChat"); // Add this line
})
```

This enables distributed tracing across all Twitch chat operations, allowing you to track message flow, performance, and correlate logs with specific messages and users.
