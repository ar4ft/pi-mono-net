# Pi.Browser - Browser Automation

Browser automation with Playwright integration.

## Features
- Chromium, Firefox, WebKit support
- Headless and headed modes
- Screenshot capture
- Element interaction
- JavaScript execution

## Usage
```csharp
var browser = new BrowserService(new BrowserConfig
{
    BrowserType = "chromium",
    Headless = true
});

await browser.InitializeAsync();

var page = await browser.NewPageAsync();
await page.NavigateAsync("https://example.com");
var screenshot = await page.ScreenshotAsync();
var text = await page.GetTextAsync();
```

## Agent Tools
- `browser_navigate` - Navigate to URLs
- `browser_screenshot` - Capture screenshots
- `browser_click` - Click elements
- `browser_type` - Type into inputs
- `browser_get_text` - Extract text

## See Also
- [Creating Tools](../technical/creating-tools.md)
