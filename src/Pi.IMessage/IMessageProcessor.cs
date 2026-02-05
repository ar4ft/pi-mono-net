namespace Pi.IMessage;

/// <summary>
/// Normalizes iMessage records for agent processing
/// </summary>
public class IMessageProcessor
{
    private readonly IMessageConfig _config;

    public IMessageProcessor(IMessageConfig config)
    {
        _config = config;
    }

    /// <summary>
    /// Normalizes an iMessage record for agent processing
    /// </summary>
    public NormalizedMessage NormalizeMessage(IMessageRecord message, IMessageChat? chat = null, IMessageHandle? handle = null)
    {
        var isGroupMessage = chat?.Style == 43; // 43 = group chat
        var sessionKey = GenerateSessionKey(message, chat, isGroupMessage);

        return new NormalizedMessage
        {
            SessionKey = sessionKey.SessionKey,
            MessageId = message.Guid,
            SenderId = message.HandleId,
            SenderName = handle?.Id ?? message.HandleId,
            Content = message.Text ?? string.Empty,
            Timestamp = message.Date,
            IsGroupMessage = isGroupMessage,
            GroupId = isGroupMessage ? chat?.Guid : null,
            GroupName = isGroupMessage ? (chat?.DisplayName ?? chat?.ChatIdentifier) : null,
            Attachments = message.Attachments.Select(a => new NormalizedAttachment
            {
                Id = a.Guid,
                FilePath = a.Filename,
                MimeType = a.MimeType ?? "application/octet-stream",
                Size = a.TotalBytes,
                Name = a.TransferName
            }).ToList(),
            Metadata = new Dictionary<string, object>
            {
                ["service"] = message.Service,
                ["is_audio"] = message.IsAudioMessage,
                ["is_read"] = message.IsRead,
                ["raw_row_id"] = message.RowId
            }
        };
    }

    /// <summary>
    /// Generates a session key for routing to agents
    /// Format: agent:{agentName}:imessage:{type}:{identifier}
    /// </summary>
    public SessionKeyInfo GenerateSessionKey(IMessageRecord message, IMessageChat? chat, bool isGroupMessage)
    {
        var type = isGroupMessage ? "group" : "dm";
        var identifier = isGroupMessage 
            ? (chat?.Guid ?? message.HandleId)
            : message.HandleId;

        return new SessionKeyInfo
        {
            SessionKey = $"agent:{_config.AgentName}:imessage:{type}:{identifier}",
            Type = type,
            Identifier = identifier
        };
    }

    /// <summary>
    /// Filters messages based on configuration
    /// </summary>
    public bool ShouldProcessMessage(IMessageRecord message, IMessageChat? chat)
    {
        // Skip messages from self
        if (message.IsFromMe)
            return false;

        // Check if it's a group message
        var isGroupMessage = chat?.Style == 43;

        // Check configuration
        if (isGroupMessage && !_config.ProcessGroupMessages)
            return false;

        if (!isGroupMessage && !_config.ProcessDirectMessages)
            return false;

        // Skip empty messages
        if (string.IsNullOrWhiteSpace(message.Text))
            return false;

        return true;
    }

    /// <summary>
    /// Extracts mentions from group messages
    /// </summary>
    public List<string> ExtractMentions(string text)
    {
        var mentions = new List<string>();
        var parts = text.Split('@');
        
        for (int i = 1; i < parts.Length; i++)
        {
            var mention = parts[i].Split(' ')[0].Trim();
            if (!string.IsNullOrEmpty(mention))
            {
                mentions.Add(mention);
            }
        }

        return mentions;
    }

    /// <summary>
    /// Checks if message contains a specific mention
    /// </summary>
    public bool HasMention(string text, string mention)
    {
        return text.Contains($"@{mention}", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Removes mentions from text
    /// </summary>
    public string RemoveMentions(string text)
    {
        return System.Text.RegularExpressions.Regex.Replace(text, @"@\w+", "").Trim();
    }
}
