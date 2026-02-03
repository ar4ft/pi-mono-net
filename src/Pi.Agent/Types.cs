using Pi.AI;

namespace Pi.Agent;

/// <summary>
/// Agent message that can be extended beyond standard LLM messages
/// </summary>
public interface IAgentMessage
{
    string Role { get; }
    long Timestamp { get; }
}

/// <summary>
/// Tool that agents can execute
/// </summary>
public record AgentTool
{
    public required string Name { get; init; }
    public required string Label { get; init; }
    public required string Description { get; init; }
    public required object Parameters { get; init; } // JSON Schema
    public required Func<string, Dictionary<string, object>, CancellationToken, Func<ToolExecutionUpdate, Task>?, Task<ToolExecutionResult>> Execute { get; init; }
}

/// <summary>
/// Tool execution update (for streaming progress)
/// </summary>
public record ToolExecutionUpdate
{
    public required List<object> Content { get; init; }
    public object? Details { get; init; }
}

/// <summary>
/// Tool execution result
/// </summary>
public record ToolExecutionResult
{
    public required List<object> Content { get; init; }
    public object? Details { get; init; }
}

/// <summary>
/// Agent context containing system prompt, messages, and tools
/// </summary>
public record AgentContext
{
    public string? SystemPrompt { get; init; }
    public required List<IAgentMessage> Messages { get; init; }
    public List<AgentTool>? Tools { get; init; }
}

/// <summary>
/// Configuration for the agent loop
/// </summary>
public record AgentLoopConfig
{
    public required Model Model { get; init; }
    public required Func<List<IAgentMessage>, Task<List<object>>> ConvertToLlm { get; init; }
    public Func<List<IAgentMessage>, CancellationToken, Task<List<IAgentMessage>>>? TransformContext { get; init; }
    public Func<string, Task<string?>>? GetApiKey { get; init; }
    public Func<Task<List<IAgentMessage>>>? GetSteeringMessages { get; init; }
    public Func<Task<List<IAgentMessage>>>? GetFollowUpMessages { get; init; }
    public SimpleStreamOptions? StreamOptions { get; init; }
}

/// <summary>
/// Agent state
/// </summary>
public record AgentState
{
    public required string SystemPrompt { get; init; }
    public required Model Model { get; init; }
    public required ThinkingLevel ThinkingLevel { get; init; }
    public required List<AgentTool> Tools { get; init; }
    public required List<IAgentMessage> Messages { get; init; }
    public required bool IsStreaming { get; init; }
    public IAgentMessage? StreamMessage { get; init; }
    public required HashSet<string> PendingToolCalls { get; init; }
    public string? Error { get; init; }
}
