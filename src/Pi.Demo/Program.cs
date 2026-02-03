using Pi.TUI;
using Pi.TUI.Components;

Console.WriteLine("Pi Demo - .NET Terminal UI");
Console.WriteLine("================================");
Console.WriteLine();

// Create terminal
var terminal = new ConsoleTerminal();

// Create TUI
var tui = new TUI(terminal);

// Add header
tui.AddChild(new Text("Welcome to Pi .NET Demo!", paddingX: 2, paddingY: 1));
tui.AddChild(new Spacer());

// Add some sample content
tui.AddChild(new Text("This demonstrates the TUI framework with:", paddingX: 2, paddingY: 0));
tui.AddChild(new Text("• Differential rendering", paddingX: 4, paddingY: 0));
tui.AddChild(new Text("• Component-based architecture", paddingX: 4, paddingY: 0));
tui.AddChild(new Text("• Terminal input handling", paddingX: 4, paddingY: 0));
tui.AddChild(new Spacer());

// Add input field
var input = new Input();
var messageCount = 0;

input.OnSubmit = (text) =>
{
    if (!string.IsNullOrWhiteSpace(text))
    {
        messageCount++;
        tui.AddChild(new Text($"[{messageCount}] You said: {text}", paddingX: 2, paddingY: 0));
        input.SetValue(""); // Clear input
    }
    
    if (text.Trim().ToLower() == "quit" || text.Trim().ToLower() == "exit")
    {
        tui.Stop();
        Environment.Exit(0);
    }
};

tui.AddChild(new Spacer());
tui.AddChild(new Text("Type your message and press Enter (type 'quit' to exit):", paddingX: 2, paddingY: 0));
tui.AddChild(input);

// Set focus to input
tui.SetFocus(input);

// Start TUI
tui.Start();

// Keep running
Console.WriteLine("\nPress Ctrl+C to exit...");
await Task.Delay(-1);
