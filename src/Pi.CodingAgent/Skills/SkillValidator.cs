namespace Pi.CodingAgent.Skills;

/// <summary>
/// Validates skill names and descriptions per Agent Skills spec
/// </summary>
public static class SkillValidator
{
    private const int MaxNameLength = 64;
    private const int MaxDescriptionLength = 1024;
    
    private static readonly HashSet<string> AllowedFrontmatterFields = new()
    {
        "name",
        "description",
        "license",
        "compatibility",
        "metadata",
        "allowed-tools",
        "disable-model-invocation"
    };

    /// <summary>
    /// Validate skill name per Agent Skills spec.
    /// Returns array of validation error messages (empty if valid).
    /// </summary>
    public static List<string> ValidateName(string name, string parentDirName)
    {
        var errors = new List<string>();

        if (name != parentDirName)
        {
            errors.Add($"name \"{name}\" does not match parent directory \"{parentDirName}\"");
        }

        if (name.Length > MaxNameLength)
        {
            errors.Add($"name exceeds {MaxNameLength} characters ({name.Length})");
        }

        if (!System.Text.RegularExpressions.Regex.IsMatch(name, "^[a-z0-9-]+$"))
        {
            errors.Add("name contains invalid characters (must be lowercase a-z, 0-9, hyphens only)");
        }

        if (name.StartsWith("-") || name.EndsWith("-"))
        {
            errors.Add("name must not start or end with a hyphen");
        }

        if (name.Contains("--"))
        {
            errors.Add("name must not contain consecutive hyphens");
        }

        return errors;
    }

    /// <summary>
    /// Validate description per Agent Skills spec.
    /// </summary>
    public static List<string> ValidateDescription(string? description)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(description))
        {
            errors.Add("description is required");
        }
        else if (description.Length > MaxDescriptionLength)
        {
            errors.Add($"description exceeds {MaxDescriptionLength} characters ({description.Length})");
        }

        return errors;
    }

    /// <summary>
    /// Check for unknown frontmatter fields.
    /// </summary>
    public static List<string> ValidateFrontmatterFields(List<string> keys)
    {
        var errors = new List<string>();
        foreach (var key in keys)
        {
            if (!AllowedFrontmatterFields.Contains(key))
            {
                errors.Add($"unknown frontmatter field \"{key}\"");
            }
        }
        return errors;
    }
}
