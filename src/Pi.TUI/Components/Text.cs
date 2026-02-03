namespace Pi.TUI.Components;

/// <summary>
/// Simple text component with word wrapping
/// </summary>
public class Text : IComponent
{
    private string _text;
    private readonly int _paddingX;
    private readonly int _paddingY;
    private string[]? _cachedLines;
    private int? _cachedWidth;

    public Text(string text, int paddingX = 1, int paddingY = 1)
    {
        _text = text;
        _paddingX = paddingX;
        _paddingY = paddingY;
    }

    public void SetText(string text)
    {
        _text = text;
        Invalidate();
    }

    public string[] Render(int width)
    {
        if (_cachedLines != null && _cachedWidth == width)
        {
            return _cachedLines;
        }

        var lines = new List<string>();

        // Add top padding
        for (int i = 0; i < _paddingY; i++)
        {
            lines.Add(new string(' ', width));
        }

        // Word wrap and add text
        var contentWidth = width - (_paddingX * 2);
        if (contentWidth > 0)
        {
            var wrappedLines = WrapText(_text, contentWidth);
            foreach (var line in wrappedLines)
            {
                var paddedLine = new string(' ', _paddingX) + line + new string(' ', width - line.Length - _paddingX);
                if (paddedLine.Length > width)
                {
                    paddedLine = paddedLine.Substring(0, width);
                }
                lines.Add(paddedLine);
            }
        }

        // Add bottom padding
        for (int i = 0; i < _paddingY; i++)
        {
            lines.Add(new string(' ', width));
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

    private static string[] WrapText(string text, int width)
    {
        if (width <= 0)
            return Array.Empty<string>();

        var lines = new List<string>();
        var words = text.Split(' ');
        var currentLine = "";

        foreach (var word in words)
        {
            if (string.IsNullOrEmpty(currentLine))
            {
                currentLine = word;
            }
            else if (currentLine.Length + 1 + word.Length <= width)
            {
                currentLine += " " + word;
            }
            else
            {
                lines.Add(currentLine);
                currentLine = word;
            }
        }

        if (!string.IsNullOrEmpty(currentLine))
        {
            lines.Add(currentLine);
        }

        return lines.Count > 0 ? lines.ToArray() : new[] { "" };
    }
}
