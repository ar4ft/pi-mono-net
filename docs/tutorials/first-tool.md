# Build Your First Tool

Create a custom agent tool in 10 minutes.

## Goal
Build a tool that gets cryptocurrency prices.

## Step 1: Create the Tool Class

```csharp
using Pi.Agent;

public class CryptoPriceTool : ITool
{
    public string Name => "get_crypto_price";
    
    public string Description => 
        "Get current cryptocurrency price in USD. " +
        "Use when user asks about crypto prices.";
    
    public object Parameters => new
    {
        type = "object",
        properties = new
        {
            symbol = new
            {
                type = "string",
                description = "Crypto symbol (BTC, ETH, etc.)"
            }
        },
        required = new[] { "symbol" }
    };
    
    public async Task<ToolResult> ExecuteAsync(
        string toolCallId,
        Dictionary<string, object> arguments,
        CancellationToken cancellationToken)
    {
        var symbol = arguments["symbol"].ToString();
        var price = await GetPriceAsync(symbol);
        
        return ToolResult.Success(toolCallId, 
            $"{symbol}: ${price:N2}");
    }
    
    private async Task<decimal> GetPriceAsync(string symbol)
    {
        // Call crypto API (simplified)
        return 50000m; // Mock price
    }
}
```

## Step 2: Register the Tool

```csharp
var tools = new List<ITool>
{
    new CryptoPriceTool(),
    // ... other tools
};

var agent = new Agent(model, ConvertToLlm, tools);
```

## Step 3: Test It

```
> What's the current Bitcoin price?

Agent uses tool: get_crypto_price(symbol: "BTC")
Result: BTC: $50,000.00
```

## Next Steps
- Add error handling
- Implement real API call
- Add more parameters (currency, timeframe)

See [Creating Tools](../technical/creating-tools.md) for advanced patterns.
