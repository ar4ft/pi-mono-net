using Pi.Gateway;
using Pi.IMessage;

Console.WriteLine("╔═══════════════════════════════════════════╗");
Console.WriteLine("║  Pi Gateway + iMessage Integration Demo  ║");
Console.WriteLine("╚═══════════════════════════════════════════╝");
Console.WriteLine();

var logger = new ConsoleLogger("Demo");

// Create gateway
logger.LogInformation("Creating Gateway...");
var gateway = new GatewayService(logger);

// Create iMessage channel
var config = new IMessageConfig
{
    DatabasePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        "Library", "Messages", "chat.db"),
    PollingIntervalSeconds = 2,
    AgentName = "main"
};

logger.LogInformation("Creating iMessage channel...");
var imessageChannel = new IMessageChannel(config, logger);

try
{
    // Register iMessage with Gateway
    logger.LogInformation("Registering iMessage channel with Gateway...");
    await gateway.RegisterChannelAsync(imessageChannel);
    
    // Start gateway
    logger.LogInformation("Starting Gateway...");
    await gateway.StartAsync();
    
    Console.WriteLine();
    Console.WriteLine("✓ Gateway running with iMessage channel");
    Console.WriteLine("✓ Monitoring Messages.app for new messages");
    Console.WriteLine();
    Console.WriteLine("Statistics:");
    var stats = gateway.GetStatistics();
    Console.WriteLine($"  Total Channels: {stats.TotalChannels}");
    Console.WriteLine($"  Active Channels: {stats.ActiveChannels}");
    Console.WriteLine($"  Active Sessions: {stats.ActiveSessions}");
    Console.WriteLine();
    Console.WriteLine("Send a message via iMessage to see it processed...");
    Console.WriteLine();
    Console.WriteLine("Press Ctrl+C to exit.");
    
    // Keep running
    var tcs = new TaskCompletionSource();
    Console.CancelKeyPress += (s, e) =>
    {
        e.Cancel = true;
        tcs.SetResult();
    };
    
    await tcs.Task;
    
    Console.WriteLine();
    logger.LogInformation("Stopping Gateway...");
    await gateway.StopAsync();
}
catch (FileNotFoundException ex) when (ex.Message.Contains("chat.db"))
{
    Console.WriteLine();
    Console.WriteLine("⚠️  Messages database not found!");
    Console.WriteLine("   This demo requires macOS with Messages.app.");
    Console.WriteLine("   Database location: ~/Library/Messages/chat.db");
    Console.WriteLine();
    Console.WriteLine("   To run this demo:");
    Console.WriteLine("   1. Make sure you're on macOS");
    Console.WriteLine("   2. Grant Terminal Full Disk Access");
    Console.WriteLine("      (System Settings → Privacy & Security → Full Disk Access)");
    Console.WriteLine("   3. Ensure Messages.app has some message history");
    return 1;
}
catch (Exception ex)
{
    logger.LogError(ex, "Fatal error");
    return 1;
}

logger.LogInformation("Gateway stopped");
Console.WriteLine("Goodbye!");
return 0;
