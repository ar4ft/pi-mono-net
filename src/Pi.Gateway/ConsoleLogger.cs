using Pi.Channels;

namespace Pi.Gateway;

/// <summary>
/// Simple console logger implementation
/// </summary>
public class ConsoleLogger : Pi.Channels.ILogger
{
    private readonly string _categoryName;
    
    public ConsoleLogger(string categoryName)
    {
        _categoryName = categoryName;
    }
    
    public void LogDebug(string message, params object[] args)
    {
        Log("DEBUG", message, args);
    }
    
    public void LogInformation(string message, params object[] args)
    {
        Log("INFO", message, args);
    }
    
    public void LogWarning(string message, params object[] args)
    {
        Log("WARN", message, args);
    }
    
    public void LogError(Exception? exception, string message, params object[] args)
    {
        Log("ERROR", message, args);
        if (exception != null)
        {
            Console.WriteLine($"  Exception: {exception.Message}");
        }
    }
    
    private void Log(string level, string message, params object[] args)
    {
        var timestamp = DateTime.Now.ToString("HH:mm:ss");
        var formatted = args.Length > 0 ? string.Format(message, args) : message;
        Console.WriteLine($"[{timestamp}] [{level}] {_categoryName}: {formatted}");
    }
}
