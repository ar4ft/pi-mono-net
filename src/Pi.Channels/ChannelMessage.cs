namespace Pi.Channels;

/// <summary>
/// Normalized message representation across all channels
/// </summary>
public record ChannelMessage
{
    /// <summary>
    /// Unique message ID (channel-specific)
    /// </summary>
    public required string Id { get; init; }
    
    /// <summary>
    /// Session key for agent routing (e.g., "agent:main:imessage:dm:+1234567890")
    /// </summary>
    public required string SessionKey { get; init; }
    
    /// <summary>
    /// Channel this message came from/goes to
    /// </summary>
    public required string ChannelId { get; init; }
    
    /// <summary>
    /// Channel type (e.g., "imessage", "telegram")
    /// </summary>
    public required string ChannelType { get; init; }
    
    /// <summary>
    /// Sender ID (phone number, username, etc.)
    /// </summary>
    public required string SenderId { get; init; }
    
    /// <summary>
    /// Sender display name
    /// </summary>
    public string? SenderName { get; init; }
    
    /// <summary>
    /// Recipient ID(s)
    /// </summary>
    public IReadOnlyList<string> RecipientIds { get; init; } = Array.Empty<string>();
    
    /// <summary>
    /// Message content (text)
    /// </summary>
    public required string Content { get; init; }
    
    /// <summary>
    /// Message timestamp
    /// </summary>
    public required DateTime Timestamp { get; init; }
    
    /// <summary>
    /// Whether this is a group message
    /// </summary>
    public bool IsGroup { get; init; }
    
    /// <summary>
    /// Group ID (if group message)
    /// </summary>
    public string? GroupId { get; init; }
    
    /// <summary>
    /// Group name (if group message)
    /// </summary>
    public string? GroupName { get; init; }
    
    /// <summary>
    /// Message direction
    /// </summary>
    public MessageDirection Direction { get; init; }
    
    /// <summary>
    /// Attachments (if any)
    /// </summary>
    public IReadOnlyList<ChannelAttachment> Attachments { get; init; } = Array.Empty<ChannelAttachment>();
    
    /// <summary>
    /// Reply to message ID (if this is a reply)
    /// </summary>
    public string? ReplyToMessageId { get; init; }
    
    /// <summary>
    /// Additional metadata
    /// </summary>
    public Dictionary<string, object> Metadata { get; init; } = new();
}

/// <summary>
/// Message direction (incoming or outgoing)
/// </summary>
public enum MessageDirection
{
    Incoming,
    Outgoing
}

/// <summary>
/// Attachment in a channel message
/// </summary>
public record ChannelAttachment
{
    /// <summary>
    /// Attachment ID
    /// </summary>
    public required string Id { get; init; }
    
    /// <summary>
    /// Attachment type (image, video, file, etc.)
    /// </summary>
    public required string Type { get; init; }
    
    /// <summary>
    /// File name (if applicable)
    /// </summary>
    public string? FileName { get; init; }
    
    /// <summary>
    /// MIME type
    /// </summary>
    public string? MimeType { get; init; }
    
    /// <summary>
    /// File size in bytes
    /// </summary>
    public long? Size { get; init; }
    
    /// <summary>
    /// URL or local path to the attachment
    /// </summary>
    public string? Url { get; init; }
    
    /// <summary>
    /// Additional metadata
    /// </summary>
    public Dictionary<string, object> Metadata { get; init; } = new();
}
