using Pi.IMessage;

Console.WriteLine("╔════════════════════════════════════════════╗");
Console.WriteLine("║  Pi iMessage Integration Demo             ║");
Console.WriteLine("╚════════════════════════════════════════════╝");
Console.WriteLine();

// Check platform
if (!OperatingSystem.IsMacOS())
{
    Console.WriteLine("❌ Error: iMessage integration requires macOS");
    Console.WriteLine();
    Console.WriteLine("This demo can only run on macOS with Messages.app installed.");
    Console.WriteLine();
    Console.WriteLine("Demo Features (macOS only):");
    Console.WriteLine("  1. Recent Messages - View last 5 messages");
    Console.WriteLine("  2. Message Normalization - Convert to agent format");
    Console.WriteLine("  3. Message Monitor - Real-time message watching");
    Console.WriteLine("  4. Send Messages - AppleScript integration");
    Console.WriteLine();
    Console.WriteLine("Documentation: See README.md for setup instructions");
    return 1;
}

Console.WriteLine("Pi iMessage Integration Demo");
Console.WriteLine("============================");
Console.WriteLine();

var config = new IMessageConfig
{
    DatabasePath = "~/Library/Messages/chat.db",
    PollingIntervalSeconds = 2,
    AgentName = "demo",
    ProcessGroupMessages = true,
    ProcessDirectMessages = true,
    MaxMessagesPerPoll = 10
};

Console.WriteLine("Configuration:");
Console.WriteLine($"  Database: {config.DatabasePath}");
Console.WriteLine($"  Polling: Every {config.PollingIntervalSeconds}s");
Console.WriteLine($"  Agent: {config.AgentName}");
Console.WriteLine();

var dbPath = Environment.ExpandEnvironmentVariables(
    config.DatabasePath.Replace("~", Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)));

if (!File.Exists(dbPath))
{
    Console.WriteLine($"❌ Error: iMessage database not found at: {dbPath}");
    Console.WriteLine();
    Console.WriteLine("Possible solutions:");
    Console.WriteLine("  1. Ensure Messages.app has been opened at least once");
    Console.WriteLine("  2. Grant Full Disk Access to Terminal in System Settings");
    Console.WriteLine("  3. Check that the database path is correct");
    return 1;
}

Console.WriteLine($"✓ Found iMessage database");
Console.WriteLine();

try
{
    using var database = new IMessageDatabase(config.DatabasePath);
    var processor = new IMessageProcessor(config);
    
    Console.WriteLine("✓ Components initialized");
    Console.WriteLine();

    var recentMessages = await database.GetRecentMessagesAsync(5);
    Console.WriteLine($"Found {recentMessages.Count} recent message(s)");
    
    if (recentMessages.Count > 0)
    {
        var normalized = processor.NormalizeMessage(recentMessages.First());
        Console.WriteLine($"Session Key Example: {normalized.SessionKey}");
    }
    
    Console.WriteLine();
    Console.WriteLine("✓ Demo completed successfully");
    return 0;
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Error: {ex.Message}");
    return 1;
}
