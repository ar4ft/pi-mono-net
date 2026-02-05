# Pi.Agent - Agent Runtime

Agent execution loop with tool calling support.

## Features
- Turn-based execution
- Tool calling
- Event streaming
- Context transformation
- Max turn limits

## Usage
```csharp
var agent = new Agent(model, ConvertToLlm, tools);

await foreach (var evt in agent.RunAsync(systemPrompt, messages, tools))
{
    switch (evt)
    {
        case MessageUpdateEvent update:
            Console.Write(update.Delta);
            break;
        case ToolExecutionStartEvent toolStart:
            Console.WriteLine($"Tool: {toolStart.ToolName}");
            break;
    }
}
```

## See Also
- [Creating Tools](../technical/creating-tools.md)
- [Architecture](../technical/architecture.md)
