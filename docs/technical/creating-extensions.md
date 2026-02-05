# Creating Extensions
Develop plugins and extensions for Pi-Mono-Net.

## What are Extensions?
Extensions are plugins that add custom functionality.

## IExtension Interface
```csharp
public interface IExtension
{
    string Id { get; }
    string Name { get; }
    string Version { get; }
    
    Task InitializeAsync();
    Task StartAsync();
    Task StopAsync();
}
```

## Example: Custom Extension
```csharp
public class MyExtension : IExtension
{
    public string Id => "my-extension";
    public string Name => "My Extension";
    public string Version => "1.0.0";
    
    public async Task InitializeAsync()
    {
        // Setup
    }
    
    public async Task StartAsync()
    {
        // Start background tasks
    }
    
    public async Task StopAsync()
    {
        // Cleanup
    }
}
```

## Loading Extensions
```csharp
var manager = new ExtensionManager("./extensions");
await manager.LoadExtensionAsync("MyExtension.dll");
await manager.StartExtensionAsync("my-extension");
```

## Extension Lifecycle
1. Load assembly
2. Initialize
3. Start
4. Stop
5. Unload

See [API Reference](api-reference.md) for more details.
