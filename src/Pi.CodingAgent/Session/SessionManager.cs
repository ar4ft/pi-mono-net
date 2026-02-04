namespace Pi.CodingAgent.Session;

/// <summary>
/// Manages session lifecycle and coordination
/// </summary>
public class SessionManager
{
    private readonly SessionStorage _storage;
    private SessionInfo? _currentSession;
    private ConversationHistory? _currentHistory;
    private Timer? _autoSaveTimer;
    private bool _isDirty;

    public SessionInfo? CurrentSession => _currentSession;
    public ConversationHistory? CurrentHistory => _currentHistory;
    public bool HasActiveSession => _currentSession != null;

    public SessionManager(SessionStorage? storage = null)
    {
        _storage = storage ?? new SessionStorage();
    }

    public async Task<SessionInfo> CreateSessionAsync(string? name = null, string? model = null)
    {
        // Save current session if exists
        if (HasActiveSession)
        {
            await SaveCurrentSessionAsync();
        }

        var sessionId = GenerateSessionId();
        _currentSession = new SessionInfo
        {
            Id = sessionId,
            Name = name ?? $"Session {DateTime.Now:yyyy-MM-dd HH:mm}",
            CreatedAt = DateTime.UtcNow,
            LastAccessed = DateTime.UtcNow,
            Model = model
        };

        _currentHistory = new ConversationHistory();
        _isDirty = true;

        await SaveCurrentSessionAsync();
        StartAutoSave();

        return _currentSession;
    }

    public async Task<bool> LoadSessionAsync(string sessionId)
    {
        // Save current session if exists
        if (HasActiveSession)
        {
            await SaveCurrentSessionAsync();
        }

        var result = await _storage.LoadSessionAsync(sessionId);
        if (result == null)
        {
            return false;
        }

        _currentSession = result.Value.Info;
        _currentHistory = result.Value.History;
        _currentSession.LastAccessed = DateTime.UtcNow;
        _isDirty = true;

        await SaveCurrentSessionAsync();
        StartAutoSave();

        return true;
    }

    public async Task SaveCurrentSessionAsync()
    {
        if (_currentSession == null || _currentHistory == null)
        {
            return;
        }

        if (!_isDirty)
        {
            return;
        }

        _currentSession.LastAccessed = DateTime.UtcNow;
        _currentSession.MessageCount = _currentHistory.Messages.Count;

        await _storage.SaveSessionAsync(_currentSession, _currentHistory);
        _isDirty = false;
    }

    public void AddMessage(string role, string content, Dictionary<string, string>? metadata = null)
    {
        if (_currentHistory == null)
        {
            throw new InvalidOperationException("No active session");
        }

        _currentHistory.AddMessage(role, content, metadata);
        _isDirty = true;
    }

    public void ClearHistory()
    {
        if (_currentHistory == null)
        {
            throw new InvalidOperationException("No active session");
        }

        _currentHistory.Clear();
        _isDirty = true;
    }

    public async Task<List<SessionInfo>> ListSessionsAsync()
    {
        return await _storage.ListSessionsAsync();
    }

    public async Task DeleteSessionAsync(string sessionId)
    {
        if (_currentSession?.Id == sessionId)
        {
            await CloseSessionAsync();
        }

        await _storage.DeleteSessionAsync(sessionId);
    }

    public async Task CloseSessionAsync()
    {
        StopAutoSave();

        if (HasActiveSession)
        {
            await SaveCurrentSessionAsync();
        }

        _currentSession = null;
        _currentHistory = null;
        _isDirty = false;
    }

    public void UpdateSessionName(string name)
    {
        if (_currentSession == null)
        {
            throw new InvalidOperationException("No active session");
        }

        _currentSession.Name = name;
        _isDirty = true;
    }

    public void UpdateSessionModel(string model)
    {
        if (_currentSession == null)
        {
            throw new InvalidOperationException("No active session");
        }

        _currentSession.Model = model;
        _isDirty = true;
    }

    public void SetMetadata(string key, string value)
    {
        if (_currentSession == null)
        {
            throw new InvalidOperationException("No active session");
        }

        _currentSession.Metadata[key] = value;
        _isDirty = true;
    }

    private void StartAutoSave()
    {
        StopAutoSave();

        // Auto-save every 30 seconds
        _autoSaveTimer = new Timer(async _ =>
        {
            try
            {
                await SaveCurrentSessionAsync();
            }
            catch
            {
                // Ignore errors in auto-save
            }
        }, null, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(30));
    }

    private void StopAutoSave()
    {
        _autoSaveTimer?.Dispose();
        _autoSaveTimer = null;
    }

    private string GenerateSessionId()
    {
        return $"session-{DateTime.UtcNow:yyyyMMdd-HHmmss}-{Guid.NewGuid().ToString("N").Substring(0, 8)}";
    }

    public void Dispose()
    {
        StopAutoSave();
    }
}
