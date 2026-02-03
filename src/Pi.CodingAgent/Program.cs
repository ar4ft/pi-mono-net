using Pi.AI;
using Pi.AI.Providers;
using Pi.Agent;
using Pi.TUI;
using Pi.TUI.Components;

Console.WriteLine("╔══════════════════════════════════════════╗");
Console.WriteLine("║  Pi Coding Agent - Interactive CLI      ║");
Console.WriteLine("╚══════════════════════════════════════════╝");
Console.WriteLine();

// Initialize CLI
var cli = new CodingAgentCLI();
await cli.RunAsync();

/// <summary>
/// Main CLI class with REPL loop
/// </summary>
public class CodingAgentCLI
{
    private readonly List<string> _messageHistory = new();
    private readonly List<IAgentMessage> _agentMessages = new();
    private Model? _currentModel;
    private bool _running = true;

    public async Task RunAsync()
    {
        ShowWelcome();
        ShowHelp();

        while (_running)
        {
            Console.Write("\n> ");
            var input = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(input))
                continue;

            _messageHistory.Add(input);

            if (input.StartsWith("/"))
            {
                await HandleCommand(input);
            }
            else
            {
                await HandleMessage(input);
            }
        }
    }

    private void ShowWelcome()
    {
        Console.WriteLine("Welcome to Pi Coding Agent!");
        Console.WriteLine("Type your questions or commands.");
        Console.WriteLine();
    }

    private void ShowHelp()
    {
        Console.WriteLine("Available commands:");
        Console.WriteLine("  /help      - Show this help");
        Console.WriteLine("  /models    - List available models");
        Console.WriteLine("  /model     - Show current model");
        Console.WriteLine("  /clear     - Clear conversation history");
        Console.WriteLine("  /history   - Show message history");
        Console.WriteLine("  /exit      - Exit the CLI");
        Console.WriteLine();
    }

    private async Task HandleCommand(string command)
    {
        var parts = command.Split(' ', 2);
        var cmd = parts[0].ToLower();
        var args = parts.Length > 1 ? parts[1] : "";

        switch (cmd)
        {
            case "/help":
                ShowHelp();
                break;

            case "/models":
                ShowModels();
                break;

            case "/model":
                if (string.IsNullOrWhiteSpace(args))
                {
                    ShowCurrentModel();
                }
                else
                {
                    SetModel(args);
                }
                break;

            case "/clear":
                ClearHistory();
                break;

            case "/history":
                ShowHistory();
                break;

            case "/exit":
            case "/quit":
            case "/q":
                _running = false;
                Console.WriteLine("Goodbye!");
                break;

            default:
                Console.WriteLine($"Unknown command: {cmd}");
                Console.WriteLine("Type /help for available commands.");
                break;
        }

        await Task.CompletedTask;
    }

    private async Task HandleMessage(string message)
    {
        if (_currentModel == null)
        {
            Console.WriteLine("⚠ No model selected. Using default (requires GitHub Copilot authentication)...");
            Console.WriteLine();
            
            // Try to authenticate with GitHub Copilot
            try
            {
                var provider = new GitHubCopilotProvider();
                var credentials = await provider.AuthenticateAsync();
                
                _currentModel = new Model
                {
                    Id = "gpt-4",
                    Name = "GPT-4 (GitHub Copilot)",
                    Api = "openai-completions",
                    Provider = "github-copilot",
                    BaseUrl = Auth.GitHubCopilotAuth.GetBaseUrlFromToken(credentials.CopilotToken),
                    Reasoning = false,
                    Input = new List<string> { "text" },
                    Cost = new ModelCost { Input = 0, Output = 0, CacheRead = 0, CacheWrite = 0 },
                    ContextWindow = 128000,
                    MaxTokens = 4096
                };
                
                Console.WriteLine($"✓ Using model: {_currentModel.Name}\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Authentication failed: {ex.Message}");
                Console.WriteLine("Please authenticate or set a different model.");
                return;
            }
        }

        Console.WriteLine($"\nProcessing: {message}");
        Console.WriteLine("─────────────────────────────────────────");
        
        // For now, just echo back
        // In a full implementation, this would use the Agent class
        Console.WriteLine($"\n[Agent]: Your message was: {message}");
        Console.WriteLine("\n(Note: Full agent integration coming soon)");
        
        await Task.CompletedTask;
    }

    private void ShowModels()
    {
        Console.WriteLine("\nAvailable Models:");
        Console.WriteLine("  1. GPT-4 (GitHub Copilot) - Requires authentication");
        Console.WriteLine("  2. GPT-3.5-Turbo (OpenAI) - Requires API key");
        Console.WriteLine("  3. Claude 3.5 Sonnet (Anthropic) - Requires API key");
        Console.WriteLine();
        Console.WriteLine("Use /model <name> to select a model");
    }

    private void ShowCurrentModel()
    {
        if (_currentModel == null)
        {
            Console.WriteLine("\n⚠ No model currently selected");
        }
        else
        {
            Console.WriteLine($"\nCurrent model: {_currentModel.Name}");
            Console.WriteLine($"  Provider: {_currentModel.Provider}");
            Console.WriteLine($"  Context window: {_currentModel.ContextWindow} tokens");
        }
    }

    private void SetModel(string modelName)
    {
        Console.WriteLine($"\n⚠ Model selection not yet implemented: {modelName}");
        Console.WriteLine("Using default (GitHub Copilot) on first message.");
    }

    private void ClearHistory()
    {
        _messageHistory.Clear();
        _agentMessages.Clear();
        Console.WriteLine("\n✓ Conversation history cleared");
    }

    private void ShowHistory()
    {
        Console.WriteLine($"\nMessage History ({_messageHistory.Count} messages):");
        for (int i = 0; i < _messageHistory.Count; i++)
        {
            Console.WriteLine($"  {i + 1}. {_messageHistory[i]}");
        }
    }
}
