namespace Pi.Channels;

/// <summary>
/// Utility for building session keys in the format: agent:{name}:{channel}:{type}:{id}
/// </summary>
public static class SessionKeyBuilder
{
    /// <summary>
    /// Build a session key for a channel conversation
    /// </summary>
    /// <param name="agentName">Agent name (e.g., "main")</param>
    /// <param name="channelType">Channel type (e.g., "imessage", "telegram")</param>
    /// <param name="conversationType">Conversation type (e.g., "dm", "group")</param>
    /// <param name="conversationId">Unique conversation identifier</param>
    /// <returns>Formatted session key</returns>
    public static string Build(
        string agentName,
        string channelType,
        string conversationType,
        string conversationId)
    {
        if (string.IsNullOrWhiteSpace(agentName))
            throw new ArgumentException("Agent name cannot be empty", nameof(agentName));
        
        if (string.IsNullOrWhiteSpace(channelType))
            throw new ArgumentException("Channel type cannot be empty", nameof(channelType));
        
        if (string.IsNullOrWhiteSpace(conversationType))
            throw new ArgumentException("Conversation type cannot be empty", nameof(conversationType));
        
        if (string.IsNullOrWhiteSpace(conversationId))
            throw new ArgumentException("Conversation ID cannot be empty", nameof(conversationId));
        
        return $"agent:{agentName}:{channelType}:{conversationType}:{conversationId}";
    }
    
    /// <summary>
    /// Parse a session key into its components
    /// </summary>
    /// <param name="sessionKey">Session key to parse</param>
    /// <returns>Parsed components</returns>
    public static SessionKeyComponents Parse(string sessionKey)
    {
        if (string.IsNullOrWhiteSpace(sessionKey))
            throw new ArgumentException("Session key cannot be empty", nameof(sessionKey));
        
        var parts = sessionKey.Split(':');
        
        if (parts.Length < 5 || parts[0] != "agent")
            throw new FormatException($"Invalid session key format: {sessionKey}");
        
        return new SessionKeyComponents
        {
            AgentName = parts[1],
            ChannelType = parts[2],
            ConversationType = parts[3],
            ConversationId = string.Join(":", parts[4..])
        };
    }
    
    /// <summary>
    /// Try to parse a session key
    /// </summary>
    public static bool TryParse(string sessionKey, out SessionKeyComponents? components)
    {
        try
        {
            components = Parse(sessionKey);
            return true;
        }
        catch
        {
            components = null;
            return false;
        }
    }
}

/// <summary>
/// Components of a parsed session key
/// </summary>
public record SessionKeyComponents
{
    public required string AgentName { get; init; }
    public required string ChannelType { get; init; }
    public required string ConversationType { get; init; }
    public required string ConversationId { get; init; }
}
