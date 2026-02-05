namespace Pi.TUI.Components;

/// <summary>
/// Empty lines for vertical spacing
/// </summary>
public class Spacer : IComponent
{
    private readonly int _lines;

    public Spacer(int lines = 1)
    {
        _lines = lines;
    }

    public string[] Render(int width)
    {
        var lines = new string[_lines];
        var emptyLine = new string(' ', width);
        
        for (int i = 0; i < _lines; i++)
        {
            lines[i] = emptyLine;
        }

        return lines;
    }
}
