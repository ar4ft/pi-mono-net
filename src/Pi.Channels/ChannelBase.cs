namespace Pi.Channels;

/// <summary>
/// Abstract base class for channel implementations
/// </summary>
public abstract class ChannelBase : IChannel
{
    protected readonly ILogger? _logger;
    protected bool _isActive;
    protected CancellationTokenSource? _cancellationTokenSource;
    
    protected ChannelBase(string id, string type, string name, ILogger? logger = null)
    {
        Id = id ?? throw new ArgumentNullException(nameof(id));
        Type = type ?? throw new ArgumentNullException(nameof(type));
        Name = name ?? throw new ArgumentNullException(nameof(name));
        _logger = logger;
    }
    
    public string Id { get; }
    public string Type { get; }
    public string Name { get; }
    public bool IsActive => _isActive;
    
    public event EventHandler<ChannelMessageEventArgs>? MessageReceived;
    public event EventHandler<ChannelStatusEventArgs>? StatusChanged;
    
    public virtual async Task StartAsync(CancellationToken cancellationToken = default)
    {
        if (_isActive)
        {
            _logger?.LogWarning("Channel {ChannelId} is already active", Id);
            return;
        }
        
        _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        
        try
        {
            _logger?.LogInformation("Starting channel {ChannelId} ({ChannelType})", Id, Type);
            
            await OnStartAsync(_cancellationTokenSource.Token);
            
            _isActive = true;
            RaiseStatusChanged("active", "Channel started successfully");
            
            _logger?.LogInformation("Channel {ChannelId} started successfully", Id);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to start channel {ChannelId}", Id);
            RaiseStatusChanged("error", "Failed to start channel", ex);
            throw;
        }
    }
    
    public virtual async Task StopAsync(CancellationToken cancellationToken = default)
    {
        if (!_isActive)
        {
            _logger?.LogWarning("Channel {ChannelId} is not active", Id);
            return;
        }
        
        try
        {
            _logger?.LogInformation("Stopping channel {ChannelId}", Id);
            
            _cancellationTokenSource?.Cancel();
            
            await OnStopAsync(cancellationToken);
            
            _isActive = false;
            RaiseStatusChanged("stopped", "Channel stopped");
            
            _logger?.LogInformation("Channel {ChannelId} stopped successfully", Id);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error stopping channel {ChannelId}", Id);
            throw;
        }
        finally
        {
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
        }
    }
    
    public virtual async Task SendMessageAsync(ChannelMessage message, CancellationToken cancellationToken = default)
    {
        if (!_isActive)
            throw new InvalidOperationException($"Channel {Id} is not active");
        
        if (message == null)
            throw new ArgumentNullException(nameof(message));
        
        try
        {
            _logger?.LogDebug("Sending message through channel {ChannelId}", Id);
            await OnSendMessageAsync(message, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to send message through channel {ChannelId}", Id);
            throw;
        }
    }
    
    public abstract ChannelCapabilities GetCapabilities();
    
    /// <summary>
    /// Override to implement channel-specific start logic
    /// </summary>
    protected abstract Task OnStartAsync(CancellationToken cancellationToken);
    
    /// <summary>
    /// Override to implement channel-specific stop logic
    /// </summary>
    protected abstract Task OnStopAsync(CancellationToken cancellationToken);
    
    /// <summary>
    /// Override to implement channel-specific send logic
    /// </summary>
    protected abstract Task OnSendMessageAsync(ChannelMessage message, CancellationToken cancellationToken);
    
    /// <summary>
    /// Raise the MessageReceived event
    /// </summary>
    protected void RaiseMessageReceived(ChannelMessage message)
    {
        _logger?.LogDebug("Received message on channel {ChannelId}", Id);
        MessageReceived?.Invoke(this, new ChannelMessageEventArgs { Message = message });
    }
    
    /// <summary>
    /// Raise the StatusChanged event
    /// </summary>
    protected void RaiseStatusChanged(string status, string? message = null, Exception? error = null)
    {
        _logger?.LogDebug("Channel {ChannelId} status changed to {Status}", Id, status);
        StatusChanged?.Invoke(this, new ChannelStatusEventArgs 
        { 
            Status = status, 
            Message = message,
            Error = error
        });
    }
}

/// <summary>
/// Logger interface for channel logging (simplified)
/// </summary>
public interface ILogger
{
    void LogDebug(string message, params object[] args);
    void LogInformation(string message, params object[] args);
    void LogWarning(string message, params object[] args);
    void LogError(Exception? exception, string message, params object[] args);
}
