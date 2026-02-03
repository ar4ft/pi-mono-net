using Pi.CodingAgent.Heartbeat;
using Pi.CodingAgent.Soul;

namespace Pi.HeartbeatDemo;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("╔═════════════════════════════════════════╗");
        Console.WriteLine("║  Pi Heartbeat & SOUL Demo              ║");
        Console.WriteLine("╚═════════════════════════════════════════╝");
        Console.WriteLine();

        // Demo 1: Heartbeat Parser
        DemoHeartbeatParser();
        Console.WriteLine();

        // Demo 2: Heartbeat Configuration
        DemoHeartbeatConfig();
        Console.WriteLine();

        // Demo 3: SOUL.md Loading
        DemoSoulLoader();
        Console.WriteLine();

        Console.WriteLine("✓ Demo complete!");
    }

    static void DemoHeartbeatParser()
    {
        Console.WriteLine("═══ Heartbeat Parser Demo ═══");
        Console.WriteLine();

        var testCases = new[]
        {
            ("HEARTBEAT_OK", "Simple ack"),
            ("HEARTBEAT_OK - all good", "Ack at start"),
            ("Everything is fine. HEARTBEAT_OK", "Ack at end"),
            ("The system has HEARTBEAT_OK status", "Ack in middle"),
            ("⚠️ Build failed! Need attention.", "Alert (no ack)"),
            ("HEARTBEAT_OK\n\nBut here's a small note about something.", "Ack with long content"),
        };

        foreach (var (response, description) in testCases)
        {
            Console.WriteLine($"Test: {description}");
            Console.WriteLine($"Input: \"{response}\"");

            var result = HeartbeatParser.ParseResponse(response);

            Console.WriteLine($"  IsOk: {result.IsOk}");
            Console.WriteLine($"  Position: {result.Position}");
            Console.WriteLine($"  ShouldDrop: {result.ShouldDrop}");
            Console.WriteLine($"  Content: \"{result.Content}\"");
            Console.WriteLine();
        }

        // Demo interval parsing
        Console.WriteLine("Interval Parsing:");
        var intervals = new[] { "30m", "1h", "2h30m", "0m", "90s" };
        foreach (var interval in intervals)
        {
            var parsed = HeartbeatParser.ParseInterval(interval);
            Console.WriteLine($"  {interval} => {parsed}");
        }
        Console.WriteLine();

        // Demo active hours
        Console.WriteLine("Active Hours Check:");
        var activeHours = new ActiveHoursConfig { Start = "08:00", End = "18:00" };
        var isActive = HeartbeatParser.IsWithinActiveHours(activeHours);
        Console.WriteLine($"  Current time: {DateTime.Now:HH:mm}");
        Console.WriteLine($"  Active hours: 08:00 - 18:00");
        Console.WriteLine($"  Is active: {isActive}");
    }

    static void DemoHeartbeatConfig()
    {
        Console.WriteLine("═══ Heartbeat Configuration Demo ═══");
        Console.WriteLine();

        // Default config
        var defaultConfig = new HeartbeatConfig();
        Console.WriteLine("Default Configuration:");
        Console.WriteLine($"  Every: {defaultConfig.Every}");
        Console.WriteLine($"  Target: {defaultConfig.Target}");
        Console.WriteLine($"  AckMaxChars: {defaultConfig.AckMaxChars}");
        Console.WriteLine($"  IncludeReasoning: {defaultConfig.IncludeReasoning}");
        Console.WriteLine();

        // Custom config
        var customConfig = new HeartbeatConfig
        {
            Every = "1h",
            Target = "none",
            IncludeReasoning = true,
            ActiveHours = new ActiveHoursConfig
            {
                Start = "09:00",
                End = "17:00"
            },
            Model = "gpt-4"
        };

        Console.WriteLine("Custom Configuration:");
        Console.WriteLine($"  Every: {customConfig.Every}");
        Console.WriteLine($"  Target: {customConfig.Target}");
        Console.WriteLine($"  IncludeReasoning: {customConfig.IncludeReasoning}");
        Console.WriteLine($"  ActiveHours: {customConfig.ActiveHours?.Start} - {customConfig.ActiveHours?.End}");
        Console.WriteLine($"  Model: {customConfig.Model}");
    }

    static void DemoSoulLoader()
    {
        Console.WriteLine("═══ SOUL.md Loader Demo ═══");
        Console.WriteLine();

        var result = SoulLoader.LoadSoul();

        if (result.Soul != null)
        {
            Console.WriteLine("✓ SOUL.md loaded successfully");
            Console.WriteLine($"  Name: {result.Soul.Name}");
            Console.WriteLine($"  Source: {result.Soul.Source}");
            Console.WriteLine($"  FilePath: {result.Soul.FilePath}");
            Console.WriteLine();
            Console.WriteLine("Content preview (first 200 chars):");
            var preview = result.Soul.Content.Length > 200
                ? result.Soul.Content[..200] + "..."
                : result.Soul.Content;
            Console.WriteLine($"  {preview}");
            Console.WriteLine();

            // Show formatted for system prompt
            Console.WriteLine("Formatted for system prompt:");
            var formatted = SoulLoader.FormatForSystemPrompt(result.Soul);
            var formattedPreview = formatted.Length > 150
                ? formatted[..150] + "..."
                : formatted;
            Console.WriteLine($"  {formattedPreview}");
        }
        else
        {
            Console.WriteLine("✗ No SOUL.md found");
            foreach (var diag in result.Diagnostics)
            {
                Console.WriteLine($"  - {diag}");
            }
        }
    }
}
