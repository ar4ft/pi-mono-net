namespace Pi.CodingAgent.Soul;

/// <summary>
/// Represents a SOUL.md file that defines agent personality and principles.
/// SOUL.md provides continuity, personality, and boundaries for the agent.
/// </summary>
public record Soul
{
    /// <summary>
    /// Name of the soul (typically "SOUL")
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Full path to the SOUL.md file
    /// </summary>
    public required string FilePath { get; init; }

    /// <summary>
    /// Content of the SOUL.md file
    /// </summary>
    public required string Content { get; init; }

    /// <summary>
    /// Source of the soul (workspace, agents, or custom path)
    /// </summary>
    public required string Source { get; init; }
}

/// <summary>
/// Options for loading SOUL.md files
/// </summary>
public record LoadSoulOptions
{
    /// <summary>
    /// Current working directory (workspace root)
    /// </summary>
    public string? Cwd { get; init; }

    /// <summary>
    /// Explicit path to SOUL.md file
    /// </summary>
    public string? SoulPath { get; init; }
}

/// <summary>
/// Result of loading a SOUL.md file
/// </summary>
public record LoadSoulResult
{
    /// <summary>
    /// The loaded soul, or null if not found
    /// </summary>
    public Soul? Soul { get; init; }

    /// <summary>
    /// Any diagnostics from loading
    /// </summary>
    public List<string> Diagnostics { get; init; } = new();
}
