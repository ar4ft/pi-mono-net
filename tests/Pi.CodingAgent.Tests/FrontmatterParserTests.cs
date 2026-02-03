using Pi.CodingAgent.Skills;

namespace Pi.CodingAgent.Tests;

public class FrontmatterParserTests
{
    [Fact]
    public void ParseFrontmatter_ValidYaml_ParsesCorrectly()
    {
        var content = @"---
name: test-skill
description: A test skill
---

# Test Content";

        var (frontmatter, body) = FrontmatterParser.ParseFrontmatter(content);

        Assert.Equal("test-skill", frontmatter.Name);
        Assert.Equal("A test skill", frontmatter.Description);
        Assert.Equal("# Test Content", body);
    }

    [Fact]
    public void ParseFrontmatter_NoFrontmatter_ReturnsEmptyFrontmatter()
    {
        var content = "# Just content";

        var (frontmatter, body) = FrontmatterParser.ParseFrontmatter(content);

        Assert.Null(frontmatter.Name);
        Assert.Null(frontmatter.Description);
        Assert.Equal("# Just content", body);
    }

    [Fact]
    public void ParseFrontmatter_DisableModelInvocation_ParsesCorrectly()
    {
        var content = @"---
name: test-skill
description: A test skill
disable-model-invocation: true
---

Content";

        var (frontmatter, body) = FrontmatterParser.ParseFrontmatter(content);

        Assert.Equal("test-skill", frontmatter.Name);
        Assert.True(frontmatter.DisableModelInvocation);
    }

    [Fact]
    public void StripFrontmatter_RemovesFrontmatter()
    {
        var content = @"---
name: test-skill
description: A test skill
---

# Content";

        var body = FrontmatterParser.StripFrontmatter(content);

        Assert.Equal("# Content", body);
    }

    [Fact]
    public void GetFrontmatterKeys_ReturnsAllKeys()
    {
        var content = @"---
name: test-skill
description: A test skill
license: MIT
---

Content";

        var keys = FrontmatterParser.GetFrontmatterKeys(content);

        Assert.Contains("name", keys);
        Assert.Contains("description", keys);
        Assert.Contains("license", keys);
        Assert.Equal(3, keys.Count);
    }
}
