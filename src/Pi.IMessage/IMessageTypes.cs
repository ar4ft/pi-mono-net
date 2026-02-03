namespace Pi.IMessage;

/// <summary>
/// Represents a message from iMessage
/// </summary>
public record IMessageRecord
{
    public required long RowId { get; init; }
    public required string Guid { get; init; }
    public string? Text { get; init; }
    public required string HandleId { get; init; }
    public required string Service { get; init; }
    public string? AccountGuid { get; init; }
    public required DateTime Date { get; init; }
    public required DateTime DateRead { get; init; }
    public required DateTime DateDelivered { get; init; }
    public required bool IsFromMe { get; init; }
    public required bool IsRead { get; init; }
    public required bool IsAudioMessage { get; init; }
    public string? Subject { get; init; }
    public string? GroupTitle { get; init; }
    public int? GroupActionType { get; init; }
    public string? AssociatedMessageGuid { get; init; }
    public string? AssociatedMessageType { get; init; }
    public string? BalloonBundleId { get; init; }
    public string? ExpressiveSendStyleId { get; init; }
    public string? ThreadOriginatorGuid { get; init; }
    public string? ThreadOriginatorPart { get; init; }
    public List<IMessageAttachment> Attachments { get; init; } = new();
    public List<string> Recipients { get; init; } = new();
}

/// <summary>
/// Represents an attachment in an iMessage
/// </summary>
public record IMessageAttachment
{
    public required long RowId { get; init; }
    public required string Guid { get; init; }
    public required DateTime CreatedDate { get; init; }
    public required DateTime StartDate { get; init; }
    public required string Filename { get; init; }
    public string? Uti { get; init; }
    public string? MimeType { get; init; }
    public required long TransferState { get; init; }
    public required bool IsOutgoing { get; init; }
    public string? TransferName { get; init; }
    public required long TotalBytes { get; init; }
}

/// <summary>
/// Represents a contact/handle in iMessage
/// </summary>
public record IMessageHandle
{
    public required long RowId { get; init; }
    public required string Id { get; init; }
    public required string Country { get; init; }
    public required string Service { get; init; }
    public required string Uncanonicalized_id { get; init; }
}

/// <summary>
/// Represents a chat in iMessage
/// </summary>
public record IMessageChat
{
    public required long RowId { get; init; }
    public required string Guid { get; init; }
    public required int Style { get; init; }
    public required int State { get; init; }
    public required string AccountId { get; init; }
    public string? ChatIdentifier { get; init; }
    public required string ServiceName { get; init; }
    public required string RoomName { get; init; }
    public required string AccountLogin { get; init; }
    public required bool IsArchived { get; init; }
    public string? LastAddressedHandle { get; init; }
    public string? DisplayName { get; init; }
    public required string GroupId { get; init; }
    public required bool IsFiltered { get; init; }
    public required int SuccessfulQuery { get; init; }
}

/// <summary>
/// Configuration for iMessage integration
/// </summary>
public record IMessageConfig
{
    /// <summary>
    /// Path to Messages database. Default: ~/Library/Messages/chat.db
    /// </summary>
    public string DatabasePath { get; init; } = "~/Library/Messages/chat.db";
    
    /// <summary>
    /// Polling interval in seconds
    /// </summary>
    public int PollingIntervalSeconds { get; init; } = 1;
    
    /// <summary>
    /// Agent name for session keys
    /// </summary>
    public string AgentName { get; init; } = "main";
    
    /// <summary>
    /// Whether to mark messages as read
    /// </summary>
    public bool MarkAsRead { get; init; } = true;
    
    /// <summary>
    /// Whether to process group messages
    /// </summary>
    public bool ProcessGroupMessages { get; init; } = true;
    
    /// <summary>
    /// Whether to process direct messages
    /// </summary>
    public bool ProcessDirectMessages { get; init; } = true;
    
    /// <summary>
    /// Maximum number of messages to fetch per poll
    /// </summary>
    public int MaxMessagesPerPoll { get; init; } = 100;
}

/// <summary>
/// Result of sending a message
/// </summary>
public record SendMessageResult
{
    public required bool Success { get; init; }
    public string? Error { get; init; }
    public string? MessageGuid { get; init; }
}

/// <summary>
/// Normalized message for agent processing
/// </summary>
public record NormalizedMessage
{
    public required string SessionKey { get; init; }
    public required string MessageId { get; init; }
    public required string SenderId { get; init; }
    public string? SenderName { get; init; }
    public required string Content { get; init; }
    public required DateTime Timestamp { get; init; }
    public required bool IsGroupMessage { get; init; }
    public string? GroupId { get; init; }
    public string? GroupName { get; init; }
    public List<NormalizedAttachment> Attachments { get; init; } = new();
    public Dictionary<string, object> Metadata { get; init; } = new();
}

/// <summary>
/// Normalized attachment for agent processing
/// </summary>
public record NormalizedAttachment
{
    public required string Id { get; init; }
    public required string FilePath { get; init; }
    public required string MimeType { get; init; }
    public required long Size { get; init; }
    public string? Name { get; init; }
}

/// <summary>
/// Session key generation result
/// </summary>
public record SessionKeyInfo
{
    public required string SessionKey { get; init; }
    public required string Type { get; init; } // "dm" or "group"
    public required string Identifier { get; init; }
}
