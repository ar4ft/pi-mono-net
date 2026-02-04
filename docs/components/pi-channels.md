# Pi.Channels - Channel Abstraction

Channel abstraction layer for multi-platform support.

## Features
- IChannel interface
- ChannelMessage normalization
- SessionKeyBuilder utilities
- ChannelCapabilities detection

## Usage
```csharp
public class MyChannel : ChannelBase
{
    public override string Id => "my-channel";
    public override string Name => "My Channel";
    
    protected override async Task StartInternalAsync(...)
    {
        // Start listening for messages
    }
    
    public override async Task SendMessageAsync(...)
    {
        // Send message to platform
    }
}
```

## See Also
- [Creating Channels](../technical/creating-channels.md)
- [iMessage Channel](pi-imessage.md)
