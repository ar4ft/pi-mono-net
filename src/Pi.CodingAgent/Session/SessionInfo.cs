using System.Text.Json;
using System.Text.Json.Serialization;

namespace Pi.CodingAgent.Session;

/// <summary>
/// Represents session metadata and information
/// </summary>
public class SessionInfo
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("last_accessed")]
    public DateTime LastAccessed { get; set; }

    [JsonPropertyName("model")]
    public string? Model { get; set; }

    [JsonPropertyName("message_count")]
    public int MessageCount { get; set; }

    [JsonPropertyName("metadata")]
    public Dictionary<string, string> Metadata { get; set; } = new();
}

/// <summary>
/// Represents a message in the conversation history
/// </summary>
public class ConversationMessage
{
    [JsonPropertyName("role")]
    public string Role { get; set; } = string.Empty;

    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;

    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }

    [JsonPropertyName("metadata")]
    public Dictionary<string, string>? Metadata { get; set; }
}

/// <summary>
/// Manages conversation history for a session
/// </summary>
public class ConversationHistory
{
    private readonly List<ConversationMessage> _messages = new();
    private readonly int _maxMessages;

    public IReadOnlyList<ConversationMessage> Messages => _messages.AsReadOnly();

    public ConversationHistory(int maxMessages = 1000)
    {
        _maxMessages = maxMessages;
    }

    public void AddMessage(string role, string content, Dictionary<string, string>? metadata = null)
    {
        var message = new ConversationMessage
        {
            Role = role,
            Content = content,
            Timestamp = DateTime.UtcNow,
            Metadata = metadata
        };

        _messages.Add(message);

        // Trim old messages if exceeds max
        if (_messages.Count > _maxMessages)
        {
            _messages.RemoveAt(0);
        }
    }

    public void Clear()
    {
        _messages.Clear();
    }

    public List<ConversationMessage> GetRecent(int count)
    {
        return _messages.TakeLast(count).ToList();
    }

    public void LoadMessages(List<ConversationMessage> messages)
    {
        _messages.Clear();
        _messages.AddRange(messages.TakeLast(_maxMessages));
    }
}
