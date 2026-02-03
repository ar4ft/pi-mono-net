using System.Text;

namespace Pi.CodingAgent.Skills;

/// <summary>
/// Formats skills for inclusion in a system prompt using XML format per Agent Skills spec
/// </summary>
public static class SkillFormatter
{
    /// <summary>
    /// Format skills for inclusion in a system prompt.
    /// Uses XML format per Agent Skills standard.
    /// Skills with disableModelInvocation=true are excluded from the prompt
    /// (they can only be invoked explicitly via /skill:name commands).
    /// </summary>
    public static string FormatSkillsForPrompt(List<Skill> skills)
    {
        var visibleSkills = skills.Where(s => !s.DisableModelInvocation).ToList();

        if (visibleSkills.Count == 0)
        {
            return "";
        }

        var sb = new StringBuilder();
        sb.AppendLine();
        sb.AppendLine();
        sb.AppendLine("The following skills provide specialized instructions for specific tasks.");
        sb.AppendLine("Use the read tool to load a skill's file when the task matches its description.");
        sb.AppendLine("When a skill file references a relative path, resolve it against the skill directory (parent of SKILL.md / dirname of the path) and use that absolute path in tool commands.");
        sb.AppendLine();
        sb.AppendLine("<available_skills>");

        foreach (var skill in visibleSkills)
        {
            sb.AppendLine("  <skill>");
            sb.AppendLine($"    <name>{EscapeXml(skill.Name)}</name>");
            sb.AppendLine($"    <description>{EscapeXml(skill.Description)}</description>");
            sb.AppendLine($"    <location>{EscapeXml(skill.FilePath)}</location>");
            sb.AppendLine("  </skill>");
        }

        sb.AppendLine("</available_skills>");

        return sb.ToString();
    }

    private static string EscapeXml(string str)
    {
        return str
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;")
            .Replace("'", "&apos;");
    }
}
