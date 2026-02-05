# Pi.IMessage - macOS Integration

macOS Messages.app integration.

## Features
- SQLite database access
- AppleScript message sending
- Real-time monitoring
- Group/DM detection
- Attachment support

## Requirements
- macOS 10.14+
- Full Disk Access permission
- Messages.app

## Usage
```csharp
var config = new IMessageConfig
{
    DatabasePath = "~/Library/Messages/chat.db",
    PollingIntervalSeconds = 2
};

var database = new IMessageDatabase(config.DatabasePath);
var processor = new IMessageProcessor(config);
var monitor = new IMessageMonitor(database, processor, config);

monitor.MessageReceived += (s, e) => {
    Console.WriteLine($"{e.Message.SenderName}: {e.Message.Content}");
};

monitor.Start();
```

## See Also
- [iMessage README](../../src/Pi.IMessage/README.md)
- [Creating Channels](../technical/creating-channels.md)
