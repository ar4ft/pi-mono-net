# Creating Agent Tools
Build custom tools that extend agent capabilities.

## What are Agent Tools?
Tools are functions that agents can call to perform actions.

## Tool Structure
```csharp
public class MyTool : ITool
{
    public string Name => "my_tool";
    public string Description => "What it does";
    public object Parameters => new { /* JSON Schema */ };
    
    public async Task<ToolResult> ExecuteAsync(...)
    {
        // Implementation
    }
}
```

## Example: Weather Tool
```csharp
public class WeatherTool : ITool
{
    public string Name => "get_weather";
    public string Description => 
        "Get current weather for a location";
    
    public object Parameters => new
    {
        type = "object",
        properties = new
        {
            location = new
            {
                type = "string",
                description = "City name"
            }
        },
        required = new[] { "location" }
    };
    
    public async Task<ToolResult> ExecuteAsync(
        string toolCallId,
        Dictionary<string, object> arguments,
        CancellationToken cancellationToken)
    {
        var location = arguments["location"].ToString();
        var weather = await GetWeatherAsync(location);
        return ToolResult.Success(toolCallId, weather);
    }
}
```

## Registering Tools
```csharp
var tools = new List<ITool>
{
    new WeatherTool(),
    new MyCustomTool()
};

var agent = new Agent(model, ConvertToLlm, tools);
```

## Best Practices
- Clear descriptions
- Detailed parameters
- Error handling
- Cancellation support
- Input validation

See [API Reference](api-reference.md) for more details.
