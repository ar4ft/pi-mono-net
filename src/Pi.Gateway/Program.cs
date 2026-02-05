using Pi.Gateway;

var logger = new ConsoleLogger("Gateway");

logger.LogInformation("Pi Gateway starting...");

var gateway = new GatewayService(logger);

try
{
    await gateway.StartAsync();
    
    logger.LogInformation("Gateway running. Press Ctrl+C to exit.");
    
    // Keep running until cancelled
    var tcs = new TaskCompletionSource();
    Console.CancelKeyPress += (s, e) =>
    {
        e.Cancel = true;
        tcs.SetResult();
    };
    
    await tcs.Task;
    
    await gateway.StopAsync();
}
catch (Exception ex)
{
    logger.LogError(ex, "Fatal error in gateway");
    return 1;
}

logger.LogInformation("Gateway stopped");
return 0;
