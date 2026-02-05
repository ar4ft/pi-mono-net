using Pi.Agent;
using System.Text.Json;

namespace Pi.Browser;

/// <summary>
/// Browser automation tools for agents
/// </summary>
public static class BrowserTools
{
    /// <summary>
    /// Create browser automation tools for agents
    /// </summary>
    public static List<AgentTool> CreateTools(BrowserService browserService)
    {
        return new List<AgentTool>
        {
            CreateNavigateTool(browserService),
            CreateScreenshotTool(browserService),
            CreateClickTool(browserService),
            CreateTypeTool(browserService),
            CreateGetTextTool(browserService)
        };
    }
    
    private static AgentTool CreateNavigateTool(BrowserService browserService)
    {
        return new AgentTool
        {
            Name = "browser_navigate",
            Label = "Navigate Browser",
            Description = "Navigate to a URL and load the page",
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    url = new
                    {
                        type = "string",
                        description = "The URL to navigate to"
                    }
                },
                required = new[] { "url" }
            },
            Execute = async (id, args, ct, onUpdate) =>
            {
                var url = args["url"].ToString() ?? throw new ArgumentException("URL required");
                
                var page = await browserService.NewPageAsync();
                var result = await page.NavigateAsync(url);
                
                return new ToolExecutionResult
                {
                    Content = new List<object>
                    {
                        new
                        {
                            type = "text",
                            text = result.Success
                                ? $"Successfully navigated to {result.Url}"
                                : $"Failed to navigate: {result.Error}"
                        }
                    }
                };
            }
        };
    }
    
    private static AgentTool CreateScreenshotTool(BrowserService browserService)
    {
        return new AgentTool
        {
            Name = "browser_screenshot",
            Label = "Take Screenshot",
            Description = "Take a screenshot of the current page",
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    full_page = new
                    {
                        type = "boolean",
                        description = "Capture full page including scrolled content (default: false)"
                    }
                }
            },
            Execute = async (id, args, ct, onUpdate) =>
            {
                var fullPage = args.ContainsKey("full_page") && bool.Parse(args["full_page"].ToString() ?? "false");
                
                var page = await browserService.NewPageAsync();
                var screenshot = await page.ScreenshotAsync(new ScreenshotOptions
                {
                    FullPage = fullPage,
                    Format = "png"
                });
                
                var base64 = Convert.ToBase64String(screenshot);
                
                return new ToolExecutionResult
                {
                    Content = new List<object>
                    {
                        new
                        {
                            type = "image",
                            source = new
                            {
                                type = "base64",
                                media_type = "image/png",
                                data = base64
                            }
                        }
                    }
                };
            }
        };
    }
    
    private static AgentTool CreateClickTool(BrowserService browserService)
    {
        return new AgentTool
        {
            Name = "browser_click",
            Label = "Click Element",
            Description = "Click an element on the page by selector",
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    selector = new
                    {
                        type = "string",
                        description = "CSS selector for the element to click"
                    }
                },
                required = new[] { "selector" }
            },
            Execute = async (id, args, ct, onUpdate) =>
            {
                var selector = args["selector"].ToString() ?? throw new ArgumentException("Selector required");
                
                var page = await browserService.NewPageAsync();
                await page.ClickAsync(selector);
                
                return new ToolExecutionResult
                {
                    Content = new List<object>
                    {
                        new
                        {
                            type = "text",
                            text = $"Clicked element: {selector}"
                        }
                    }
                };
            }
        };
    }
    
    private static AgentTool CreateTypeTool(BrowserService browserService)
    {
        return new AgentTool
        {
            Name = "browser_type",
            Label = "Type Text",
            Description = "Type text into an input field",
            Parameters = new
            {
                type = "object",
                properties = new
                {
                    selector = new
                    {
                        type = "string",
                        description = "CSS selector for the input field"
                    },
                    text = new
                    {
                        type = "string",
                        description = "Text to type"
                    }
                },
                required = new[] { "selector", "text" }
            },
            Execute = async (id, args, ct, onUpdate) =>
            {
                var selector = args["selector"].ToString() ?? throw new ArgumentException("Selector required");
                var text = args["text"].ToString() ?? throw new ArgumentException("Text required");
                
                var page = await browserService.NewPageAsync();
                await page.TypeAsync(selector, text);
                
                return new ToolExecutionResult
                {
                    Content = new List<object>
                    {
                        new
                        {
                            type = "text",
                            text = $"Typed text into {selector}"
                        }
                    }
                };
            }
        };
    }
    
    private static AgentTool CreateGetTextTool(BrowserService browserService)
    {
        return new AgentTool
        {
            Name = "browser_get_text",
            Label = "Get Page Text",
            Description = "Get visible text from the current page",
            Parameters = new
            {
                type = "object",
                properties = new object()
            },
            Execute = async (id, args, ct, onUpdate) =>
            {
                var page = await browserService.NewPageAsync();
                var text = await page.GetTextAsync();
                
                return new ToolExecutionResult
                {
                    Content = new List<object>
                    {
                        new
                        {
                            type = "text",
                            text = text
                        }
                    }
                };
            }
        };
    }
}
