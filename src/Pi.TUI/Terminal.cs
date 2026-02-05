namespace Pi.TUI;

/// <summary>
/// Terminal interface for TUI operations
/// </summary>
public interface ITerminal
{
    /// <summary>
    /// Start the terminal with input and resize handlers
    /// </summary>
    void Start(Action<string> onInput, Action onResize);

    /// <summary>
    /// Stop the terminal and restore state
    /// </summary>
    void Stop();

    /// <summary>
    /// Write output to terminal
    /// </summary>
    void Write(string data);

    /// <summary>
    /// Terminal width in columns
    /// </summary>
    int Columns { get; }

    /// <summary>
    /// Terminal height in rows
    /// </summary>
    int Rows { get; }

    /// <summary>
    /// Move cursor by N lines (negative = up, positive = down)
    /// </summary>
    void MoveBy(int lines);

    /// <summary>
    /// Hide the cursor
    /// </summary>
    void HideCursor();

    /// <summary>
    /// Show the cursor
    /// </summary>
    void ShowCursor();

    /// <summary>
    /// Clear current line
    /// </summary>
    void ClearLine();

    /// <summary>
    /// Clear from cursor to end of screen
    /// </summary>
    void ClearFromCursor();

    /// <summary>
    /// Clear entire screen and move cursor to (0,0)
    /// </summary>
    void ClearScreen();

    /// <summary>
    /// Set terminal window title
    /// </summary>
    void SetTitle(string title);
}

/// <summary>
/// Console-based terminal implementation
/// </summary>
public class ConsoleTerminal : ITerminal
{
    private Action<string>? _inputHandler;
    private Action? _resizeHandler;
    private CancellationTokenSource? _inputCancellation;
    private Task? _inputTask;
    
    public int Columns => Console.WindowWidth;
    public int Rows => Console.WindowHeight;

    public void Start(Action<string> onInput, Action onResize)
    {
        _inputHandler = onInput;
        _resizeHandler = onResize;

        // Enable bracketed paste mode
        Console.Write("\x1b[?2004h");

        // Start input reading task
        _inputCancellation = new CancellationTokenSource();
        _inputTask = Task.Run(() => ReadInputLoop(_inputCancellation.Token));

        // Monitor console size changes
        Console.CancelKeyPress += OnCancelKeyPress;
    }

    public void Stop()
    {
        // Disable bracketed paste mode
        Console.Write("\x1b[?2004l");

        _inputCancellation?.Cancel();
        _inputTask?.Wait(TimeSpan.FromSeconds(1));
        
        Console.CancelKeyPress -= OnCancelKeyPress;
        
        _inputCancellation?.Dispose();
        _inputCancellation = null;
        _inputTask = null;
    }

    private async Task ReadInputLoop(CancellationToken cancellationToken)
    {
        var buffer = new char[1024];
        
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                // Read from console input
                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(intercept: true);
                    var input = FormatKeyInput(key);
                    _inputHandler?.Invoke(input);
                }
                else
                {
                    await Task.Delay(10, cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception)
            {
                // Ignore errors during cancellation
            }
        }
    }

    private static string FormatKeyInput(ConsoleKeyInfo key)
    {
        // Convert ConsoleKeyInfo to string representation
        if (key.Key == ConsoleKey.Enter)
            return "\r";
        if (key.Key == ConsoleKey.Tab)
            return "\t";
        if (key.Key == ConsoleKey.Escape)
            return "\x1b";
        if (key.Key == ConsoleKey.Backspace)
            return "\x7f";

        // Handle arrow keys
        if (key.Key == ConsoleKey.UpArrow)
            return "\x1b[A";
        if (key.Key == ConsoleKey.DownArrow)
            return "\x1b[B";
        if (key.Key == ConsoleKey.RightArrow)
            return "\x1b[C";
        if (key.Key == ConsoleKey.LeftArrow)
            return "\x1b[D";

        return key.KeyChar.ToString();
    }

    private void OnCancelKeyPress(object? sender, ConsoleCancelEventArgs e)
    {
        e.Cancel = true;
        _inputHandler?.Invoke("\x03"); // Ctrl+C
    }

    public void Write(string data)
    {
        Console.Write(data);
    }

    public void MoveBy(int lines)
    {
        if (lines < 0)
        {
            Console.Write($"\x1b[{-lines}A"); // Move up
        }
        else if (lines > 0)
        {
            Console.Write($"\x1b[{lines}B"); // Move down
        }
    }

    public void HideCursor()
    {
        Console.Write("\x1b[?25l");
    }

    public void ShowCursor()
    {
        Console.Write("\x1b[?25h");
    }

    public void ClearLine()
    {
        Console.Write("\x1b[2K");
    }

    public void ClearFromCursor()
    {
        Console.Write("\x1b[J");
    }

    public void ClearScreen()
    {
        Console.Write("\x1b[2J\x1b[H");
    }

    public void SetTitle(string title)
    {
        Console.Write($"\x1b]0;{title}\x07");
    }
}
