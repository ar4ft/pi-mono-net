using Pi.AI;

namespace Pi.Agent;

/// <summary>
/// Events emitted by the agent
/// </summary>
public abstract record AgentEvent
{
    public abstract string Type { get; }
}

public record AgentStartEvent : AgentEvent
{
    public override string Type => "agent_start";
}

public record AgentEndEvent : AgentEvent
{
    public override string Type => "agent_end";
    public required List<IAgentMessage> Messages { get; init; }
}

public record TurnStartEvent : AgentEvent
{
    public override string Type => "turn_start";
}

public record TurnEndEvent : AgentEvent
{
    public override string Type => "turn_end";
    public required IAgentMessage Message { get; init; }
    public required List<ToolResultMessage> ToolResults { get; init; }
}

public record MessageStartEvent : AgentEvent
{
    public override string Type => "message_start";
    public required IAgentMessage Message { get; init; }
}

public record MessageUpdateEvent : AgentEvent
{
    public override string Type => "message_update";
    public required IAgentMessage Message { get; init; }
    public required AssistantMessageEvent AssistantMessageEvent { get; init; }
}

public record MessageEndEvent : AgentEvent
{
    public override string Type => "message_end";
    public required IAgentMessage Message { get; init; }
}

public record ToolExecutionStartEvent : AgentEvent
{
    public override string Type => "tool_execution_start";
    public required string ToolCallId { get; init; }
    public required string ToolName { get; init; }
    public required Dictionary<string, object> Args { get; init; }
}

public record ToolExecutionUpdateEvent : AgentEvent
{
    public override string Type => "tool_execution_update";
    public required string ToolCallId { get; init; }
    public required ToolExecutionUpdate PartialResult { get; init; }
}

public record ToolExecutionEndEvent : AgentEvent
{
    public override string Type => "tool_execution_end";
    public required string ToolCallId { get; init; }
    public required ToolExecutionResult Result { get; init; }
}
