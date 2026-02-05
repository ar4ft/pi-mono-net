using Microsoft.Playwright;

namespace Pi.Browser;

/// <summary>
/// Represents a browser page with automation capabilities
/// </summary>
public class BrowserPage : IAsyncDisposable
{
    private readonly IPage _page;
    
    internal BrowserPage(IPage page)
    {
        _page = page ?? throw new ArgumentNullException(nameof(page));
    }
    
    /// <summary>
    /// Navigate to a URL
    /// </summary>
    public async Task<NavigationResult> NavigateAsync(string url, int timeout = 30000)
    {
        try
        {
            var response = await _page.GotoAsync(url, new PageGotoOptions
            {
                Timeout = timeout,
                WaitUntil = WaitUntilState.DOMContentLoaded
            });
            
            return new NavigationResult
            {
                Success = response?.Ok ?? false,
                Status = response?.Status ?? 0,
                Url = _page.Url
            };
        }
        catch (Exception ex)
        {
            return new NavigationResult
            {
                Success = false,
                Error = ex.Message
            };
        }
    }
    
    /// <summary>
    /// Get page content (HTML)
    /// </summary>
    public async Task<string> GetContentAsync()
    {
        return await _page.ContentAsync();
    }
    
    /// <summary>
    /// Get page text (visible text only)
    /// </summary>
    public async Task<string> GetTextAsync()
    {
        return await _page.InnerTextAsync("body");
    }
    
    /// <summary>
    /// Get page title
    /// </summary>
    public async Task<string> GetTitleAsync()
    {
        return await _page.TitleAsync();
    }
    
    /// <summary>
    /// Take a screenshot
    /// </summary>
    public async Task<byte[]> ScreenshotAsync(ScreenshotOptions? options = null)
    {
        options ??= new ScreenshotOptions();
        
        return await _page.ScreenshotAsync(new PageScreenshotOptions
        {
            FullPage = options.FullPage,
            Type = options.Format == "jpeg" ? ScreenshotType.Jpeg : ScreenshotType.Png,
            Quality = options.Quality
        });
    }
    
    /// <summary>
    /// Click an element
    /// </summary>
    public async Task ClickAsync(string selector, int timeout = 30000)
    {
        await _page.ClickAsync(selector, new PageClickOptions { Timeout = timeout });
    }
    
    /// <summary>
    /// Type text into an input
    /// </summary>
    public async Task TypeAsync(string selector, string text, int timeout = 30000)
    {
        await _page.FillAsync(selector, text, new PageFillOptions { Timeout = timeout });
    }
    
    /// <summary>
    /// Wait for a selector to appear
    /// </summary>
    public async Task<bool> WaitForSelectorAsync(string selector, int timeout = 30000)
    {
        try
        {
            await _page.WaitForSelectorAsync(selector, new PageWaitForSelectorOptions
            {
                Timeout = timeout,
                State = WaitForSelectorState.Visible
            });
            return true;
        }
        catch
        {
            return false;
        }
    }
    
    /// <summary>
    /// Execute JavaScript in the page
    /// </summary>
    public async Task<T?> EvaluateAsync<T>(string script)
    {
        return await _page.EvaluateAsync<T>(script);
    }
    
    /// <summary>
    /// Get element text
    /// </summary>
    public async Task<string?> GetElementTextAsync(string selector)
    {
        try
        {
            return await _page.TextContentAsync(selector);
        }
        catch
        {
            return null;
        }
    }
    
    /// <summary>
    /// Get element attribute
    /// </summary>
    public async Task<string?> GetAttributeAsync(string selector, string attributeName)
    {
        try
        {
            return await _page.GetAttributeAsync(selector, attributeName);
        }
        catch
        {
            return null;
        }
    }
    
    /// <summary>
    /// Check if element exists
    /// </summary>
    public async Task<bool> ElementExistsAsync(string selector)
    {
        return await _page.Locator(selector).CountAsync() > 0;
    }
    
    /// <summary>
    /// Close the page
    /// </summary>
    public async Task CloseAsync()
    {
        await _page.CloseAsync();
    }
    
    public async ValueTask DisposeAsync()
    {
        await CloseAsync();
    }
}

/// <summary>
/// Navigation result
/// </summary>
public record NavigationResult
{
    public bool Success { get; init; }
    public int Status { get; init; }
    public string? Url { get; init; }
    public string? Error { get; init; }
}

/// <summary>
/// Screenshot options
/// </summary>
public record ScreenshotOptions
{
    /// <summary>
    /// Capture full page (including scrolled content)
    /// </summary>
    public bool FullPage { get; init; } = false;
    
    /// <summary>
    /// Image format (png or jpeg)
    /// </summary>
    public string Format { get; init; } = "png";
    
    /// <summary>
    /// JPEG quality (0-100, only for jpeg)
    /// </summary>
    public int? Quality { get; init; }
}
