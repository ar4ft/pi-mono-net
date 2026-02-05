namespace Pi.TUI;

/// <summary>
/// Main TUI class managing components and rendering
/// </summary>
public class TUI : Container
{
    private readonly ITerminal _terminal;
    private IComponent? _focusedComponent;
    private bool _isRunning;
    private string[]? _lastRender;
    private int _lastWidth;

    public TUI(ITerminal terminal)
    {
        _terminal = terminal;
        _lastWidth = terminal.Columns;
    }

    /// <summary>
    /// Start the TUI
    /// </summary>
    public void Start()
    {
        _isRunning = true;
        _terminal.Start(OnInput, OnResize);
        _terminal.HideCursor();
        RequestRender();
    }

    /// <summary>
    /// Stop the TUI
    /// </summary>
    public void Stop()
    {
        _isRunning = false;
        _terminal.ShowCursor();
        _terminal.Stop();
    }

    /// <summary>
    /// Request a re-render
    /// </summary>
    public void RequestRender()
    {
        if (!_isRunning) return;

        var width = _terminal.Columns;
        var lines = Render(width);

        RenderDifferential(lines, width);
        
        _lastRender = lines;
        _lastWidth = width;
    }

    private void RenderDifferential(string[] lines, int width)
    {
        // First render or width changed - full clear and render
        if (_lastRender == null || width != _lastWidth)
        {
            _terminal.ClearScreen();
            WriteLines(lines);
            return;
        }

        // Find first changed line
        int firstChanged = FindFirstChangedLine(lines, _lastRender);
        
        if (firstChanged == -1)
        {
            // No changes
            return;
        }

        // Move to first changed line
        int currentLine = _lastRender.Length;
        int linesToMove = currentLine - firstChanged;
        if (linesToMove > 0)
        {
            _terminal.MoveBy(-linesToMove);
        }

        // Clear from cursor and write new lines
        _terminal.ClearFromCursor();
        WriteLines(lines.Skip(firstChanged).ToArray());
    }

    private static int FindFirstChangedLine(string[] newLines, string[] oldLines)
    {
        int minLength = Math.Min(newLines.Length, oldLines.Length);
        
        for (int i = 0; i < minLength; i++)
        {
            if (newLines[i] != oldLines[i])
            {
                return i;
            }
        }

        // If lengths differ, first changed is at the shorter length
        if (newLines.Length != oldLines.Length)
        {
            return minLength;
        }

        return -1; // No changes
    }

    private void WriteLines(string[] lines)
    {
        foreach (var line in lines)
        {
            _terminal.Write(line);
            _terminal.Write("\r\n");
        }
    }

    private void OnInput(string data)
    {
        if (_focusedComponent != null)
        {
            _focusedComponent.HandleInput(data);
        }
        else
        {
            HandleInput(data);
        }
        
        RequestRender();
    }

    private void OnResize()
    {
        Invalidate();
        RequestRender();
    }

    /// <summary>
    /// Set the focused component
    /// </summary>
    public void SetFocus(IComponent? component)
    {
        if (_focusedComponent is IFocusable oldFocusable)
        {
            oldFocusable.Focused = false;
        }

        _focusedComponent = component;

        if (_focusedComponent is IFocusable newFocusable)
        {
            newFocusable.Focused = true;
        }

        RequestRender();
    }
}
