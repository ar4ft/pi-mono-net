namespace Pi.TUI.Components;

/// <summary>
/// Box component with borders
/// </summary>
public class Box : Container
{
    private readonly string _title;
    private readonly BoxStyle _style;

    public Box(string title = "", BoxStyle style = BoxStyle.Single)
    {
        _title = title;
        _style = style;
    }

    public override string[] Render(int width)
    {
        var childLines = base.Render(width - 4); // Account for borders
        var lines = new List<string>();

        var (topLeft, topRight, bottomLeft, bottomRight, horizontal, vertical) = GetBorderChars(_style);

        // Top border
        var topBorder = topLeft + (_title.Length > 0 && _title.Length < width - 4
            ? $" {_title} ".PadRight(width - 2, horizontal[0])
            : new string(horizontal[0], width - 2)) + topRight;
        lines.Add(topBorder);

        // Content with side borders
        foreach (var line in childLines)
        {
            var paddedLine = line.Length < width - 4 
                ? line.PadRight(width - 4)
                : line.Substring(0, width - 4);
            lines.Add($"{vertical} {paddedLine} {vertical}");
        }

        // Bottom border
        lines.Add(bottomLeft + new string(horizontal[0], width - 2) + bottomRight);

        return lines.ToArray();
    }

    private static (string TopLeft, string TopRight, string BottomLeft, string BottomRight, string Horizontal, string Vertical) GetBorderChars(BoxStyle style)
    {
        return style switch
        {
            BoxStyle.Single => ("┌", "┐", "└", "┘", "─", "│"),
            BoxStyle.Double => ("╔", "╗", "╚", "╝", "═", "║"),
            BoxStyle.Rounded => ("╭", "╮", "╰", "╯", "─", "│"),
            BoxStyle.Ascii => ("+", "+", "+", "+", "-", "|"),
            _ => ("┌", "┐", "└", "┘", "─", "│")
        };
    }
}

public enum BoxStyle
{
    Single,
    Double,
    Rounded,
    Ascii
}
