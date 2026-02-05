# Pi.AI - LLM Integration

LLM provider integration with streaming support.

## Features
- OpenAI provider
- GitHub Copilot provider
- Streaming responses
- Token management
- OAuth device code flow

## Usage
```csharp
var provider = new GitHubCopilotProvider();
var credentials = await provider.AuthenticateAsync();

var model = new Model
{
    Id = "gpt-4",
    Provider = "github-copilot",
    BaseUrl = credentials.BaseUrl
};

await foreach (var evt in provider.Stream(model, context))
{
    if (evt is TextDeltaEvent delta)
        Console.Write(delta.Delta);
}
```

## See Also
- [Authentication Guide](../user-guide/authentication.md)
- [API Reference](../technical/api-reference.md)
