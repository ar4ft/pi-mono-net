namespace Pi.TUI.Components;

/// <summary>
/// Single-line input field
/// </summary>
public class Input : IComponent, IFocusable
{
    private string _value = "";
    private int _cursorPosition;
    
    public bool Focused { get; set; }
    public Action<string>? OnSubmit { get; set; }

    public void SetValue(string value)
    {
        _value = value;
        _cursorPosition = value.Length;
    }

    public string GetValue() => _value;

    public string[] Render(int width)
    {
        var displayValue = _value;
        if (displayValue.Length > width - 2)
        {
            // Scroll horizontally if needed
            var start = Math.Max(0, _cursorPosition - (width - 4));
            displayValue = displayValue.Substring(start, Math.Min(width - 2, displayValue.Length - start));
        }

        var line = "> " + displayValue;
        
        // Pad to width
        if (line.Length < width)
        {
            line += new string(' ', width - line.Length);
        }
        else if (line.Length > width)
        {
            line = line.Substring(0, width);
        }

        return new[] { line };
    }

    public void HandleInput(string data)
    {
        if (data == "\r" || data == "\n")
        {
            OnSubmit?.Invoke(_value);
        }
        else if (data == "\x7f" || data == "\b") // Backspace
        {
            if (_cursorPosition > 0)
            {
                _value = _value.Remove(_cursorPosition - 1, 1);
                _cursorPosition--;
            }
        }
        else if (data == "\x1b") // Escape
        {
            _value = "";
            _cursorPosition = 0;
        }
        else if (data.Length == 1 && !char.IsControl(data[0]))
        {
            _value = _value.Insert(_cursorPosition, data);
            _cursorPosition++;
        }
    }
}
