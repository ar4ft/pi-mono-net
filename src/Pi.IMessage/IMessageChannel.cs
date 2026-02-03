using Pi.Channels;

namespace Pi.IMessage;

/// <summary>
/// iMessage channel adapter for Gateway integration
/// </summary>
public class IMessageChannel : ChannelBase
{
    private readonly IMessageConfig _config;
    private readonly IMessageDatabase _database;
    private readonly IMessageProcessor _processor;
    private readonly IMessageSender _sender;
    private IMessageMonitor? _monitor;
    
    public IMessageChannel(
        IMessageConfig config,
        ILogger? logger = null)
        : base(
            id: $"imessage-{Environment.MachineName}",
            type: "imessage",
            name: "iMessage",
            logger)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _database = new IMessageDatabase(_config.DatabasePath);
        _processor = new IMessageProcessor(_config);
        _sender = new IMessageSender();
    }
    
    public override ChannelCapabilities GetCapabilities()
    {
        return new ChannelCapabilities
        {
            CanSend = true,
            CanReceive = true,
            SupportsAttachments = true,
            SupportsGroups = true,
            SupportsReadReceipts = false,
            SupportsTypingIndicators = false,
            SupportsReactions = false,
            MaxMessageLength = 0 // No limit
        };
    }
    
    protected override async Task OnStartAsync(CancellationToken cancellationToken)
    {
        // Create and start the monitor
        _monitor = new IMessageMonitor(_database, _processor, _config);
        
        // Subscribe to messages from the monitor
        _monitor.MessageReceived += OnMonitorMessageReceived;
        
        // Start monitoring
        _monitor.Start();
        
        await Task.CompletedTask;
    }
    
    protected override async Task OnStopAsync(CancellationToken cancellationToken)
    {
        if (_monitor != null)
        {
            _monitor.MessageReceived -= OnMonitorMessageReceived;
            await _monitor.StopAsync();
            _monitor = null;
        }
    }
    
    protected override async Task OnSendMessageAsync(ChannelMessage message, CancellationToken cancellationToken)
    {
        if (message == null)
            throw new ArgumentNullException(nameof(message));
        
        // Extract recipient
        var recipient = message.RecipientIds.FirstOrDefault();
        if (string.IsNullOrEmpty(recipient))
        {
            throw new InvalidOperationException("No recipient specified");
        }
        
        // Send via AppleScript
        await _sender.SendMessageAsync(recipient, message.Content);
    }
    
    private void OnMonitorMessageReceived(object? sender, MessageReceivedEventArgs e)
    {
        // Convert NormalizedMessage to ChannelMessage
        var channelMessage = ConvertToChannelMessage(e.Message);
        
        // Raise the message received event
        RaiseMessageReceived(channelMessage);
    }
    
    private ChannelMessage ConvertToChannelMessage(NormalizedMessage message)
    {
        return new ChannelMessage
        {
            Id = message.MessageId,
            SessionKey = message.SessionKey,
            ChannelId = Id,
            ChannelType = Type,
            SenderId = message.SenderId,
            SenderName = message.SenderName,
            RecipientIds = Array.Empty<string>(), // iMessage doesn't expose recipients easily
            Content = message.Content,
            Timestamp = message.Timestamp,
            IsGroup = message.IsGroupMessage,
            GroupId = message.GroupId,
            GroupName = message.GroupName,
            Direction = MessageDirection.Incoming, // Monitor only receives incoming
            Attachments = message.Attachments.Select(a => new ChannelAttachment
            {
                Id = a.Id,
                Type = "file",
                FileName = a.Name,
                MimeType = a.MimeType,
                Size = a.Size,
                Url = a.FilePath
            }).ToList(),
            Metadata = message.Metadata
        };
    }
}
