namespace Pi.CodingAgent.Heartbeat;

/// <summary>
/// Configuration for periodic heartbeat checks.
/// Heartbeats run periodic agent turns to surface items that need attention.
/// </summary>
public record HeartbeatConfig
{
    /// <summary>
    /// Interval between heartbeats (e.g., "30m", "1h", "0m" to disable)
    /// Default: 30m (or 1h for some providers)
    /// </summary>
    public string Every { get; init; } = "30m";

    /// <summary>
    /// Where heartbeat messages should be delivered
    /// Options: "last" (last channel), "none", or specific channel id
    /// </summary>
    public string Target { get; init; } = "last";

    /// <summary>
    /// Custom prompt for heartbeat runs
    /// Default: "Read HEARTBEAT.md if it exists (workspace context). Follow it strictly. 
    /// Do not infer or repeat old tasks from prior chats. If nothing needs attention, reply HEARTBEAT_OK."
    /// </summary>
    public string Prompt { get; init; } = 
        "Read HEARTBEAT.md if it exists (workspace context). Follow it strictly. " +
        "Do not infer or repeat old tasks from prior chats. If nothing needs attention, reply HEARTBEAT_OK.";

    /// <summary>
    /// If true, deliver separate reasoning message when available
    /// </summary>
    public bool IncludeReasoning { get; init; } = false;

    /// <summary>
    /// Maximum characters allowed after HEARTBEAT_OK before message is dropped
    /// Default: 300
    /// </summary>
    public int AckMaxChars { get; init; } = 300;

    /// <summary>
    /// Active hours for heartbeat (local time)
    /// If set, heartbeats only run within this time window
    /// </summary>
    public ActiveHoursConfig? ActiveHours { get; init; }

    /// <summary>
    /// Optional channel-specific recipient override
    /// </summary>
    public string? To { get; init; }

    /// <summary>
    /// Optional model override for heartbeat runs
    /// </summary>
    public string? Model { get; init; }
}

/// <summary>
/// Time window for active hours (local time)
/// </summary>
public record ActiveHoursConfig
{
    /// <summary>
    /// Start time in 24-hour format (e.g., "08:00")
    /// </summary>
    public required string Start { get; init; }

    /// <summary>
    /// End time in 24-hour format (e.g., "24:00")
    /// </summary>
    public required string End { get; init; }
}

/// <summary>
/// Result of parsing a heartbeat response
/// </summary>
public record HeartbeatResponse
{
    /// <summary>
    /// Whether HEARTBEAT_OK was found in the response
    /// </summary>
    public required bool IsOk { get; init; }

    /// <summary>
    /// Position where HEARTBEAT_OK was found (Start, End, Middle, or None)
    /// </summary>
    public required HeartbeatOkPosition Position { get; init; }

    /// <summary>
    /// The response text after stripping HEARTBEAT_OK
    /// </summary>
    public required string Content { get; init; }

    /// <summary>
    /// Whether this response should be dropped (HEARTBEAT_OK + minimal content)
    /// </summary>
    public required bool ShouldDrop { get; init; }
}

/// <summary>
/// Position of HEARTBEAT_OK in the response
/// </summary>
public enum HeartbeatOkPosition
{
    None,
    Start,
    End,
    Middle
}
