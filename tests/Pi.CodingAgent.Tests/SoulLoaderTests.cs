using Xunit;
using FluentAssertions;

namespace Pi.CodingAgent.Tests;

public class SoulLoaderTests
{
    [Fact]
    public void LoadSoul_CreatesResultWithDiagnostics()
    {
        // Act
        var result = Pi.CodingAgent.Soul.SoulLoader.LoadSoul();

        // Assert
        result.Should().NotBeNull();
        result.Diagnostics.Should().NotBeNull();
    }

    [Fact]
    public void FormatForSystemPrompt_WrapsContentInSoulTags()
    {
        // Arrange
        var soul = new Pi.CodingAgent.Soul.Soul
        {
            Name = "SOUL",
            FilePath = "/test/SOUL.md",
            Content = "Test content",
            Source = "test"
        };

        // Act
        var formatted = Pi.CodingAgent.Soul.SoulLoader.FormatForSystemPrompt(soul);

        // Assert
        formatted.Should().Contain("<soul>");
        formatted.Should().Contain("</soul>");
        formatted.Should().Contain("Test content");
    }

    [Fact]
    public void LoadSoul_WithExplicitPath_LoadsFromPath()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        var tempMdFile = Path.ChangeExtension(tempFile, ".md");
        File.Move(tempFile, tempMdFile);
        
        try
        {
            File.WriteAllText(tempMdFile, "Test soul content");
            var options = new Pi.CodingAgent.Soul.LoadSoulOptions { SoulPath = tempMdFile };

            // Act
            var result = Pi.CodingAgent.Soul.SoulLoader.LoadSoul(options);

            // Assert
            result.Soul.Should().NotBeNull();
            result.Soul!.Content.Should().Be("Test soul content");
            result.Soul.Source.Should().Be("custom");
        }
        finally
        {
            if (File.Exists(tempMdFile))
                File.Delete(tempMdFile);
        }
    }

    [Fact]
    public void LoadSoul_WithNonExistentPath_ReturnsNull()
    {
        // Arrange
        var options = new Pi.CodingAgent.Soul.LoadSoulOptions { SoulPath = "/nonexistent/path/SOUL.md" };

        // Act
        var result = Pi.CodingAgent.Soul.SoulLoader.LoadSoul(options);

        // Assert
        result.Soul.Should().BeNull();
        result.Diagnostics.Should().NotBeEmpty();
    }
}
