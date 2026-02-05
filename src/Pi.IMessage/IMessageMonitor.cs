namespace Pi.IMessage;

/// <summary>
/// Event args for new messages
/// </summary>
public class MessageReceivedEventArgs : EventArgs
{
    public required NormalizedMessage Message { get; init; }
    public required IMessageRecord RawMessage { get; init; }
    public IMessageChat? Chat { get; init; }
    public IMessageHandle? Handle { get; init; }
}

/// <summary>
/// Event args for monitor errors
/// </summary>
public class MonitorErrorEventArgs : EventArgs
{
    public required Exception Exception { get; init; }
    public required string Context { get; init; }
}

/// <summary>
/// Monitors the iMessage database for new messages
/// </summary>
public class IMessageMonitor : IDisposable
{
    private readonly IIMessageDatabase _database;
    private readonly IMessageProcessor _processor;
    private readonly IMessageConfig _config;
    private readonly CancellationTokenSource _cts;
    private Task? _monitorTask;
    private long _lastProcessedRowId;
    private bool _disposed;

    public event EventHandler<MessageReceivedEventArgs>? MessageReceived;
    public event EventHandler<MonitorErrorEventArgs>? Error;

    public IMessageMonitor(IIMessageDatabase database, IMessageProcessor processor, IMessageConfig config)
    {
        _database = database;
        _processor = processor;
        _config = config;
        _cts = new CancellationTokenSource();
    }

    /// <summary>
    /// Starts monitoring for new messages
    /// </summary>
    public void Start()
    {
        if (_monitorTask != null)
            throw new InvalidOperationException("Monitor is already running");

        _monitorTask = Task.Run(MonitorLoopAsync, _cts.Token);
    }

    /// <summary>
    /// Stops monitoring
    /// </summary>
    public async Task StopAsync()
    {
        if (_monitorTask == null)
            return;

        _cts.Cancel();
        
        try
        {
            await _monitorTask;
        }
        catch (OperationCanceledException)
        {
            // Expected
        }
        
        _monitorTask = null;
    }

    private async Task MonitorLoopAsync()
    {
        try
        {
            // Initialize with most recent message
            var recentMessages = await _database.GetRecentMessagesAsync(1, _cts.Token);
            _lastProcessedRowId = recentMessages.FirstOrDefault()?.RowId ?? 0;

            Console.WriteLine($"[IMessageMonitor] Starting monitor from row ID: {_lastProcessedRowId}");

            while (!_cts.Token.IsCancellationRequested)
            {
                try
                {
                    await PollForNewMessagesAsync(_cts.Token);
                    await Task.Delay(TimeSpan.FromSeconds(_config.PollingIntervalSeconds), _cts.Token);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    OnError(ex, "Polling for messages");
                    await Task.Delay(TimeSpan.FromSeconds(5), _cts.Token);
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Expected
        }
        catch (Exception ex)
        {
            OnError(ex, "Monitor loop");
        }
    }

    private async Task PollForNewMessagesAsync(CancellationToken cancellationToken)
    {
        var newMessages = await _database.GetMessagesSinceAsync(
            _lastProcessedRowId, 
            _config.MaxMessagesPerPoll, 
            cancellationToken);

        if (newMessages.Count == 0)
            return;

        Console.WriteLine($"[IMessageMonitor] Found {newMessages.Count} new message(s)");

        foreach (var message in newMessages)
        {
            try
            {
                await ProcessMessageAsync(message, cancellationToken);
                _lastProcessedRowId = Math.Max(_lastProcessedRowId, message.RowId);
            }
            catch (Exception ex)
            {
                OnError(ex, $"Processing message {message.Guid}");
            }
        }
    }

    private async Task ProcessMessageAsync(IMessageRecord message, CancellationToken cancellationToken)
    {
        // Get chat info if available
        IMessageChat? chat = null;
        if (!string.IsNullOrEmpty(message.GroupTitle))
        {
            // This is a group message, try to get chat info
            // Note: We'd need the chat GUID which isn't in the message record
            // In a real implementation, we'd join with chat_message_join table
        }

        // Get handle info
        IMessageHandle? handle = null;
        try
        {
            handle = await _database.GetHandleByIdAsync(message.HandleId, cancellationToken);
        }
        catch
        {
            // Handle might not exist
        }

        // Check if we should process this message
        if (!_processor.ShouldProcessMessage(message, chat))
        {
            Console.WriteLine($"[IMessageMonitor] Skipping message {message.Guid} (filtered)");
            return;
        }

        // Normalize the message
        var normalizedMessage = _processor.NormalizeMessage(message, chat, handle);

        Console.WriteLine($"[IMessageMonitor] Processing message from {normalizedMessage.SenderName}: {normalizedMessage.Content}");

        // Raise event
        OnMessageReceived(normalizedMessage, message, chat, handle);
    }

    private void OnMessageReceived(NormalizedMessage message, IMessageRecord rawMessage, IMessageChat? chat, IMessageHandle? handle)
    {
        MessageReceived?.Invoke(this, new MessageReceivedEventArgs
        {
            Message = message,
            RawMessage = rawMessage,
            Chat = chat,
            Handle = handle
        });
    }

    private void OnError(Exception exception, string context)
    {
        Console.WriteLine($"[IMessageMonitor] Error in {context}: {exception.Message}");
        Error?.Invoke(this, new MonitorErrorEventArgs
        {
            Exception = exception,
            Context = context
        });
    }

    public void Dispose()
    {
        if (_disposed) return;

        _cts.Cancel();
        _cts.Dispose();
        _monitorTask?.Wait(TimeSpan.FromSeconds(5));
        
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
