namespace Pi.TUI;

/// <summary>
/// Base interface for all TUI components
/// </summary>
public interface IComponent
{
    /// <summary>
    /// Render the component to an array of strings (one per line)
    /// Each line must not exceed the provided width
    /// </summary>
    string[] Render(int width);

    /// <summary>
    /// Handle keyboard/terminal input
    /// </summary>
    void HandleInput(string data) { }

    /// <summary>
    /// Invalidate cached render state
    /// </summary>
    void Invalidate() { }
}

/// <summary>
/// Interface for components that can receive focus and show a cursor
/// </summary>
public interface IFocusable
{
    bool Focused { get; set; }
}

/// <summary>
/// Container component that holds child components
/// </summary>
public class Container : IComponent
{
    protected readonly List<IComponent> _children = new();

    public void AddChild(IComponent component)
    {
        _children.Add(component);
    }

    public void RemoveChild(IComponent component)
    {
        _children.Remove(component);
    }

    public void ClearChildren()
    {
        _children.Clear();
    }

    public virtual string[] Render(int width)
    {
        var lines = new List<string>();
        
        foreach (var child in _children)
        {
            var childLines = child.Render(width);
            lines.AddRange(childLines);
        }

        return lines.ToArray();
    }

    public virtual void HandleInput(string data)
    {
        // Override in derived classes to handle input
    }

    public virtual void Invalidate()
    {
        foreach (var child in _children)
        {
            child.Invalidate();
        }
    }
}
