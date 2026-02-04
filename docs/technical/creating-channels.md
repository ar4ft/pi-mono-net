# Creating Channels
Integrate new messaging platforms with Pi-Mono-Net.

## What are Channels?
Channels connect messaging platforms to the Gateway.

## IChannel Interface
```csharp
public interface IChannel
{
    string Id { get; }
    string Name { get; }
    ChannelCapabilities Capabilities { get; }
    
    event EventHandler<ChannelMessage> MessageReceived;
    
    Task StartAsync(CancellationToken cancellationToken);
    Task StopAsync();
    Task SendMessageAsync(ChannelMessage message, ...);
}
```

## Example: Telegram Channel
```csharp
public class TelegramChannel : ChannelBase
{
    private readonly TelegramBotClient _client;
    
    public override string Id => "telegram";
    public override string Name => "Telegram";
    
    protected override async Task StartInternalAsync(...)
    {
        _client.StartReceiving(HandleUpdateAsync, ...);
    }
    
    public override async Task SendMessageAsync(...)
    {
        await _client.SendTextMessageAsync(...);
    }
    
    private async Task HandleUpdateAsync(...)
    {
        var message = new ChannelMessage { /* normalize */ };
        OnMessageReceived(message);
    }
}
```

## Session Key Format
```
agent:{agentName}:{channel}:{type}:{id}
```

Example: `agent:main:telegram:private:123456`

## Registering Channels
```csharp
var channel = new TelegramChannel(botToken);
await gateway.RegisterChannelAsync(channel);
await channel.StartAsync(cancellationToken);
```

See [Architecture](architecture.md) for more details.
