using Pi.CodingAgent.Heartbeat;

namespace Pi.CodingAgent.Skills;

/// <summary>
/// Represents a skill loaded from a SKILL.md file.
/// Skills provide specialized instructions for specific tasks.
/// </summary>
public record Skill
{
    /// <summary>
    /// Skill name (must match parent directory name, lowercase a-z, 0-9, hyphens only)
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Brief description of what the skill does
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// Full path to the SKILL.md file
    /// </summary>
    public required string FilePath { get; init; }

    /// <summary>
    /// Directory containing the skill file (parent of SKILL.md)
    /// </summary>
    public required string BaseDir { get; init; }

    /// <summary>
    /// Source of the skill (user, project, or path)
    /// </summary>
    public required string Source { get; init; }

    /// <summary>
    /// If true, skill is not included in system prompt (only invoked explicitly)
    /// </summary>
    public bool DisableModelInvocation { get; init; }

    /// <summary>
    /// Optional heartbeat configuration for this skill
    /// If present, the skill can run periodic heartbeat checks
    /// </summary>
    public HeartbeatConfig? Heartbeat { get; init; }

    /// <summary>
    /// Path to HEARTBEAT.md file if it exists
    /// </summary>
    public string? HeartbeatFilePath { get; init; }
}

/// <summary>
/// Frontmatter fields from SKILL.md YAML header
/// </summary>
public record SkillFrontmatter
{
    public string? Name { get; init; }
    public string? Description { get; init; }
    public bool? DisableModelInvocation { get; init; }
    public string? License { get; init; }
    public string? Compatibility { get; init; }
    public Dictionary<string, object>? Metadata { get; init; }
    public List<string>? AllowedTools { get; init; }
    public Dictionary<string, object>? Heartbeat { get; init; }
}

/// <summary>
/// Result of loading skills from a directory
/// </summary>
public record LoadSkillsResult
{
    public required List<Skill> Skills { get; init; }
    public required List<ResourceDiagnostic> Diagnostics { get; init; }
}

/// <summary>
/// Diagnostic message from skill loading/validation
/// </summary>
public record ResourceDiagnostic
{
    public required string Type { get; init; } // "warning", "error", "collision"
    public required string Message { get; init; }
    public required string Path { get; init; }
    public SkillCollision? Collision { get; init; }
}

/// <summary>
/// Details about a skill name collision
/// </summary>
public record SkillCollision
{
    public required string ResourceType { get; init; }
    public required string Name { get; init; }
    public required string WinnerPath { get; init; }
    public required string LoserPath { get; init; }
}
