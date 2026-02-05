using Pi.Channels;
using System.Collections.Concurrent;

namespace Pi.Gateway;

/// <summary>
/// Manages registered channels
/// </summary>
public class ChannelRegistry
{
    private readonly ConcurrentDictionary<string, IChannel> _channels = new();
    private readonly Pi.Channels.ILogger? _logger;
    
    public ChannelRegistry(Pi.Channels.ILogger? logger = null)
    {
        _logger = logger;
    }
    
    /// <summary>
    /// Register a channel
    /// </summary>
    public async Task<bool> RegisterChannelAsync(IChannel channel, CancellationToken cancellationToken = default)
    {
        if (channel == null)
            throw new ArgumentNullException(nameof(channel));
        
        if (_channels.ContainsKey(channel.Id))
        {
            _logger?.LogWarning("Channel {ChannelId} is already registered", channel.Id);
            return false;
        }
        
        if (_channels.TryAdd(channel.Id, channel))
        {
            _logger?.LogInformation("Registered channel {ChannelId} ({ChannelType})", channel.Id, channel.Type);
            
            // Start the channel if not already active
            if (!channel.IsActive)
            {
                await channel.StartAsync(cancellationToken);
            }
            
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// Unregister a channel
    /// </summary>
    public async Task<bool> UnregisterChannelAsync(string channelId, CancellationToken cancellationToken = default)
    {
        if (_channels.TryRemove(channelId, out var channel))
        {
            _logger?.LogInformation("Unregistered channel {ChannelId}", channelId);
            
            // Stop the channel if active
            if (channel.IsActive)
            {
                await channel.StopAsync(cancellationToken);
            }
            
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// Get a channel by ID
    /// </summary>
    public IChannel? GetChannel(string channelId)
    {
        _channels.TryGetValue(channelId, out var channel);
        return channel;
    }
    
    /// <summary>
    /// Get all registered channels
    /// </summary>
    public IEnumerable<IChannel> GetAllChannels()
    {
        return _channels.Values;
    }
    
    /// <summary>
    /// Get channels by type
    /// </summary>
    public IEnumerable<IChannel> GetChannelsByType(string type)
    {
        return _channels.Values.Where(c => c.Type.Equals(type, StringComparison.OrdinalIgnoreCase));
    }
    
    /// <summary>
    /// Get active channels
    /// </summary>
    public IEnumerable<IChannel> GetActiveChannels()
    {
        return _channels.Values.Where(c => c.IsActive);
    }
    
    /// <summary>
    /// Start all channels
    /// </summary>
    public async Task StartAllAsync(CancellationToken cancellationToken = default)
    {
        foreach (var channel in _channels.Values)
        {
            if (!channel.IsActive)
            {
                try
                {
                    await channel.StartAsync(cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Failed to start channel {ChannelId}", channel.Id);
                }
            }
        }
    }
    
    /// <summary>
    /// Stop all channels
    /// </summary>
    public async Task StopAllAsync(CancellationToken cancellationToken = default)
    {
        foreach (var channel in _channels.Values)
        {
            if (channel.IsActive)
            {
                try
                {
                    await channel.StopAsync(cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Failed to stop channel {ChannelId}", channel.Id);
                }
            }
        }
    }
    
    /// <summary>
    /// Get channel count
    /// </summary>
    public int Count => _channels.Count;
}
