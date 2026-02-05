using System.Text.Json;

namespace Pi.CodingAgent.Session;

/// <summary>
/// Handles persistence of sessions to disk
/// </summary>
public class SessionStorage
{
    private readonly string _baseDirectory;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public SessionStorage(string? baseDirectory = null)
    {
        _baseDirectory = baseDirectory ?? GetDefaultSessionDirectory();
        EnsureDirectoryExists();
    }

    private static string GetDefaultSessionDirectory()
    {
        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        return Path.Combine(home, ".pi", "sessions");
    }

    private void EnsureDirectoryExists()
    {
        if (!Directory.Exists(_baseDirectory))
        {
            Directory.CreateDirectory(_baseDirectory);
        }
    }

    private string GetSessionDirectory(string sessionId)
    {
        return Path.Combine(_baseDirectory, sessionId);
    }

    private string GetInfoFilePath(string sessionId)
    {
        return Path.Combine(GetSessionDirectory(sessionId), "info.json");
    }

    private string GetHistoryFilePath(string sessionId)
    {
        return Path.Combine(GetSessionDirectory(sessionId), "history.json");
    }

    public async Task SaveSessionAsync(SessionInfo info, ConversationHistory history)
    {
        var sessionDir = GetSessionDirectory(info.Id);
        if (!Directory.Exists(sessionDir))
        {
            Directory.CreateDirectory(sessionDir);
        }

        // Save session info
        var infoJson = JsonSerializer.Serialize(info, JsonOptions);
        await File.WriteAllTextAsync(GetInfoFilePath(info.Id), infoJson);

        // Save conversation history
        var historyJson = JsonSerializer.Serialize(history.Messages, JsonOptions);
        await File.WriteAllTextAsync(GetHistoryFilePath(info.Id), historyJson);
    }

    public async Task<(SessionInfo Info, ConversationHistory History)?> LoadSessionAsync(string sessionId)
    {
        var sessionDir = GetSessionDirectory(sessionId);
        if (!Directory.Exists(sessionDir))
        {
            return null;
        }

        var infoPath = GetInfoFilePath(sessionId);
        var historyPath = GetHistoryFilePath(sessionId);

        if (!File.Exists(infoPath))
        {
            return null;
        }

        // Load session info
        var infoJson = await File.ReadAllTextAsync(infoPath);
        var info = JsonSerializer.Deserialize<SessionInfo>(infoJson, JsonOptions);

        if (info == null)
        {
            return null;
        }

        // Load conversation history
        var history = new ConversationHistory();
        if (File.Exists(historyPath))
        {
            var historyJson = await File.ReadAllTextAsync(historyPath);
            var messages = JsonSerializer.Deserialize<List<ConversationMessage>>(historyJson, JsonOptions);
            if (messages != null)
            {
                history.LoadMessages(messages);
            }
        }

        return (info, history);
    }

    public async Task<List<SessionInfo>> ListSessionsAsync()
    {
        var sessions = new List<SessionInfo>();

        if (!Directory.Exists(_baseDirectory))
        {
            return sessions;
        }

        var sessionDirs = Directory.GetDirectories(_baseDirectory);
        foreach (var dir in sessionDirs)
        {
            var sessionId = Path.GetFileName(dir);
            var infoPath = GetInfoFilePath(sessionId);

            if (File.Exists(infoPath))
            {
                try
                {
                    var infoJson = await File.ReadAllTextAsync(infoPath);
                    var info = JsonSerializer.Deserialize<SessionInfo>(infoJson, JsonOptions);
                    if (info != null)
                    {
                        sessions.Add(info);
                    }
                }
                catch
                {
                    // Skip corrupted sessions
                }
            }
        }

        return sessions.OrderByDescending(s => s.LastAccessed).ToList();
    }

    public async Task DeleteSessionAsync(string sessionId)
    {
        var sessionDir = GetSessionDirectory(sessionId);
        if (Directory.Exists(sessionDir))
        {
            Directory.Delete(sessionDir, true);
        }
    }

    public bool SessionExists(string sessionId)
    {
        return Directory.Exists(GetSessionDirectory(sessionId)) 
            && File.Exists(GetInfoFilePath(sessionId));
    }
}
