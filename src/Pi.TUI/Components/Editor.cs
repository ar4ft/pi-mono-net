using System.Text;

namespace Pi.TUI.Components;

/// <summary>
/// Multi-line text editor component with cursor management
/// </summary>
public class Editor : IComponent, IFocusable
{
    private readonly List<string> _lines;
    private int _cursorRow;
    private int _cursorCol;
    private int _scrollOffset;
    private string[]? _cachedRender;
    private int? _cachedWidth;

    public bool Focused { get; set; }
    public Action<string>? OnSubmit { get; set; }
    public int MaxVisibleLines { get; set; } = 10;

    public Editor(string initialText = "")
    {
        _lines = string.IsNullOrEmpty(initialText) 
            ? new List<string> { "" }
            : new List<string>(initialText.Split('\n'));
        _cursorRow = 0;
        _cursorCol = 0;
        _scrollOffset = 0;
    }

    public string GetText() => string.Join("\n", _lines);

    public void SetText(string text)
    {
        _lines.Clear();
        _lines.AddRange(string.IsNullOrEmpty(text) ? new[] { "" } : text.Split('\n'));
        _cursorRow = 0;
        _cursorCol = 0;
        _scrollOffset = 0;
        Invalidate();
    }

    public void Clear()
    {
        _lines.Clear();
        _lines.Add("");
        _cursorRow = 0;
        _cursorCol = 0;
        _scrollOffset = 0;
        Invalidate();
    }

    public string[] Render(int width)
    {
        if (_cachedRender != null && _cachedWidth == width)
            return _cachedRender;

        var lines = new List<string>();
        var contentWidth = Math.Max(1, width - 2); // Account for "> " prefix

        // Ensure scroll offset is valid
        if (_cursorRow < _scrollOffset)
            _scrollOffset = _cursorRow;
        if (_cursorRow >= _scrollOffset + MaxVisibleLines)
            _scrollOffset = _cursorRow - MaxVisibleLines + 1;

        // Render visible lines
        var endLine = Math.Min(_scrollOffset + MaxVisibleLines, _lines.Count);
        for (var i = _scrollOffset; i < endLine; i++)
        {
            var line = _lines[i];
            var prefix = (i == _cursorRow && Focused) ? "> " : "  ";
            
            // Truncate or pad line
            var displayLine = line.Length > contentWidth 
                ? line.Substring(0, contentWidth)
                : line.PadRight(contentWidth);
            
            lines.Add(prefix + displayLine);
        }

        // Add indicator if there are more lines above/below
        if (_scrollOffset > 0)
            lines[0] = "↑ " + lines[0].Substring(2);
        if (endLine < _lines.Count)
            lines[^1] = "↓ " + lines[^1].Substring(2);

        _cachedRender = lines.ToArray();
        _cachedWidth = width;
        return _cachedRender;
    }

    public void HandleInput(string data)
    {
        Invalidate();

        // Handle special keys
        if (data == "\r" || data == "\n") // Enter
        {
            InsertNewLine();
        }
        else if (data == "\x7f" || data == "\b") // Backspace
        {
            Backspace();
        }
        else if (data == "\x1b") // Escape - submit
        {
            OnSubmit?.Invoke(GetText());
        }
        else if (data == "\x1b[A") // Up arrow
        {
            MoveCursorUp();
        }
        else if (data == "\x1b[B") // Down arrow
        {
            MoveCursorDown();
        }
        else if (data == "\x1b[C") // Right arrow
        {
            MoveCursorRight();
        }
        else if (data == "\x1b[D") // Left arrow
        {
            MoveCursorLeft();
        }
        else if (data == "\x01") // Ctrl+A - Home
        {
            _cursorCol = 0;
        }
        else if (data == "\x05") // Ctrl+E - End
        {
            _cursorCol = _lines[_cursorRow].Length;
        }
        else if (data == "\x0d") // Ctrl+M or Ctrl+Enter - Submit
        {
            OnSubmit?.Invoke(GetText());
        }
        else if (data.Length == 1 && !char.IsControl(data[0])) // Printable character
        {
            InsertCharacter(data[0]);
        }
    }

    public void Invalidate()
    {
        _cachedRender = null;
        _cachedWidth = null;
    }

    private void InsertCharacter(char c)
    {
        var line = _lines[_cursorRow];
        _lines[_cursorRow] = line.Insert(_cursorCol, c.ToString());
        _cursorCol++;
    }

    private void InsertNewLine()
    {
        var currentLine = _lines[_cursorRow];
        var beforeCursor = currentLine.Substring(0, _cursorCol);
        var afterCursor = currentLine.Substring(_cursorCol);
        
        _lines[_cursorRow] = beforeCursor;
        _lines.Insert(_cursorRow + 1, afterCursor);
        _cursorRow++;
        _cursorCol = 0;
    }

    private void Backspace()
    {
        if (_cursorCol > 0)
        {
            // Delete character before cursor
            var line = _lines[_cursorRow];
            _lines[_cursorRow] = line.Remove(_cursorCol - 1, 1);
            _cursorCol--;
        }
        else if (_cursorRow > 0)
        {
            // Join with previous line
            var currentLine = _lines[_cursorRow];
            _lines.RemoveAt(_cursorRow);
            _cursorRow--;
            _cursorCol = _lines[_cursorRow].Length;
            _lines[_cursorRow] += currentLine;
        }
    }

    private void MoveCursorUp()
    {
        if (_cursorRow > 0)
        {
            _cursorRow--;
            _cursorCol = Math.Min(_cursorCol, _lines[_cursorRow].Length);
        }
    }

    private void MoveCursorDown()
    {
        if (_cursorRow < _lines.Count - 1)
        {
            _cursorRow++;
            _cursorCol = Math.Min(_cursorCol, _lines[_cursorRow].Length);
        }
    }

    private void MoveCursorLeft()
    {
        if (_cursorCol > 0)
        {
            _cursorCol--;
        }
        else if (_cursorRow > 0)
        {
            _cursorRow--;
            _cursorCol = _lines[_cursorRow].Length;
        }
    }

    private void MoveCursorRight()
    {
        if (_cursorCol < _lines[_cursorRow].Length)
        {
            _cursorCol++;
        }
        else if (_cursorRow < _lines.Count - 1)
        {
            _cursorRow++;
            _cursorCol = 0;
        }
    }

    public (int Row, int Col) GetCursorPosition() => (_cursorRow, _cursorCol);
    public int GetLineCount() => _lines.Count;
}
