using Pi.Channels;
using Pi.Agent;
using System.Collections.Concurrent;

namespace Pi.Gateway;

/// <summary>
/// Manages sessions between channels and agents
/// </summary>
public class SessionManager
{
    private readonly ConcurrentDictionary<string, Session> _sessions = new();
    private readonly ConcurrentDictionary<string, string> _channelToSession = new();
    
    /// <summary>
    /// Get or create a session for a session key
    /// </summary>
    public Session GetOrCreateSession(string sessionKey, string channelId, string channelType)
    {
        return _sessions.GetOrAdd(sessionKey, key =>
        {
            var session = new Session
            {
                SessionKey = key,
                ChannelId = channelId,
                ChannelType = channelType,
                CreatedAt = DateTime.UtcNow,
                LastActivity = DateTime.UtcNow,
                Messages = new List<IAgentMessage>(),
                Metadata = new Dictionary<string, object>()
            };
            
            _channelToSession.TryAdd(channelId, key);
            
            return session;
        });
    }
    
    /// <summary>
    /// Get a session by session key
    /// </summary>
    public Session? GetSession(string sessionKey)
    {
        _sessions.TryGetValue(sessionKey, out var session);
        return session;
    }
    
    /// <summary>
    /// Get all active sessions
    /// </summary>
    public IEnumerable<Session> GetAllSessions()
    {
        return _sessions.Values;
    }
    
    /// <summary>
    /// Update session last activity
    /// </summary>
    public void UpdateActivity(string sessionKey)
    {
        if (_sessions.TryGetValue(sessionKey, out var session))
        {
            session.LastActivity = DateTime.UtcNow;
        }
    }
    
    /// <summary>
    /// Add a message to session history
    /// </summary>
    public void AddMessage(string sessionKey, IAgentMessage message)
    {
        if (_sessions.TryGetValue(sessionKey, out var session))
        {
            session.Messages.Add(message);
            session.LastActivity = DateTime.UtcNow;
        }
    }
    
    /// <summary>
    /// Clear old inactive sessions
    /// </summary>
    public int ClearInactiveSessions(TimeSpan inactivityThreshold)
    {
        var cutoff = DateTime.UtcNow - inactivityThreshold;
        var toRemove = _sessions.Where(kvp => kvp.Value.LastActivity < cutoff).ToList();
        
        foreach (var kvp in toRemove)
        {
            _sessions.TryRemove(kvp.Key, out _);
            _channelToSession.TryRemove(kvp.Value.ChannelId, out _);
        }
        
        return toRemove.Count;
    }
    
    /// <summary>
    /// Get session count
    /// </summary>
    public int Count => _sessions.Count;
}

/// <summary>
/// Represents an active session between a channel and an agent
/// </summary>
public class Session
{
    /// <summary>
    /// Session key (e.g., "agent:main:imessage:dm:+1234567890")
    /// </summary>
    public required string SessionKey { get; init; }
    
    /// <summary>
    /// Channel ID this session is associated with
    /// </summary>
    public required string ChannelId { get; init; }
    
    /// <summary>
    /// Channel type (e.g., "imessage", "telegram")
    /// </summary>
    public required string ChannelType { get; init; }
    
    /// <summary>
    /// When this session was created
    /// </summary>
    public DateTime CreatedAt { get; init; }
    
    /// <summary>
    /// Last activity timestamp
    /// </summary>
    public DateTime LastActivity { get; set; }
    
    /// <summary>
    /// Message history for this session
    /// </summary>
    public List<IAgentMessage> Messages { get; init; } = new();
    
    /// <summary>
    /// Session metadata
    /// </summary>
    public Dictionary<string, object> Metadata { get; init; } = new();
    
    /// <summary>
    /// Current agent state
    /// </summary>
    public AgentState? AgentState { get; set; }
}
