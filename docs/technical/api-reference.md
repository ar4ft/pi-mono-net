# API Reference
Complete API documentation for Pi-Mono-Net.

## Pi.AI

### Model
```csharp
public record Model
{
    public string Id { get; init; }
    public string Provider { get; init; }
    public string BaseUrl { get; init; }
    // ...
}
```

### IStreamFunction
```csharp
public interface IStreamFunction
{
    IAsyncEnumerable<Event> Stream(
        Model model,
        Context context,
        CancellationToken cancellationToken);
}
```

## Pi.Agent

### Agent
```csharp
public class Agent
{
    public Agent(Model model, 
        ConvertToLlmFunc convertToLlm,
        List<ITool> tools = null);
    
    public async Task<string> PromptAsync(
        string message,
        CancellationToken cancellationToken);
}
```

### ITool
```csharp
public interface ITool
{
    string Name { get; }
    string Description { get; }
    object Parameters { get; }
    
    Task<ToolResult> ExecuteAsync(
        string toolCallId,
        Dictionary<string, object> arguments,
        CancellationToken cancellationToken);
}
```

## Pi.Gateway

### GatewayService
```csharp
public class GatewayService
{
    public async Task StartAsync(
        CancellationToken cancellationToken);
    public async Task StopAsync();
    public async Task RegisterChannelAsync(IChannel channel);
}
```

## Pi.Channels

### IChannel
```csharp
public interface IChannel
{
    string Id { get; }
    string Name { get; }
    
    event EventHandler<ChannelMessage> MessageReceived;
    
    Task StartAsync(CancellationToken cancellationToken);
    Task SendMessageAsync(ChannelMessage message, ...);
}
```

See component documentation for complete details.
