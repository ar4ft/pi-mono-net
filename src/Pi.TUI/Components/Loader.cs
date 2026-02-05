namespace Pi.TUI.Components;

/// <summary>
/// Animated loader/spinner component
/// </summary>
public class Loader : IComponent
{
    private readonly string _message;
    private int _frame;
    private static readonly string[] Frames = new[] { "⠋", "⠙", "⠹", "⠸", "⠼", "⠴", "⠦", "⠧", "⠇", "⠏" };

    public Loader(string message = "Loading...")
    {
        _message = message;
        _frame = 0;
    }

    public void Advance()
    {
        _frame = (_frame + 1) % Frames.Length;
    }

    public string[] Render(int width)
    {
        var line = $"{Frames[_frame]} {_message}";
        if (line.Length < width)
            line = line.PadRight(width);
        else if (line.Length > width)
            line = line.Substring(0, width);
        
        return new[] { line };
    }
}
