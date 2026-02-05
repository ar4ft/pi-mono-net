namespace Pi.AI;

/// <summary>
/// Events emitted during assistant message streaming
/// </summary>
public abstract record AssistantMessageEvent
{
    public abstract string Type { get; }
}

public record AssistantMessageStartEvent : AssistantMessageEvent
{
    public override string Type => "start";
    public required AssistantMessage Partial { get; init; }
}

public record TextStartEvent : AssistantMessageEvent
{
    public override string Type => "text_start";
    public required int ContentIndex { get; init; }
    public required AssistantMessage Partial { get; init; }
}

public record TextDeltaEvent : AssistantMessageEvent
{
    public override string Type => "text_delta";
    public required int ContentIndex { get; init; }
    public required string Delta { get; init; }
    public required AssistantMessage Partial { get; init; }
}

public record TextEndEvent : AssistantMessageEvent
{
    public override string Type => "text_end";
    public required int ContentIndex { get; init; }
    public required string Content { get; init; }
    public required AssistantMessage Partial { get; init; }
}

public record ThinkingStartEvent : AssistantMessageEvent
{
    public override string Type => "thinking_start";
    public required int ContentIndex { get; init; }
    public required AssistantMessage Partial { get; init; }
}

public record ThinkingDeltaEvent : AssistantMessageEvent
{
    public override string Type => "thinking_delta";
    public required int ContentIndex { get; init; }
    public required string Delta { get; init; }
    public required AssistantMessage Partial { get; init; }
}

public record ThinkingEndEvent : AssistantMessageEvent
{
    public override string Type => "thinking_end";
    public required int ContentIndex { get; init; }
    public required string Content { get; init; }
    public required AssistantMessage Partial { get; init; }
}

public record ToolCallStartEvent : AssistantMessageEvent
{
    public override string Type => "toolcall_start";
    public required int ContentIndex { get; init; }
    public required AssistantMessage Partial { get; init; }
}

public record ToolCallDeltaEvent : AssistantMessageEvent
{
    public override string Type => "toolcall_delta";
    public required int ContentIndex { get; init; }
    public required string Delta { get; init; }
    public required AssistantMessage Partial { get; init; }
}

public record ToolCallEndEvent : AssistantMessageEvent
{
    public override string Type => "toolcall_end";
    public required int ContentIndex { get; init; }
    public required ToolCall ToolCall { get; init; }
    public required AssistantMessage Partial { get; init; }
}

public record DoneEvent : AssistantMessageEvent
{
    public override string Type => "done";
    public required StopReason Reason { get; init; } // stop, length, or toolUse
    public required AssistantMessage Message { get; init; }
}

public record ErrorEvent : AssistantMessageEvent
{
    public override string Type => "error";
    public required StopReason Reason { get; init; } // aborted or error
    public required AssistantMessage Error { get; init; }
}
