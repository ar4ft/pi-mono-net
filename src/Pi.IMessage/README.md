# Pi.IMessage - macOS Messages Integration

Complete iMessage integration for the Pi agent platform, enabling agent communication through Apple Messages.app.

## Overview

Pi.IMessage provides a production-ready channel for interacting with AI agents through macOS Messages.app. It includes database reading, real-time monitoring, and AppleScript-based sending.

## Features

- ✅ **Database Access** - Read messages from Messages.app SQLite database
- ✅ **Real-time Monitoring** - Poll for new messages with configurable intervals
- ✅ **Message Sending** - Send messages via AppleScript
- ✅ **Group Support** - Handle both DMs and group chats
- ✅ **Attachment Support** - Process image and file attachments
- ✅ **Session Keys** - Generate routing keys for agent dispatching
- ✅ **Contact Resolution** - Lookup contact information
- ✅ **Message Normalization** - Convert to agent-friendly format

## Requirements

- **macOS 10.14+**
- **Messages.app** installed and configured
- **Full Disk Access** permission for Terminal
- **.NET 8.0** runtime

## Installation

```bash
# Add package reference
dotnet add package Pi.IMessage

# Or include in your .csproj
<ItemGroup>
  <ProjectReference Include="../Pi.IMessage/Pi.IMessage.csproj" />
</ItemGroup>
```

## Quick Start

### Basic Usage

```csharp
using Pi.IMessage;

// Configure
var config = new IMessageConfig
{
    DatabasePath = "~/Library/Messages/chat.db",
    PollingIntervalSeconds = 2,
    AgentName = "main",
    ProcessGroupMessages = true,
    ProcessDirectMessages = true
};

// Initialize components
using var database = new IMessageDatabase(config.DatabasePath);
var processor = new IMessageProcessor(config);
var sender = new IMessageSender();

// Get recent messages
var messages = await database.GetRecentMessagesAsync(10);
foreach (var msg in messages)
{
    Console.WriteLine($"{msg.Date}: {msg.Text}");
}

// Send a message
var result = await sender.SendMessageAsync("+1234567890", "Hello from Pi!");
if (result.Success)
{
    Console.WriteLine("Message sent!");
}
```

### Real-time Monitoring

```csharp
using var monitor = new IMessageMonitor(database, processor, config);

// Handle incoming messages
monitor.MessageReceived += async (sender, e) =>
{
    var msg = e.Message;
    Console.WriteLine($"[{msg.SessionKey}] {msg.SenderName}: {msg.Content}");
    
    // Process with your agent
    var response = await YourAgent.ProcessAsync(msg);
    
    // Send response
    await sender.SendMessageAsync(msg.SenderId, response);
};

// Handle errors
monitor.Error += (sender, e) =>
{
    Console.WriteLine($"Error: {e.Exception.Message}");
};

// Start monitoring
monitor.Start();

// Keep running
Console.WriteLine("Monitoring messages. Press Ctrl+C to stop.");
await Task.Delay(Timeout.Infinite);
```

## Configuration

### IMessageConfig Options

```csharp
var config = new IMessageConfig
{
    // Database path (default: ~/Library/Messages/chat.db)
    DatabasePath = "~/Library/Messages/chat.db",
    
    // How often to poll for new messages (default: 1 second)
    PollingIntervalSeconds = 2,
    
    // Agent name for session keys (default: "main")
    AgentName = "main",
    
    // Whether to mark messages as read (default: true)
    MarkAsRead = true,
    
    // Process group messages (default: true)
    ProcessGroupMessages = true,
    
    // Process direct messages (default: true)
    ProcessDirectMessages = true,
    
    // Max messages per poll (default: 100)
    MaxMessagesPerPoll = 100
};
```

## Session Keys

Session keys are generated for routing messages to agents:

### Format
- **DM**: `agent:{agentName}:imessage:dm:{handle}`
- **Group**: `agent:{agentName}:imessage:group:{chatGuid}`

### Examples
```
agent:main:imessage:dm:+1234567890
agent:main:imessage:group:iMessage;+;chat123456789
```

