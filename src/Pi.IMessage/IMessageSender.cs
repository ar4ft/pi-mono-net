using System.Diagnostics;
using System.Text;

namespace Pi.IMessage;

/// <summary>
/// Interface for sending iMessages
/// </summary>
public interface IIMessageSender
{
    Task<SendMessageResult> SendMessageAsync(string recipient, string message, CancellationToken cancellationToken = default);
    Task<SendMessageResult> SendMessageToChatAsync(string chatGuid, string message, CancellationToken cancellationToken = default);
}

/// <summary>
/// Sends iMessages using AppleScript
/// </summary>
public class IMessageSender : IIMessageSender
{
    public async Task<SendMessageResult> SendMessageAsync(string recipient, string message, CancellationToken cancellationToken = default)
    {
        try
        {
            var script = $@"
tell application ""Messages""
    set targetService to 1st account whose service type = iMessage
    set targetBuddy to participant ""{EscapeAppleScript(recipient)}"" of targetService
    send ""{EscapeAppleScript(message)}"" to targetBuddy
end tell
";

            var result = await ExecuteAppleScriptAsync(script, cancellationToken);
            return new SendMessageResult
            {
                Success = result.ExitCode == 0,
                Error = result.ExitCode != 0 ? result.Error : null,
                MessageGuid = Guid.NewGuid().ToString() // AppleScript doesn't return GUID
            };
        }
        catch (Exception ex)
        {
            return new SendMessageResult
            {
                Success = false,
                Error = ex.Message
            };
        }
    }

    public async Task<SendMessageResult> SendMessageToChatAsync(string chatGuid, string message, CancellationToken cancellationToken = default)
    {
        try
        {
            var script = $@"
tell application ""Messages""
    set targetChat to a reference to text chat id ""{EscapeAppleScript(chatGuid)}""
    send ""{EscapeAppleScript(message)}"" to targetChat
end tell
";

            var result = await ExecuteAppleScriptAsync(script, cancellationToken);
            return new SendMessageResult
            {
                Success = result.ExitCode == 0,
                Error = result.ExitCode != 0 ? result.Error : null,
                MessageGuid = Guid.NewGuid().ToString()
            };
        }
        catch (Exception ex)
        {
            return new SendMessageResult
            {
                Success = false,
                Error = ex.Message
            };
        }
    }

    private static async Task<(int ExitCode, string Output, string Error)> ExecuteAppleScriptAsync(
        string script, 
        CancellationToken cancellationToken)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "osascript",
            ArgumentList = { "-e", script },
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process { StartInfo = psi };
        process.Start();

        var outputTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
        var errorTask = process.StandardError.ReadToEndAsync(cancellationToken);

        await process.WaitForExitAsync(cancellationToken);

        return (process.ExitCode, await outputTask, await errorTask);
    }

    private static string EscapeAppleScript(string text)
    {
        return text.Replace("\\", "\\\\")
                   .Replace("\"", "\\\"")
                   .Replace("\n", "\\n")
                   .Replace("\r", "\\r")
                   .Replace("\t", "\\t");
    }
}
