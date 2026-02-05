namespace Pi.TUI.Components;

/// <summary>
/// Selectable list component
/// </summary>
public class SelectList<T> : IComponent, IFocusable where T : notnull
{
    private readonly List<SelectItem<T>> _items;
    private int _selectedIndex;

    public bool Focused { get; set; }
    public Action<T>? OnSelect { get; set; }

    public SelectList(IEnumerable<SelectItem<T>> items)
    {
        _items = new List<SelectItem<T>>(items);
        _selectedIndex = 0;
    }

    public void AddItem(SelectItem<T> item)
    {
        _items.Add(item);
    }

    public void SetItems(IEnumerable<SelectItem<T>> items)
    {
        _items.Clear();
        _items.AddRange(items);
        _selectedIndex = 0;
    }

    public T? GetSelectedValue() => _items.Count > 0 ? _items[_selectedIndex].Value : default;

    public string[] Render(int width)
    {
        var lines = new List<string>();

        for (var i = 0; i < _items.Count; i++)
        {
            var item = _items[i];
            var prefix = i == _selectedIndex ? (Focused ? "→ " : "> ") : "  ";
            var line = prefix + item.Label;
            
            if (line.Length > width)
                line = line.Substring(0, width);
            else
                line = line.PadRight(width);
            
            lines.Add(line);
        }

        return lines.ToArray();
    }

    public void HandleInput(string data)
    {
        if (data == "\x1b[A") // Up
        {
            _selectedIndex = Math.Max(0, _selectedIndex - 1);
        }
        else if (data == "\x1b[B") // Down
        {
            _selectedIndex = Math.Min(_items.Count - 1, _selectedIndex + 1);
        }
        else if (data == "\r" || data == "\n") // Enter
        {
            if (_items.Count > 0)
            {
                OnSelect?.Invoke(_items[_selectedIndex].Value);
            }
        }
    }
}

public record SelectItem<T>(string Label, T Value);