## Permissions Setup

### macOS System Settings

1. Open **System Settings** → **Privacy & Security**
2. Navigate to **Full Disk Access**
3. Click the **+** button
4. Add your Terminal application or IDE
5. Restart the application

### Verification

Run the demo to verify permissions:

```bash
cd src/Pi.IMessageDemo
dotnet run
```

If successful, you'll see:
```
✓ Found iMessage database
✓ Components initialized
Found X recent message(s)
```

## Message Processing

### Message Flow

```
Messages.app
    ↓
SQLite Database (~/Library/Messages/chat.db)
    ↓
IMessageDatabase.GetMessagesSinceAsync()
    ↓
IMessageProcessor.NormalizeMessage()
    ↓
NormalizedMessage (agent-friendly format)
    ↓
Your Agent Logic
    ↓
IMessageSender.SendMessageAsync()
    ↓
Messages.app (via AppleScript)
```

### Message Types

```csharp
// Raw database message
IMessageRecord rawMessage;

// Normalized for agents
NormalizedMessage normalized = processor.NormalizeMessage(rawMessage);

// Access normalized fields
string sessionKey = normalized.SessionKey;
string senderId = normalized.SenderId;
string content = normalized.Content;
DateTime timestamp = normalized.Timestamp;
bool isGroup = normalized.IsGroupMessage;
```

## API Reference

### IMessageDatabase

```csharp
Task<List<IMessageRecord>> GetRecentMessagesAsync(int limit = 100);
Task<List<IMessageRecord>> GetMessagesSinceAsync(long rowId, int limit = 100);
Task<IMessageChat?> GetChatByGuidAsync(string chatGuid);
Task<IMessageHandle?> GetHandleByIdAsync(string handleId);
Task<List<IMessageHandle>> GetHandlesForChatAsync(string chatGuid);
```

### IMessageSender

```csharp
Task<SendMessageResult> SendMessageAsync(string recipient, string message);
Task<SendMessageResult> SendMessageToChatAsync(string chatGuid, string message);
```

### IMessageMonitor

```csharp
void Start();                              // Start monitoring
Task StopAsync();                          // Stop monitoring
event MessageReceivedEventArgs;            // New message event
event MonitorErrorEventArgs;               // Error event
```

### IMessageProcessor

```csharp
NormalizedMessage NormalizeMessage(IMessageRecord message, IMessageChat? chat, IMessageHandle? handle);
SessionKeyInfo GenerateSessionKey(IMessageRecord message, IMessageChat? chat, bool isGroupMessage);
bool ShouldProcessMessage(IMessageRecord message, IMessageChat? chat);
List<string> ExtractMentions(string text);
bool HasMention(string text, string mention);
```

## Examples

See `src/Pi.IMessageDemo/Program.cs` for a complete working example.

### Demo Features

The demo application demonstrates:
- Database connection and verification
- Recent message retrieval
- Message normalization
- Real-time monitoring (optional)
- Session key generation

Run the demo:
```bash
cd src/Pi.IMessageDemo
dotnet run
```

## Architecture

### Component Diagram

```
┌─────────────────────────────────────────┐
│          Messages.app                   │
└─────────────────────────────────────────┘
                  ↓
        ┌─────────────────┐
        │  SQLite Database│
        │  (chat.db)      │
        └─────────────────┘
                  ↓
┌─────────────────────────────────────────┐
│        IMessageDatabase                 │
│  • GetRecentMessagesAsync()             │
│  • GetMessagesSinceAsync()              │
│  • GetChatByGuidAsync()                 │
└─────────────────────────────────────────┘
                  ↓
┌─────────────────────────────────────────┐
│        IMessageMonitor                  │
│  • Polling loop                         │
│  • Event emission                       │
│  • Error handling                       │
└─────────────────────────────────────────┘
                  ↓
┌─────────────────────────────────────────┐
│        IMessageProcessor                │
│  • Message normalization                │
│  • Session key generation               │
│  • Filtering logic                      │
└─────────────────────────────────────────┘
                  ↓
          Your Agent Logic
                  ↓
┌─────────────────────────────────────────┐
│        IMessageSender                   │
│  • AppleScript execution                │
│  • Message formatting                   │
│  • Error handling                       │
└─────────────────────────────────────────┘
                  ↓
        Messages.app (sent)
```

