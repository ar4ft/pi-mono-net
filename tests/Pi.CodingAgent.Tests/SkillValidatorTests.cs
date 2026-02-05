using Pi.CodingAgent.Skills;

namespace Pi.CodingAgent.Tests;

public class SkillValidatorTests
{
    [Fact]
    public void ValidateName_ValidName_ReturnsNoErrors()
    {
        var errors = SkillValidator.ValidateName("my-skill", "my-skill");
        Assert.Empty(errors);
    }

    [Fact]
    public void ValidateName_NameMismatch_ReturnsError()
    {
        var errors = SkillValidator.ValidateName("different-name", "my-skill");
        Assert.Single(errors);
        Assert.Contains("does not match parent directory", errors[0]);
    }

    [Fact]
    public void ValidateName_TooLong_ReturnsError()
    {
        var longName = new string('a', 65);
        var errors = SkillValidator.ValidateName(longName, longName);
        Assert.Contains(errors, e => e.Contains("exceeds 64 characters"));
    }

    [Fact]
    public void ValidateName_InvalidCharacters_ReturnsError()
    {
        var errors = SkillValidator.ValidateName("My_Skill!", "My_Skill!");
        Assert.Contains(errors, e => e.Contains("invalid characters"));
    }

    [Fact]
    public void ValidateName_StartsWithHyphen_ReturnsError()
    {
        var errors = SkillValidator.ValidateName("-my-skill", "-my-skill");
        Assert.Contains(errors, e => e.Contains("must not start or end with a hyphen"));
    }

    [Fact]
    public void ValidateName_EndsWithHyphen_ReturnsError()
    {
        var errors = SkillValidator.ValidateName("my-skill-", "my-skill-");
        Assert.Contains(errors, e => e.Contains("must not start or end with a hyphen"));
    }

    [Fact]
    public void ValidateName_ConsecutiveHyphens_ReturnsError()
    {
        var errors = SkillValidator.ValidateName("my--skill", "my--skill");
        Assert.Contains(errors, e => e.Contains("consecutive hyphens"));
    }

    [Fact]
    public void ValidateDescription_ValidDescription_ReturnsNoErrors()
    {
        var errors = SkillValidator.ValidateDescription("A valid description");
        Assert.Empty(errors);
    }

    [Fact]
    public void ValidateDescription_NullDescription_ReturnsError()
    {
        var errors = SkillValidator.ValidateDescription(null);
        Assert.Single(errors);
        Assert.Contains("description is required", errors[0]);
    }

    [Fact]
    public void ValidateDescription_EmptyDescription_ReturnsError()
    {
        var errors = SkillValidator.ValidateDescription("   ");
        Assert.Single(errors);
        Assert.Contains("description is required", errors[0]);
    }

    [Fact]
    public void ValidateDescription_TooLong_ReturnsError()
    {
        var longDesc = new string('a', 1025);
        var errors = SkillValidator.ValidateDescription(longDesc);
        Assert.Contains(errors, e => e.Contains("exceeds 1024 characters"));
    }

    [Fact]
    public void ValidateFrontmatterFields_AllowedFields_ReturnsNoErrors()
    {
        var keys = new List<string> { "name", "description", "license" };
        var errors = SkillValidator.ValidateFrontmatterFields(keys);
        Assert.Empty(errors);
    }

    [Fact]
    public void ValidateFrontmatterFields_UnknownField_ReturnsError()
    {
        var keys = new List<string> { "name", "author", "version" };
        var errors = SkillValidator.ValidateFrontmatterFields(keys);
        Assert.Equal(2, errors.Count);
        Assert.Contains(errors, e => e.Contains("unknown frontmatter field \"author\""));
        Assert.Contains(errors, e => e.Contains("unknown frontmatter field \"version\""));
    }
}
