# Pi.Gateway - Message Routing

Central message broker and orchestrator.

## Features
- WebSocket server
- Session management
- Message routing
- Channel registry
- Event bus

## Usage
```csharp
var gateway = new GatewayService();

// Register channels
await gateway.RegisterChannelAsync(iMessageChannel);
await gateway.RegisterChannelAsync(telegramChannel);

// Start gateway
await gateway.StartAsync(cancellationToken);
```

## Session Management
Sessions identified by session keys:
```
agent:{name}:{channel}:{type}:{id}
```

## See Also
- [Creating Channels](../technical/creating-channels.md)
- [Architecture](../technical/architecture.md)
