# Build an Extension

Create a custom extension in 15 minutes.

## Goal
Build an extension that logs all agent interactions.

## Step 1: Create Extension Class

```csharp
using Pi.Extensions;

public class LoggingExtension : IExtension
{
    public string Id => "logging-extension";
    public string Name => "Logging Extension";
    public string Version => "1.0.0";
    
    private ILogger _logger;
    
    public async Task InitializeAsync()
    {
        _logger = LoggerFactory.Create();
        _logger.LogInformation("Logging extension initialized");
    }
    
    public async Task StartAsync()
    {
        _logger.LogInformation("Logging extension started");
        
        // Subscribe to events
        EventBus.Subscribe<MessageEvent>(OnMessage);
    }
    
    public async Task StopAsync()
    {
        _logger.LogInformation("Logging extension stopped");
    }
    
    private void OnMessage(MessageEvent evt)
    {
        _logger.LogInformation(
            "Message: {Sender} -> {Content}",
            evt.Sender,
            evt.Content
        );
    }
}
```

## Step 2: Build as DLL

```bash
dotnet build
```

## Step 3: Load Extension

```csharp
var manager = new ExtensionManager("./extensions");
await manager.LoadExtensionAsync("LoggingExtension.dll");
await manager.StartExtensionAsync("logging-extension");
```

## Step 4: Test It

All agent messages will now be logged!

See [Creating Extensions](../technical/creating-extensions.md) for advanced patterns.
