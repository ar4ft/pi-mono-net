using Pi.AI;
using System.Runtime.CompilerServices;

namespace Pi.Agent;

/// <summary>
/// Main Agent class with tool execution and state management
/// </summary>
public class Agent
{
    private readonly AgentLoopConfig _config;
    private readonly List<Action<AgentEvent>> _subscribers = new();
    private AgentState _state;
    private CancellationTokenSource? _cancellation;

    public AgentState State => _state;

    public Agent(Model model, Func<List<IAgentMessage>, Task<List<object>>> convertToLlm)
    {
        _config = new AgentLoopConfig
        {
            Model = model,
            ConvertToLlm = convertToLlm
        };

        _state = new AgentState
        {
            SystemPrompt = "",
            Model = model,
            ThinkingLevel = ThinkingLevel.Off,
            Tools = new List<AgentTool>(),
            Messages = new List<IAgentMessage>(),
            IsStreaming = false,
            StreamMessage = null,
            PendingToolCalls = new HashSet<string>(),
            Error = null
        };
    }

    /// <summary>
    /// Subscribe to agent events
    /// </summary>
    public IDisposable Subscribe(Action<AgentEvent> handler)
    {
        _subscribers.Add(handler);
        return new Unsubscriber(() => _subscribers.Remove(handler));
    }

    private void Emit(AgentEvent evt)
    {
        foreach (var subscriber in _subscribers)
        {
            subscriber(evt);
        }
    }

    /// <summary>
    /// Set system prompt
    /// </summary>
    public void SetSystemPrompt(string prompt)
    {
        _state = _state with { SystemPrompt = prompt };
    }

    /// <summary>
    /// Set model
    /// </summary>
    public void SetModel(Model model)
    {
        _state = _state with { Model = model };
    }

    /// <summary>
    /// Set thinking level
    /// </summary>
    public void SetThinkingLevel(ThinkingLevel level)
    {
        _state = _state with { ThinkingLevel = level };
    }

    /// <summary>
    /// Set tools
    /// </summary>
    public void SetTools(List<AgentTool> tools)
    {
        _state = _state with { Tools = tools };
    }

    /// <summary>
    /// Append a message to the conversation
    /// </summary>
    public void AppendMessage(IAgentMessage message)
    {
        _state.Messages.Add(message);
    }

    /// <summary>
    /// Clear all messages
    /// </summary>
    public void ClearMessages()
    {
        _state.Messages.Clear();
    }

    /// <summary>
    /// Prompt the agent with a user message
    /// </summary>
    public async Task Prompt(string content, CancellationToken cancellationToken = default)
    {
        var userMessage = new UserAgentMessage
        {
            Content = content,
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        };

        AppendMessage(userMessage);

        await RunLoop(cancellationToken);
    }

    /// <summary>
    /// Abort current operation
    /// </summary>
    public void Abort()
    {
        _cancellation?.Cancel();
    }

    private async Task RunLoop(CancellationToken cancellationToken)
    {
        _cancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        
        try
        {
            var streamFunction = new SimpleStreamFunction();
            // Register provider here based on model API
            // For now, just run a basic loop

            var agentLoop = new AgentLoop(_config, streamFunction);

            await foreach (var evt in agentLoop.RunAsync(
                _state.SystemPrompt,
                _state.Messages,
                _state.Tools,
                _cancellation.Token))
            {
                Emit(evt);
                
                // Update state based on events
                _state = evt switch
                {
                    MessageStartEvent => _state with { IsStreaming = true },
                    MessageEndEvent msgEnd => _state with
                    {
                        IsStreaming = false,
                        StreamMessage = msgEnd.Message
                    },
                    AgentEndEvent => _state with { IsStreaming = false },
                    _ => _state
                };
            }
        }
        finally
        {
            _cancellation?.Dispose();
            _cancellation = null;
        }
    }

    private class Unsubscriber : IDisposable
    {
        private readonly Action _unsubscribe;

        public Unsubscriber(Action unsubscribe)
        {
            _unsubscribe = unsubscribe;
        }

        public void Dispose()
        {
            _unsubscribe();
        }
    }
}

/// <summary>
/// User agent message implementation
/// </summary>
public record UserAgentMessage : IAgentMessage
{
    public string Role => "user";
    public required object Content { get; init; }
    public required long Timestamp { get; init; }
}
