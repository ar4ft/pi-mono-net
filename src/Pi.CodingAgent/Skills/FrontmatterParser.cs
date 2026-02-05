using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Pi.CodingAgent.Skills;

/// <summary>
/// Parses YAML frontmatter from markdown files
/// </summary>
public static class FrontmatterParser
{
    private static readonly IDeserializer YamlDeserializer = new DeserializerBuilder()
        .WithNamingConvention(HyphenatedNamingConvention.Instance)
        .IgnoreUnmatchedProperties()
        .Build();

    /// <summary>
    /// Parse frontmatter from markdown content
    /// </summary>
    public static (SkillFrontmatter Frontmatter, string Body) ParseFrontmatter(string content)
    {
        var normalized = NormalizeNewlines(content);
        var (yamlString, body) = ExtractFrontmatter(normalized);

        if (yamlString == null)
        {
            return (new SkillFrontmatter(), body);
        }

        try
        {
            var frontmatter = YamlDeserializer.Deserialize<SkillFrontmatter>(yamlString);
            return (frontmatter ?? new SkillFrontmatter(), body);
        }
        catch
        {
            return (new SkillFrontmatter(), body);
        }
    }

    /// <summary>
    /// Strip frontmatter and return only the body
    /// </summary>
    public static string StripFrontmatter(string content)
    {
        return ParseFrontmatter(content).Body;
    }

    /// <summary>
    /// Get all frontmatter keys (for validation)
    /// </summary>
    public static List<string> GetFrontmatterKeys(string content)
    {
        var normalized = NormalizeNewlines(content);
        var (yamlString, _) = ExtractFrontmatter(normalized);

        if (yamlString == null)
        {
            return new List<string>();
        }

        try
        {
            var dict = YamlDeserializer.Deserialize<Dictionary<string, object>>(yamlString);
            return dict?.Keys.ToList() ?? new List<string>();
        }
        catch
        {
            return new List<string>();
        }
    }

    private static string NormalizeNewlines(string value)
    {
        return value.Replace("\r\n", "\n").Replace("\r", "\n");
    }

    private static (string? YamlString, string Body) ExtractFrontmatter(string normalized)
    {
        if (!normalized.StartsWith("---"))
        {
            return (null, normalized);
        }

        var endIndex = normalized.IndexOf("\n---", 3, StringComparison.Ordinal);
        if (endIndex == -1)
        {
            return (null, normalized);
        }

        var yamlString = normalized.Substring(4, endIndex - 4);
        var body = normalized.Substring(endIndex + 4).Trim();

        return (yamlString, body);
    }
}
