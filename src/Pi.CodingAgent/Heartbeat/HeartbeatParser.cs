using System.Text.RegularExpressions;

namespace Pi.CodingAgent.Heartbeat;

/// <summary>
/// Parser for heartbeat responses.
/// Handles HEARTBEAT_OK recognition and response processing.
/// </summary>
public static class HeartbeatParser
{
    private const string HeartbeatOkToken = "HEARTBEAT_OK";
    private static readonly Regex HeartbeatOkRegex = new(
        @"\bHEARTBEAT_OK\b",
        RegexOptions.Compiled | RegexOptions.IgnoreCase
    );

    /// <summary>
    /// Parse a heartbeat response to determine if it's an ack or alert
    /// </summary>
    /// <param name="response">The response text</param>
    /// <param name="ackMaxChars">Maximum chars allowed after HEARTBEAT_OK for drop</param>
    /// <returns>Parsed heartbeat response</returns>
    public static HeartbeatResponse ParseResponse(string response, int ackMaxChars = 300)
    {
        if (string.IsNullOrWhiteSpace(response))
        {
            return new HeartbeatResponse
            {
                IsOk = false,
                Position = HeartbeatOkPosition.None,
                Content = response ?? "",
                ShouldDrop = false
            };
        }

        var trimmed = response.Trim();
        var match = HeartbeatOkRegex.Match(trimmed);

        if (!match.Success)
        {
            // No HEARTBEAT_OK found
            return new HeartbeatResponse
            {
                IsOk = false,
                Position = HeartbeatOkPosition.None,
                Content = response,
                ShouldDrop = false
            };
        }

        // Determine position
        var position = DeterminePosition(trimmed, match);
        
        // Strip HEARTBEAT_OK
        var content = StripHeartbeatOk(trimmed, match, position);
        
        // Determine if should drop
        var shouldDrop = ShouldDropMessage(position, content, ackMaxChars);

        return new HeartbeatResponse
        {
            IsOk = true,
            Position = position,
            Content = content,
            ShouldDrop = shouldDrop
        };
    }

    /// <summary>
    /// Check if currently within active hours
    /// </summary>
    public static bool IsWithinActiveHours(ActiveHoursConfig? activeHours)
    {
        if (activeHours == null)
            return true;

        var now = DateTime.Now;
        var currentTime = now.TimeOfDay;

        if (!TimeSpan.TryParse(activeHours.Start, out var start))
            return true;

        if (!TimeSpan.TryParse(activeHours.End, out var end))
            return true;

        // Handle case where end time is next day (e.g., 22:00 to 02:00)
        if (end < start)
        {
            return currentTime >= start || currentTime <= end;
        }

        return currentTime >= start && currentTime <= end;
    }

    /// <summary>
    /// Parse interval string to TimeSpan (e.g., "30m", "1h", "2h30m")
    /// </summary>
    public static TimeSpan? ParseInterval(string interval)
    {
        if (string.IsNullOrWhiteSpace(interval))
            return null;

        interval = interval.Trim().ToLower();

        if (interval == "0m" || interval == "0")
            return TimeSpan.Zero; // Disabled

        // Match patterns like "30m", "1h", "2h30m", "90s"
        var minutesMatch = Regex.Match(interval, @"(\d+)m");
        var hoursMatch = Regex.Match(interval, @"(\d+)h");
        var secondsMatch = Regex.Match(interval, @"(\d+)s");

        var totalMinutes = 0;
        var totalHours = 0;
        var totalSeconds = 0;

        if (minutesMatch.Success)
            totalMinutes = int.Parse(minutesMatch.Groups[1].Value);

        if (hoursMatch.Success)
            totalHours = int.Parse(hoursMatch.Groups[1].Value);

        if (secondsMatch.Success)
            totalSeconds = int.Parse(secondsMatch.Groups[1].Value);

        if (totalMinutes == 0 && totalHours == 0 && totalSeconds == 0)
            return null;

        return TimeSpan.FromHours(totalHours) + TimeSpan.FromMinutes(totalMinutes) + TimeSpan.FromSeconds(totalSeconds);
    }

    private static HeartbeatOkPosition DeterminePosition(string text, Match match)
    {
        var index = match.Index;
        var length = match.Length;

        // Check if at start (allowing whitespace)
        var beforeText = text[..index].Trim();
        if (beforeText.Length == 0)
            return HeartbeatOkPosition.Start;

        // Check if at end (allowing whitespace)
        var afterText = text[(index + length)..].Trim();
        if (afterText.Length == 0)
            return HeartbeatOkPosition.End;

        // Must be in the middle
        return HeartbeatOkPosition.Middle;
    }

    private static string StripHeartbeatOk(string text, Match match, HeartbeatOkPosition position)
    {
        // Only strip if at start or end
        if (position == HeartbeatOkPosition.Middle)
            return text;

        // Remove the matched HEARTBEAT_OK
        var result = text.Remove(match.Index, match.Length);
        return result.Trim();
    }

    private static bool ShouldDropMessage(HeartbeatOkPosition position, string content, int ackMaxChars)
    {
        // Only drop if HEARTBEAT_OK is at start or end AND remaining content is minimal
        if (position == HeartbeatOkPosition.Middle)
            return false;

        if (position == HeartbeatOkPosition.None)
            return false;

        // Drop if content is empty or very short
        return content.Length <= ackMaxChars;
    }
}
