namespace Pi.TUI.Components;

/// <summary>
/// Basic Markdown renderer component
/// </summary>
public class Markdown : IComponent
{
    private string _markdown;
    private string[]? _cachedLines;
    private int? _cachedWidth;

    public Markdown(string markdown)
    {
        _markdown = markdown;
    }

    public void SetMarkdown(string markdown)
    {
        _markdown = markdown;
        Invalidate();
    }

    public string[] Render(int width)
    {
        if (_cachedLines != null && _cachedWidth == width)
            return _cachedLines;

        var lines = new List<string>();
        var mdLines = _markdown.Split('\n');

        foreach (var line in mdLines)
        {
            var trimmed = line.TrimStart();
            
            // Headers
            if (trimmed.StartsWith("# "))
            {
                var text = trimmed.Substring(2);
                lines.Add($"┏━━ {text} ━━┓".PadRight(width));
            }
            else if (trimmed.StartsWith("## "))
            {
                var text = trimmed.Substring(3);
                lines.Add($"── {text} ──".PadRight(width));
            }
            else if (trimmed.StartsWith("### "))
            {
                var text = trimmed.Substring(4);
                lines.Add($"• {text}".PadRight(width));
            }
            // Lists
            else if (trimmed.StartsWith("- ") || trimmed.StartsWith("* "))
            {
                var text = trimmed.Substring(2);
                lines.Add($"  • {text}".PadRight(width));
            }
            // Code blocks
            else if (trimmed.StartsWith("```"))
            {
                lines.Add("┌─ Code ─┐".PadRight(width));
            }
            // Bold/Italic (simple replacement)
            else
            {
                var rendered = line
                    .Replace("**", "")
                    .Replace("__", "")
                    .Replace("*", "")
                    .Replace("_", "");
                
                if (rendered.Length > width)
                    rendered = rendered.Substring(0, width);
                else
                    rendered = rendered.PadRight(width);
                
                lines.Add(rendered);
            }
        }

        _cachedLines = lines.ToArray();
        _cachedWidth = width;
        return _cachedLines;
    }

    public void Invalidate()
    {
        _cachedLines = null;
        _cachedWidth = null;
    }
}
