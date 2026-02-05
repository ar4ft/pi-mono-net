using Microsoft.Playwright;

namespace Pi.Browser;

/// <summary>
/// Browser automation service using Playwright
/// </summary>
public class BrowserService
{
    private IPlaywright? _playwright;
    private IBrowser? _browser;
    private readonly BrowserConfig _config;
    private bool _initialized;
    
    public BrowserService(BrowserConfig? config = null)
    {
        _config = config ?? new BrowserConfig();
    }
    
    /// <summary>
    /// Initialize the browser (must be called before use)
    /// </summary>
    public async Task InitializeAsync()
    {
        if (_initialized)
            return;
        
        _playwright = await Playwright.CreateAsync();
        
        var launchOptions = new BrowserTypeLaunchOptions
        {
            Headless = _config.Headless,
            SlowMo = _config.SlowMo,
            Timeout = _config.Timeout
        };
        
        _browser = _config.BrowserType.ToLowerInvariant() switch
        {
            "firefox" => await _playwright.Firefox.LaunchAsync(launchOptions),
            "webkit" => await _playwright.Webkit.LaunchAsync(launchOptions),
            _ => await _playwright.Chromium.LaunchAsync(launchOptions)
        };
        
        _initialized = true;
    }
    
    /// <summary>
    /// Create a new browser page
    /// </summary>
    public async Task<BrowserPage> NewPageAsync()
    {
        if (!_initialized || _browser == null)
            throw new InvalidOperationException("Browser not initialized. Call InitializeAsync first.");
        
        var page = await _browser.NewPageAsync();
        return new BrowserPage(page);
    }
    
    /// <summary>
    /// Close the browser
    /// </summary>
    public async Task CloseAsync()
    {
        if (_browser != null)
        {
            await _browser.CloseAsync();
            _browser = null;
        }
        
        _playwright?.Dispose();
        _playwright = null;
        _initialized = false;
    }
}

/// <summary>
/// Browser configuration
/// </summary>
public record BrowserConfig
{
    /// <summary>
    /// Browser type (chromium, firefox, webkit)
    /// </summary>
    public string BrowserType { get; init; } = "chromium";
    
    /// <summary>
    /// Run in headless mode (no visible browser window)
    /// </summary>
    public bool Headless { get; init; } = true;
    
    /// <summary>
    /// Slow down operations by milliseconds (for debugging)
    /// </summary>
    public float SlowMo { get; init; } = 0;
    
    /// <summary>
    /// Default timeout in milliseconds
    /// </summary>
    public float Timeout { get; init; } = 30000;
}
