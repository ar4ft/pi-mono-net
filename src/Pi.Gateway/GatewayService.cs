using Pi.Channels;
using Pi.Agent;
using Pi.AI;
using System.Threading.Channels;

namespace Pi.Gateway;

/// <summary>
/// Central gateway orchestrating messages between channels and agents
/// </summary>
public class GatewayService
{
    private readonly ChannelRegistry _channelRegistry;
    private readonly SessionManager _sessionManager;
    private readonly Pi.Channels.ILogger? _logger;
    private readonly Channel<GatewayMessage> _messageQueue;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private Task? _processingTask;
    
    public GatewayService(Pi.Channels.ILogger? logger = null)
    {
        _channelRegistry = new ChannelRegistry(logger);
        _sessionManager = new SessionManager();
        _logger = logger;
        _messageQueue = Channel.CreateUnbounded<GatewayMessage>();
        _cancellationTokenSource = new CancellationTokenSource();
    }
    
    /// <summary>
    /// Start the gateway
    /// </summary>
    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        _logger?.LogInformation("Starting Gateway");
        
        // Start message processing task
        _processingTask = Task.Run(() => ProcessMessagesAsync(_cancellationTokenSource.Token));
        
        // Start all channels
        await _channelRegistry.StartAllAsync(cancellationToken);
        
        _logger?.LogInformation("Gateway started");
    }
    
    /// <summary>
    /// Stop the gateway
    /// </summary>
    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        _logger?.LogInformation("Stopping Gateway");
        
        // Stop all channels
        await _channelRegistry.StopAllAsync(cancellationToken);
        
        // Stop message processing
        _cancellationTokenSource.Cancel();
        _messageQueue.Writer.Complete();
        
        if (_processingTask != null)
        {
            await _processingTask;
        }
        
        _logger?.LogInformation("Gateway stopped");
    }
    
    /// <summary>
    /// Register a channel with the gateway
    /// </summary>
    public async Task RegisterChannelAsync(IChannel channel, CancellationToken cancellationToken = default)
    {
        await _channelRegistry.RegisterChannelAsync(channel, cancellationToken);
        
        // Subscribe to channel messages
        channel.MessageReceived += OnChannelMessageReceived;
        channel.StatusChanged += OnChannelStatusChanged;
    }
    
    /// <summary>
    /// Unregister a channel from the gateway
    /// </summary>
    public async Task UnregisterChannelAsync(string channelId, CancellationToken cancellationToken = default)
    {
        var channel = _channelRegistry.GetChannel(channelId);
        if (channel != null)
        {
            channel.MessageReceived -= OnChannelMessageReceived;
            channel.StatusChanged -= OnChannelStatusChanged;
        }
        
        await _channelRegistry.UnregisterChannelAsync(channelId, cancellationToken);
    }
    
    /// <summary>
    /// Send a message to a channel
    /// </summary>
    public async Task SendMessageAsync(string channelId, ChannelMessage message, CancellationToken cancellationToken = default)
    {
        var channel = _channelRegistry.GetChannel(channelId);
        if (channel == null)
        {
            _logger?.LogWarning("Channel {ChannelId} not found", channelId);
            return;
        }
        
        await channel.SendMessageAsync(message, cancellationToken);
    }
    
    /// <summary>
    /// Get gateway statistics
    /// </summary>
    public GatewayStatistics GetStatistics()
    {
        return new GatewayStatistics
        {
            TotalChannels = _channelRegistry.Count,
            ActiveChannels = _channelRegistry.GetActiveChannels().Count(),
            ActiveSessions = _sessionManager.Count,
            QueuedMessages = _messageQueue.Reader.Count
        };
    }
    
    private void OnChannelMessageReceived(object? sender, ChannelMessageEventArgs e)
    {
        _logger?.LogDebug("Received message from channel {ChannelId}", e.Message.ChannelId);
        
        // Queue the message for processing
        _messageQueue.Writer.TryWrite(new GatewayMessage
        {
            Type = GatewayMessageType.ChannelMessage,
            ChannelMessage = e.Message,
            Timestamp = DateTime.UtcNow
        });
    }
    
    private void OnChannelStatusChanged(object? sender, ChannelStatusEventArgs e)
    {
        var channel = sender as IChannel;
        _logger?.LogInformation("Channel {ChannelId} status changed to {Status}", channel?.Id, e.Status);
    }
    
    private async Task ProcessMessagesAsync(CancellationToken cancellationToken)
    {
        _logger?.LogInformation("Message processing started");
        
        await foreach (var gatewayMessage in _messageQueue.Reader.ReadAllAsync(cancellationToken))
        {
            try
            {
                await ProcessMessageAsync(gatewayMessage, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error processing message");
            }
        }
        
        _logger?.LogInformation("Message processing stopped");
    }
    
    private async Task ProcessMessageAsync(GatewayMessage gatewayMessage, CancellationToken cancellationToken)
    {
        if (gatewayMessage.ChannelMessage == null)
            return;
        
        var message = gatewayMessage.ChannelMessage;
        
        // Get or create session
        var session = _sessionManager.GetOrCreateSession(
            message.SessionKey,
            message.ChannelId,
            message.ChannelType
        );
        
        // Add to session history
        _sessionManager.AddMessage(message.SessionKey, new UserAgentMessage
        {
            Content = message.Content,
            Timestamp = new DateTimeOffset(message.Timestamp).ToUnixTimeSeconds()
        });
        
        // TODO: Route to agent and process
        // For now, just log
        _logger?.LogInformation("Processing message in session {SessionKey}: {Content}", 
            message.SessionKey, message.Content);
        
        // Echo response (placeholder for actual agent processing)
        var responseMessage = new ChannelMessage
        {
            Id = Guid.NewGuid().ToString(),
            SessionKey = message.SessionKey,
            ChannelId = message.ChannelId,
            ChannelType = message.ChannelType,
            SenderId = "agent",
            Content = $"Echo: {message.Content}",
            Timestamp = DateTime.UtcNow,
            Direction = MessageDirection.Outgoing,
            RecipientIds = new[] { message.SenderId }
        };
        
        await SendMessageAsync(message.ChannelId, responseMessage, cancellationToken);
    }
}

/// <summary>
/// Internal gateway message
/// </summary>
internal record GatewayMessage
{
    public required GatewayMessageType Type { get; init; }
    public ChannelMessage? ChannelMessage { get; init; }
    public required DateTime Timestamp { get; init; }
}

/// <summary>
/// Gateway message type
/// </summary>
internal enum GatewayMessageType
{
    ChannelMessage,
    SystemMessage
}

/// <summary>
/// Gateway statistics
/// </summary>
public record GatewayStatistics
{
    public int TotalChannels { get; init; }
    public int ActiveChannels { get; init; }
    public int ActiveSessions { get; init; }
    public int QueuedMessages { get; init; }
}
