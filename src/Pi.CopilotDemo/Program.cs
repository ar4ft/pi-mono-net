using Pi.AI;
using Pi.AI.Providers;
using Pi.AI.Auth;

Console.WriteLine("╔═══════════════════════════════════════╗");
Console.WriteLine("║  GitHub Copilot .NET Demo            ║");
Console.WriteLine("╚═══════════════════════════════════════╝");
Console.WriteLine();

// Create GitHub Copilot provider (default)
var provider = new GitHubCopilotProvider();

// Authenticate
Console.WriteLine("Starting authentication...");
GitHubCopilotCredentials credentials;

try
{
    credentials = await provider.AuthenticateAsync();
}
catch (Exception ex)
{
    Console.WriteLine($"\n❌ Authentication failed: {ex.Message}");
    return;
}

// Create a model for Copilot
var model = new Model
{
    Id = "gpt-4",
    Name = "GPT-4 (GitHub Copilot)",
    Api = "openai-completions",
    Provider = "github-copilot",
    BaseUrl = GitHubCopilotAuth.GetBaseUrlFromToken(credentials.CopilotToken),
    Reasoning = false,
    Input = new List<string> { "text" },
    Cost = new ModelCost { Input = 0, Output = 0, CacheRead = 0, CacheWrite = 0 }, // Free with subscription
    ContextWindow = 128000,
    MaxTokens = 4096
};

Console.WriteLine($"\n✓ Using model: {model.Name}");
Console.WriteLine($"✓ Base URL: {model.BaseUrl}");

// Create context
var context = new Context
{
    SystemPrompt = "You are a helpful AI assistant powered by GitHub Copilot.",
    Messages = new List<object>
    {
        new UserMessage
        {
            Content = "Hello! Can you write a C# function to calculate fibonacci numbers?",
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        }
    }
};

Console.WriteLine("\n─────────────────────────────────────");
Console.WriteLine("Sending request to GitHub Copilot...");
Console.WriteLine("─────────────────────────────────────\n");

try
{
    var fullResponse = "";
    await foreach (var evt in provider.Stream(model, context))
    {
        switch (evt)
        {
            case TextStartEvent:
                Console.Write("Assistant: ");
                break;
            
            case TextDeltaEvent delta:
                Console.Write(delta.Delta);
                fullResponse += delta.Delta;
                break;
            
            case DoneEvent done:
                Console.WriteLine($"\n\n─────────────────────────────────────");
                Console.WriteLine($"✓ Response complete");
                Console.WriteLine($"  Tokens used: {done.Message.Usage.TotalTokens}");
                Console.WriteLine($"  Input: {done.Message.Usage.Input}");
                Console.WriteLine($"  Output: {done.Message.Usage.Output}");
                Console.WriteLine($"  Cost: ${done.Message.Usage.Cost.Total:F4}");
                break;
            
            case ErrorEvent error:
                Console.WriteLine($"\n\n❌ Error: {error.Error.ErrorMessage}");
                break;
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine($"\n❌ Error during streaming: {ex.Message}");
}

Console.WriteLine("\n✓ Demo complete!");
