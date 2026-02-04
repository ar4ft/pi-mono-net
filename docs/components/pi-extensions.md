# Pi.Extensions - Plugin System

Plugin system for extending functionality.

## Features
- Dynamic assembly loading
- Extension lifecycle management
- Metadata and versioning
- Load, unload, enable, disable

## Usage
```csharp
var manager = new ExtensionManager("./extensions");

await manager.LoadExtensionAsync("MyExtension.dll");
await manager.StartExtensionAsync("my-extension");

// List extensions
var extensions = manager.GetExtensions();

// Stop and unload
await manager.StopExtensionAsync("my-extension");
manager.UnloadExtension("my-extension");
```

## See Also
- [Creating Extensions](../technical/creating-extensions.md)
