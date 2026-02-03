namespace Pi.AI;

/// <summary>
/// Known API types supported by the system
/// </summary>
public enum KnownApi
{
    OpenAICompletions,
    OpenAIResponses,
    AzureOpenAIResponses,
    OpenAICodexResponses,
    AnthropicMessages,
    BedrockConverseStream,
    GoogleGenerativeAI,
    GoogleGeminiCli,
    GoogleVertex
}

/// <summary>
/// Known LLM providers
/// </summary>
public enum KnownProvider
{
    AmazonBedrock,
    Anthropic,
    Google,
    GoogleGeminiCli,
    GoogleAntigravity,
    GoogleVertex,
    OpenAI,
    AzureOpenAIResponses,
    OpenAICodex,
    GitHubCopilot,
    XAI,
    Groq,
    Cerebras,
    OpenRouter,
    VercelAIGateway,
    ZAI,
    Mistral,
    MiniMax,
    MiniMaxCN,
    HuggingFace,
    OpenCode,
    KimiCoding
}

/// <summary>
/// Thinking/reasoning level for models that support extended reasoning
/// </summary>
public enum ThinkingLevel
{
    Off,
    Minimal,
    Low,
    Medium,
    High,
    XHigh
}

/// <summary>
/// Token budgets for each thinking level (token-based providers only)
/// </summary>
public record ThinkingBudgets
{
    public int? Minimal { get; init; }
    public int? Low { get; init; }
    public int? Medium { get; init; }
    public int? High { get; init; }
}

/// <summary>
/// Cache retention preference for prompt caching
/// </summary>
public enum CacheRetention
{
    None,
    Short,
    Long
}

/// <summary>
/// Options for streaming LLM requests
/// </summary>
public record StreamOptions
{
    public double? Temperature { get; init; }
    public int? MaxTokens { get; init; }
    public CancellationToken CancellationToken { get; init; } = CancellationToken.None;
    public string? ApiKey { get; init; }
    public CacheRetention CacheRetention { get; init; } = CacheRetention.Short;
    public string? SessionId { get; init; }
    public Action<object>? OnPayload { get; init; }
    public Dictionary<string, string>? Headers { get; init; }
    public int MaxRetryDelayMs { get; init; } = 60000;
}

/// <summary>
/// Stream options with thinking/reasoning support
/// </summary>
public record SimpleStreamOptions : StreamOptions
{
    public ThinkingLevel? Reasoning { get; init; }
    public ThinkingBudgets? ThinkingBudgets { get; init; }
}

/// <summary>
/// Text content block
/// </summary>
public record TextContent
{
    public string Type => "text";
    public required string Text { get; init; }
    public string? TextSignature { get; init; }
}

/// <summary>
/// Thinking/reasoning content block
/// </summary>
public record ThinkingContent
{
    public string Type => "thinking";
    public required string Thinking { get; init; }
    public string? ThinkingSignature { get; init; }
}

/// <summary>
/// Image content block
/// </summary>
public record ImageContent
{
    public string Type => "image";
    public required string Data { get; init; } // base64 encoded
    public required string MimeType { get; init; }
}

/// <summary>
/// Tool call content block
/// </summary>
public record ToolCall
{
    public string Type => "toolCall";
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required Dictionary<string, object> Arguments { get; init; }
    public string? ThoughtSignature { get; init; }
}

/// <summary>
/// Token usage and cost information
/// </summary>
public record Usage
{
    public required int Input { get; init; }
    public required int Output { get; init; }
    public required int CacheRead { get; init; }
    public required int CacheWrite { get; init; }
    public required int TotalTokens { get; init; }
    public required UsageCost Cost { get; init; }
}

public record UsageCost
{
    public required double Input { get; init; }
    public required double Output { get; init; }
    public required double CacheRead { get; init; }
    public required double CacheWrite { get; init; }
    public required double Total { get; init; }
}

/// <summary>
/// Reason why the LLM stopped generating
/// </summary>
public enum StopReason
{
    Stop,
    Length,
    ToolUse,
    Error,
    Aborted
}

/// <summary>
/// User message
/// </summary>
public record UserMessage
{
    public string Role => "user";
    public required object Content { get; init; } // string or List<TextContent | ImageContent>
    public required long Timestamp { get; init; }
}

/// <summary>
/// Assistant/LLM response message
/// </summary>
public record AssistantMessage
{
    public string Role => "assistant";
    public required List<object> Content { get; init; } // List<TextContent | ThinkingContent | ToolCall>
    public required string Api { get; init; }
    public required string Provider { get; init; }
    public required string Model { get; init; }
    public required Usage Usage { get; init; }
    public required StopReason StopReason { get; init; }
    public string? ErrorMessage { get; init; }
    public required long Timestamp { get; init; }
}

/// <summary>
/// Tool execution result message
/// </summary>
public record ToolResultMessage
{
    public string Role => "toolResult";
    public required string ToolCallId { get; init; }
    public required string ToolName { get; init; }
    public required List<object> Content { get; init; } // List<TextContent | ImageContent>
    public object? Details { get; init; }
    public required bool IsError { get; init; }
    public required long Timestamp { get; init; }
}

/// <summary>
/// Tool definition
/// </summary>
public record Tool
{
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required object Parameters { get; init; } // JSON Schema
}

/// <summary>
/// Context for LLM requests (system prompt, messages, tools)
/// </summary>
public record Context
{
    public string? SystemPrompt { get; init; }
    public required List<object> Messages { get; init; } // List of UserMessage | AssistantMessage | ToolResultMessage
    public List<Tool>? Tools { get; init; }
}

/// <summary>
/// Model definition
/// </summary>
public record Model
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string Api { get; init; }
    public required string Provider { get; init; }
    public required string BaseUrl { get; init; }
    public required bool Reasoning { get; init; }
    public required List<string> Input { get; init; } // ["text", "image"]
    public required ModelCost Cost { get; init; }
    public required int ContextWindow { get; init; }
    public required int MaxTokens { get; init; }
    public Dictionary<string, string>? Headers { get; init; }
}

public record ModelCost
{
    public required double Input { get; init; } // $/million tokens
    public required double Output { get; init; }
    public required double CacheRead { get; init; }
    public required double CacheWrite { get; init; }
}
