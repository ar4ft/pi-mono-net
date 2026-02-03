using Pi.CodingAgent.Heartbeat;
using Xunit;
using FluentAssertions;

namespace Pi.CodingAgent.Tests;

public class HeartbeatParserTests
{
    [Fact]
    public void ParseResponse_SimpleHeartbeatOk_ReturnsOkAtStart()
    {
        // Arrange
        var response = "HEARTBEAT_OK";

        // Act
        var result = HeartbeatParser.ParseResponse(response);

        // Assert
        result.IsOk.Should().BeTrue();
        result.Position.Should().Be(HeartbeatOkPosition.Start);
        result.ShouldDrop.Should().BeTrue();
        result.Content.Should().BeEmpty();
    }

    [Fact]
    public void ParseResponse_HeartbeatOkAtStart_StripsToken()
    {
        // Arrange
        var response = "HEARTBEAT_OK - all systems operational";

        // Act
        var result = HeartbeatParser.ParseResponse(response);

        // Assert
        result.IsOk.Should().BeTrue();
        result.Position.Should().Be(HeartbeatOkPosition.Start);
        result.Content.Should().Be("- all systems operational");
        result.ShouldDrop.Should().BeTrue();
    }

    [Fact]
    public void ParseResponse_HeartbeatOkAtEnd_StripsToken()
    {
        // Arrange
        var response = "Everything is fine. HEARTBEAT_OK";

        // Act
        var result = HeartbeatParser.ParseResponse(response);

        // Assert
        result.IsOk.Should().BeTrue();
        result.Position.Should().Be(HeartbeatOkPosition.End);
        result.Content.Should().Be("Everything is fine.");
        result.ShouldDrop.Should().BeTrue();
    }

    [Fact]
    public void ParseResponse_HeartbeatOkInMiddle_DoesNotStrip()
    {
        // Arrange
        var response = "The system has HEARTBEAT_OK status and is running.";

        // Act
        var result = HeartbeatParser.ParseResponse(response);

        // Assert
        result.IsOk.Should().BeTrue();
        result.Position.Should().Be(HeartbeatOkPosition.Middle);
        result.Content.Should().Be(response);
        result.ShouldDrop.Should().BeFalse();
    }

    [Fact]
    public void ParseResponse_NoHeartbeatOk_ReturnsNotOk()
    {
        // Arrange
        var response = "⚠️ Build failed! Need attention.";

        // Act
        var result = HeartbeatParser.ParseResponse(response);

        // Assert
        result.IsOk.Should().BeFalse();
        result.Position.Should().Be(HeartbeatOkPosition.None);
        result.Content.Should().Be(response);
        result.ShouldDrop.Should().BeFalse();
    }

    [Fact]
    public void ParseResponse_HeartbeatOkWithLongContent_DoesNotDrop()
    {
        // Arrange
        var longContent = new string('a', 400);
        var response = $"HEARTBEAT_OK\n\n{longContent}";

        // Act
        var result = HeartbeatParser.ParseResponse(response, ackMaxChars: 300);

        // Assert
        result.IsOk.Should().BeTrue();
        result.Position.Should().Be(HeartbeatOkPosition.Start);
        result.ShouldDrop.Should().BeFalse();
        result.Content.Length.Should().BeGreaterThan(300);
    }

    [Fact]
    public void ParseResponse_EmptyString_ReturnsNotOk()
    {
        // Arrange
        var response = "";

        // Act
        var result = HeartbeatParser.ParseResponse(response);

        // Assert
        result.IsOk.Should().BeFalse();
        result.Position.Should().Be(HeartbeatOkPosition.None);
        result.ShouldDrop.Should().BeFalse();
    }

    [Theory]
    [InlineData("30m", 30)]
    [InlineData("1h", 60)]
    [InlineData("2h", 120)]
    [InlineData("2h30m", 150)]
    [InlineData("90s", 1.5)]
    [InlineData("0m", 0)]
    public void ParseInterval_ValidInterval_ReturnsCorrectTimeSpan(string interval, double expectedMinutes)
    {
        // Act
        var result = HeartbeatParser.ParseInterval(interval);

        // Assert
        result.Should().NotBeNull();
        result!.Value.TotalMinutes.Should().BeApproximately(expectedMinutes, 0.01);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("invalid")]
    public void ParseInterval_InvalidInterval_ReturnsNull(string? interval)
    {
        // Act
        var result = HeartbeatParser.ParseInterval(interval!);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void IsWithinActiveHours_NoActiveHours_ReturnsTrue()
    {
        // Act
        var result = HeartbeatParser.IsWithinActiveHours(null);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsWithinActiveHours_WithinHours_ReturnsTrue()
    {
        // Arrange - create time window that includes current time
        var now = DateTime.Now;
        var start = now.AddHours(-1).ToString("HH:mm");
        var end = now.AddHours(1).ToString("HH:mm");
        var activeHours = new ActiveHoursConfig { Start = start, End = end };

        // Act
        var result = HeartbeatParser.IsWithinActiveHours(activeHours);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsWithinActiveHours_OutsideHours_ReturnsFalse()
    {
        // Arrange - create time window that does not include current time
        var now = DateTime.Now;
        var start = now.AddHours(2).ToString("HH:mm");
        var end = now.AddHours(3).ToString("HH:mm");
        var activeHours = new ActiveHoursConfig { Start = start, End = end };

        // Act
        var result = HeartbeatParser.IsWithinActiveHours(activeHours);

        // Assert
        result.Should().BeFalse();
    }
}
