# Pi.TUI - Terminal UI Framework

Terminal UI framework with component system.

## Features
- Component-based architecture
- Differential rendering
- Focus management
- Built-in components (Text, Input, Editor, Box, etc.)

## Usage
```csharp
var terminal = new ConsoleTerminal();
var tui = new TUI(terminal);

tui.AddChild(new Text("Hello, World!"));

var input = new Input();
input.OnSubmit = text => {
    tui.AddChild(new Text($"You: {text}"));
};

tui.SetFocus(input);
tui.Start();
```

## Components
- Text, Spacer, Input
- Editor (multi-line)
- Box (with borders)
- Markdown, Loader, SelectList

## See Also
- [Quick Start](../user-guide/quick-start.md)