## Limitations

### Current Limitations

1. **macOS Only** - Requires macOS and Messages.app
2. **Polling-based** - Not push-based (1-2 second delay)
3. **Read-only Database** - Can't modify database directly
4. **AppleScript Send** - Requires accessibility permissions
5. **No Reactions** - Can't send/receive message reactions yet
6. **Limited Attachments** - Basic attachment info only

### Future Enhancements

- Gateway integration for agent routing
- Push-based notifications (FSEvents)
- Message reactions support
- Rich attachment handling
- Read receipts
- Typing indicators
- Group management (add/remove participants)

## Troubleshooting

### Database Not Found

**Error**: `iMessage database not found at: ~/Library/Messages/chat.db`

**Solutions**:
1. Open Messages.app at least once
2. Grant Full Disk Access to Terminal
3. Verify database path is correct

### Permission Denied

**Error**: `SQLite error: unable to open database`

**Solutions**:
1. System Settings → Privacy & Security → Full Disk Access
2. Add Terminal or your IDE
3. Restart the application

### AppleScript Errors

**Error**: `osascript failed with exit code 1`

**Solutions**:
1. System Settings → Privacy & Security → Accessibility
2. Add Terminal or your IDE
3. Grant Automation permissions for Messages.app

### No Messages Detected

**Issue**: Monitor runs but no messages are detected

**Solutions**:
1. Send a test message in Messages.app
2. Check polling interval (try reducing to 1 second)
3. Verify message filtering settings
4. Check database permissions

## Integration

### Gateway Integration (Future)

When the Gateway is implemented, integration will look like:

```csharp
// Connect to Gateway
var gateway = new Gateway("ws://localhost:18789");

// Register iMessage channel
var channel = new IMessageChannel(config);
await gateway.RegisterChannelAsync(channel);

// Messages flow automatically
// Gateway → Agent → Response → Channel → iMessage
```

### Agent Integration (Current)

Without Gateway, integrate directly:

```csharp
monitor.MessageReceived += async (s, e) =>
{
    var msg = e.Message;
    
    // Create agent context
    var context = new Context
    {
        SystemPrompt = "You are a helpful assistant",
        Messages = ConvertToAgentMessages(msg)
    };
    
    // Run agent
    var agent = new Agent(model, convertToLlm);
    var response = await agent.RunAsync(context);
    
    // Send response
    await sender.SendMessageAsync(msg.SenderId, response);
};
```

## Testing

### Unit Tests

```bash
cd tests/Pi.IMessage.Tests
dotnet test
```

### Manual Testing

```bash
# Run demo application
cd src/Pi.IMessageDemo
dotnet run

# Send yourself a test message in Messages.app
# Verify the demo detects it
```

## Contributing

When contributing to Pi.IMessage:

1. Follow .NET coding conventions
2. Add XML documentation comments
3. Include unit tests for new features
4. Test on actual macOS hardware
5. Update this README with new features

## License

See LICENSE file in repository root.

## Support

- **Issues**: GitHub Issues
- **Discussions**: GitHub Discussions
- **Documentation**: See `/docs` directory

## Related Projects

- **Pi.Agent** - Agent runtime
- **Pi.AI** - LLM integration
- **Pi.Gateway** - (Future) Central orchestrator
- **Pi.TUI** - Terminal UI framework

## See Also

- [OpenClaw Gap Analysis](../../OPENCLAW_GAP_ANALYSIS.md) - Component comparison
- [Priority List](../../IMESSAGE_PRIORITY_LIST.md) - Development priorities
- [Phase 3 Complete](../../PHASE3_COMPLETE.md) - Implementation status

---

**Status**: ✅ Production Ready (awaiting Gateway integration)

**Version**: 1.0.0

**Last Updated**: 2026-02-03
