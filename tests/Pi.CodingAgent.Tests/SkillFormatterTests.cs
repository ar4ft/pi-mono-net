using Pi.CodingAgent.Skills;

namespace Pi.CodingAgent.Tests;

public class SkillFormatterTests
{
    [Fact]
    public void FormatSkillsForPrompt_EmptyList_ReturnsEmptyString()
    {
        var skills = new List<Skill>();
        var result = SkillFormatter.FormatSkillsForPrompt(skills);
        Assert.Equal("", result);
    }

    [Fact]
    public void FormatSkillsForPrompt_SingleSkill_FormatsCorrectly()
    {
        var skills = new List<Skill>
        {
            new Skill
            {
                Name = "test-skill",
                Description = "A test skill",
                FilePath = "/path/to/skill.md",
                BaseDir = "/path/to",
                Source = "test",
                DisableModelInvocation = false
            }
        };

        var result = SkillFormatter.FormatSkillsForPrompt(skills);

        Assert.Contains("<available_skills>", result);
        Assert.Contains("<skill>", result);
        Assert.Contains("<name>test-skill</name>", result);
        Assert.Contains("<description>A test skill</description>", result);
        Assert.Contains("<location>/path/to/skill.md</location>", result);
        Assert.Contains("</skill>", result);
        Assert.Contains("</available_skills>", result);
    }

    [Fact]
    public void FormatSkillsForPrompt_ExcludesDisabledSkills()
    {
        var skills = new List<Skill>
        {
            new Skill
            {
                Name = "visible-skill",
                Description = "Visible",
                FilePath = "/path/to/visible.md",
                BaseDir = "/path/to",
                Source = "test",
                DisableModelInvocation = false
            },
            new Skill
            {
                Name = "hidden-skill",
                Description = "Hidden",
                FilePath = "/path/to/hidden.md",
                BaseDir = "/path/to",
                Source = "test",
                DisableModelInvocation = true
            }
        };

        var result = SkillFormatter.FormatSkillsForPrompt(skills);

        Assert.Contains("visible-skill", result);
        Assert.DoesNotContain("hidden-skill", result);
    }

    [Fact]
    public void FormatSkillsForPrompt_EscapesXmlCharacters()
    {
        var skills = new List<Skill>
        {
            new Skill
            {
                Name = "test-skill",
                Description = "A <test> & \"skill\"",
                FilePath = "/path/to/skill.md",
                BaseDir = "/path/to",
                Source = "test",
                DisableModelInvocation = false
            }
        };

        var result = SkillFormatter.FormatSkillsForPrompt(skills);

        Assert.Contains("A &lt;test&gt; &amp; &quot;skill&quot;", result);
    }
}
