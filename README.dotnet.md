# Pi Monorepo - .NET Port

> **This is a .NET port of the pi-mono TypeScript repository.**

Tools for building AI agents and managing LLM deployments, now available in C# and .NET.

## Getting Started

### Prerequisites

- [.NET 8.0 SDK](https://dotnet.net/download/dotnet/8.0) or later
- Windows, macOS, or Linux

### Building

```bash
# Build all projects
dotnet build

# Build specific project
dotnet build src/Pi.AI/Pi.AI.csproj

# Run tests
dotnet test
```

## Projects

| Project | Description | Status |
|---------|-------------|--------|
| **Pi.AI** | Unified multi-provider LLM API (OpenAI, Anthropic, Google, etc.) | ✅ Core types implemented |
| **Pi.TUI** | Terminal UI library with differential rendering | ✅ Core framework implemented |
| **Pi.Agent** | Agent runtime with tool calling and state management | ✅ Core types implemented |
| **Pi.CodingAgent** | Interactive coding agent CLI | 🚧 In progress |
| **Pi.MOM** | Slack bot that delegates messages to the pi coding agent | 🚧 In progress |
| **Pi.Pods** | CLI for managing vLLM deployments on GPU pods | 🚧 In progress |
| **Pi.WebUI** | Web components for AI chat interfaces | 🚧 In progress |

## Architecture

The .NET port follows these design decisions:

- **Target Framework**: .NET 8.0 (LTS)
- **Solution Structure**: Single solution with multiple projects
- **Package Management**: NuGet packages for distribution
- **Testing**: xUnit testing framework
- **Async**: Task-based async/await patterns
- **Terminal UI**: Custom implementation using System.Console
- **HTTP**: HttpClient with System.Net.Http
- **JSON**: System.Text.Json for serialization

## Project Structure

```
pi-mono-net/
├── src/                    # Source projects
│   ├── Pi.AI/             # LLM integration library
│   ├── Pi.TUI/            # Terminal UI framework
│   ├── Pi.Agent/          # Agent runtime
│   ├── Pi.CodingAgent/    # Coding agent CLI
│   ├── Pi.MOM/            # Slack bot
│   ├── Pi.Pods/           # GPU pod management
│   └── Pi.WebUI/          # Web UI components
├── tests/                  # Test projects
│   ├── Pi.AI.Tests/
│   ├── Pi.TUI.Tests/
│   └── Pi.Agent.Tests/
└── docs/                   # Documentation

## Example Usage

### Pi.AI - LLM Integration

```csharp
using Pi.AI;

// Create a model
var model = new Model
{
    Id = "gpt-4",
    Name = "GPT-4",
    Api = "openai-completions",
    Provider = "openai",
    BaseUrl = "https://api.openai.com/v1",
    Reasoning = false,
    Input = new List<string> { "text" },
    Cost = new ModelCost
    {
        Input = 30.0,
        Output = 60.0,
        CacheRead = 0,
        CacheWrite = 0
    },
    ContextWindow = 128000,
    MaxTokens = 4096
};

// Create a context
var context = new Context
{
    SystemPrompt = "You are a helpful assistant.",
    Messages = new List<object>
    {
        new UserMessage
        {
            Content = "Hello!",
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        }
    }
};
```

### Pi.TUI - Terminal UI

```csharp
using Pi.TUI;
using Pi.TUI.Components;

// Create terminal
var terminal = new ConsoleTerminal();

// Create TUI
var tui = new TUI(terminal);

// Add components
tui.AddChild(new Text("Welcome to Pi!"));
tui.AddChild(new Spacer());

var input = new Input();
input.OnSubmit = (text) =>
{
    tui.AddChild(new Text($"You said: {text}"));
};
tui.AddChild(input);
tui.SetFocus(input);

// Start
tui.Start();
```

### Pi.Agent - Agent Runtime

```csharp
using Pi.Agent;
using Pi.AI;

// Create agent
var agent = new Agent(model, async messages =>
{
    // Convert agent messages to LLM messages
    return messages
        .Where(m => m.Role == "user" || m.Role == "assistant")
        .ToList<object>();
});

// Subscribe to events
agent.Subscribe(evt =>
{
    Console.WriteLine($"Event: {evt.Type}");
});

// Set system prompt and tools
agent.SetSystemPrompt("You are a helpful coding assistant.");

// Prompt the agent
await agent.Prompt("Hello, write a function to calculate fibonacci numbers");
```

## Development

### Running Tests

```bash
dotnet test
```

### Adding Dependencies

```bash
# Add a NuGet package to a project
cd src/Pi.AI
dotnet add package Newtonsoft.Json
```

### Creating a New Project

```bash
# Create a new class library
dotnet new classlib -n Pi.NewLibrary -o src/Pi.NewLibrary -f net8.0

# Add to solution (if solution file exists)
dotnet sln add src/Pi.NewLibrary/Pi.NewLibrary.csproj
```

## Differences from TypeScript Version

While we strive for feature parity, there are some differences due to platform capabilities:

1. **Terminal Handling**: .NET's Console API has different capabilities than Node.js process.stdin/stdout
2. **Async Patterns**: Uses Task-based async/await instead of Promise
3. **Type System**: C# strong typing vs TypeScript's structural typing
4. **Package Management**: NuGet instead of npm

## Contributing

Contributions are welcome! Please see [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines.

## License

MIT

## Original TypeScript Repository

This is a port of [badlogic/pi-mono](https://github.com/badlogic/pi-mono).
