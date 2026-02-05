namespace Pi.Channels;

/// <summary>
/// Represents a communication channel (e.g., iMessage, Telegram, Discord)
/// </summary>
public interface IChannel
{
    /// <summary>
    /// Unique identifier for this channel instance
    /// </summary>
    string Id { get; }
    
    /// <summary>
    /// Channel type (e.g., "imessage", "telegram", "discord")
    /// </summary>
    string Type { get; }
    
    /// <summary>
    /// Channel display name
    /// </summary>
    string Name { get; }
    
    /// <summary>
    /// Whether the channel is currently active
    /// </summary>
    bool IsActive { get; }
    
    /// <summary>
    /// Event fired when a new message is received
    /// </summary>
    event EventHandler<ChannelMessageEventArgs>? MessageReceived;
    
    /// <summary>
    /// Event fired when the channel status changes
    /// </summary>
    event EventHandler<ChannelStatusEventArgs>? StatusChanged;
    
    /// <summary>
    /// Start the channel (begin listening for messages)
    /// </summary>
    Task StartAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Stop the channel
    /// </summary>
    Task StopAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Send a message through this channel
    /// </summary>
    Task SendMessageAsync(ChannelMessage message, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get channel capabilities
    /// </summary>
    ChannelCapabilities GetCapabilities();
}

/// <summary>
/// Event args for message received events
/// </summary>
public class ChannelMessageEventArgs : EventArgs
{
    public required ChannelMessage Message { get; init; }
}

/// <summary>
/// Event args for channel status change events
/// </summary>
public class ChannelStatusEventArgs : EventArgs
{
    public required string Status { get; init; }
    public string? Message { get; init; }
    public Exception? Error { get; init; }
}

/// <summary>
/// Channel capabilities
/// </summary>
public record ChannelCapabilities
{
    /// <summary>
    /// Supports sending messages
    /// </summary>
    public bool CanSend { get; init; } = true;
    
    /// <summary>
    /// Supports receiving messages
    /// </summary>
    public bool CanReceive { get; init; } = true;
    
    /// <summary>
    /// Supports sending attachments
    /// </summary>
    public bool SupportsAttachments { get; init; } = false;
    
    /// <summary>
    /// Supports group conversations
    /// </summary>
    public bool SupportsGroups { get; init; } = false;
    
    /// <summary>
    /// Supports read receipts
    /// </summary>
    public bool SupportsReadReceipts { get; init; } = false;
    
    /// <summary>
    /// Supports typing indicators
    /// </summary>
    public bool SupportsTypingIndicators { get; init; } = false;
    
    /// <summary>
    /// Supports reactions/emoji
    /// </summary>
    public bool SupportsReactions { get; init; } = false;
    
    /// <summary>
    /// Maximum message length (0 = unlimited)
    /// </summary>
    public int MaxMessageLength { get; init; } = 0;
}
