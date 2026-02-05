using Pi.AI;
using System.Runtime.CompilerServices;
using System.Threading.Channels;

namespace Pi.Agent;

/// <summary>
/// Main agent execution loop with LLM integration and tool calling
/// </summary>
public class AgentLoop
{
    private readonly AgentLoopConfig _config;
    private readonly SimpleStreamFunction _streamFunction;
    private int _turnCount;
    private const int MaxTurns = 25;

    public AgentLoop(AgentLoopConfig config, SimpleStreamFunction streamFunction)
    {
        _config = config;
        _streamFunction = streamFunction;
        _turnCount = 0;
    }

    /// <summary>
    /// Run the agent loop until completion or max turns
    /// </summary>
    public async IAsyncEnumerable<AgentEvent> RunAsync(
        string systemPrompt,
        List<IAgentMessage> messages,
        List<AgentTool> tools,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var channel = Channel.CreateUnbounded<AgentEvent>();
        var runTask = RunLoopAsync(systemPrompt, messages, tools, channel.Writer, cancellationToken);

        await foreach (var evt in channel.Reader.ReadAllAsync(cancellationToken))
        {
            yield return evt;
        }

        await runTask;
    }

    private async Task RunLoopAsync(
        string systemPrompt,
        List<IAgentMessage> messages,
        List<AgentTool> tools,
        ChannelWriter<AgentEvent> writer,
        CancellationToken cancellationToken)
    {
        try
        {
            await writer.WriteAsync(new AgentStartEvent(), cancellationToken);

            _turnCount = 0;

            while (_turnCount < MaxTurns)
            {
                cancellationToken.ThrowIfCancellationRequested();

                await writer.WriteAsync(new TurnStartEvent(), cancellationToken);

                // Apply context transformation if provided
                if (_config.TransformContext != null)
                {
                    messages = await _config.TransformContext(messages, cancellationToken);
                }

                // Convert messages to LLM format
                var llmMessages = await _config.ConvertToLlm(messages);

                // Create context
                var context = new Context
                {
                    SystemPrompt = systemPrompt,
                    Messages = llmMessages,
                    Tools = tools.Select(t => new Tool
                    {
                        Name = t.Name,
                        Description = t.Description,
                        Parameters = t.Parameters
                    }).ToList()
                };

                // Get API key if provider function exists
                string? apiKey = null;
                if (_config.GetApiKey != null)
                {
                    apiKey = await _config.GetApiKey(_config.Model.Provider);
                }

                var options = _config.StreamOptions ?? new SimpleStreamOptions();
                if (!string.IsNullOrEmpty(apiKey))
                {
                    options = options with { ApiKey = apiKey };
                }

                // Stream LLM response
                AssistantMessage? assistantMessage = null;
                var toolCalls = new List<ToolCall>();

                await foreach (var evt in _streamFunction.StreamSimple(_config.Model, context, options, cancellationToken))
                {
                    switch (evt)
                    {
                        case AssistantMessageStartEvent start:
                            assistantMessage = start.Partial;
                            await writer.WriteAsync(new MessageStartEvent { Message = new AssistantAgentMessage(assistantMessage) }, cancellationToken);
                            break;

                        case TextDeltaEvent delta:
                            if (assistantMessage != null)
                            {
                                assistantMessage = delta.Partial;
                                await writer.WriteAsync(new MessageUpdateEvent
                                {
                                    Message = new AssistantAgentMessage(assistantMessage),
                                    AssistantMessageEvent = evt
                                }, cancellationToken);
                            }
                            break;

                        case ToolCallEndEvent toolEnd:
                            toolCalls.Add(toolEnd.ToolCall);
                            break;

                        case DoneEvent done:
                            assistantMessage = done.Message;
                            await writer.WriteAsync(new MessageEndEvent { Message = new AssistantAgentMessage(assistantMessage) }, cancellationToken);
                            break;

                        case ErrorEvent error:
                            assistantMessage = error.Error;
                            await writer.WriteAsync(new MessageEndEvent { Message = new AssistantAgentMessage(assistantMessage) }, cancellationToken);
                            break;
                    }
                }

                if (assistantMessage == null)
                {
                    break;
                }

                messages.Add(new AssistantAgentMessage(assistantMessage));

                // Check for steering messages
                List<IAgentMessage>? steeringMessages = null;
                if (_config.GetSteeringMessages != null)
                {
                    steeringMessages = await _config.GetSteeringMessages();
                }

                if (steeringMessages != null && steeringMessages.Count > 0)
                {
                    messages.AddRange(steeringMessages);
                    await writer.WriteAsync(new TurnEndEvent
                    {
                        Message = new AssistantAgentMessage(assistantMessage),
                        ToolResults = new List<ToolResultMessage>()
                    }, cancellationToken);
                    _turnCount++;
                    continue;
                }

                // Execute tools
                var toolResults = new List<ToolResultMessage>();
                if (toolCalls.Count > 0 && assistantMessage.StopReason == StopReason.ToolUse)
                {
                    foreach (var toolCall in toolCalls)
                    {
                        var tool = tools.FirstOrDefault(t => t.Name == toolCall.Name);
                        if (tool == null)
                        {
                            var errorResult = new ToolResultMessage
                            {
                                ToolCallId = toolCall.Id,
                                ToolName = toolCall.Name,
                                Content = new List<object> { new TextContent { Text = $"Tool '{toolCall.Name}' not found" } },
                                IsError = true,
                                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                            };
                            toolResults.Add(errorResult);
                            messages.Add(new ToolResultAgentMessage(errorResult));
                            continue;
                        }

                        await writer.WriteAsync(new ToolExecutionStartEvent
                        {
                            ToolCallId = toolCall.Id,
                            ToolName = toolCall.Name,
                            Args = toolCall.Arguments
                        }, cancellationToken);

                        try
                        {
                            var result = await tool.Execute(toolCall.Id, toolCall.Arguments, cancellationToken, null);

                            var toolResult = new ToolResultMessage
                            {
                                ToolCallId = toolCall.Id,
                                ToolName = toolCall.Name,
                                Content = result.Content,
                                Details = result.Details,
                                IsError = false,
                                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                            };
                            toolResults.Add(toolResult);
                            messages.Add(new ToolResultAgentMessage(toolResult));

                            await writer.WriteAsync(new ToolExecutionEndEvent
                            {
                                ToolCallId = toolCall.Id,
                                Result = result
                            }, cancellationToken);
                        }
                        catch (Exception ex)
                        {
                            var errorResult = new ToolResultMessage
                            {
                                ToolCallId = toolCall.Id,
                                ToolName = toolCall.Name,
                                Content = new List<object> { new TextContent { Text = $"Error: {ex.Message}" } },
                                IsError = true,
                                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                            };
                            toolResults.Add(errorResult);
                            messages.Add(new ToolResultAgentMessage(errorResult));
                        }
                    }
                }

                await writer.WriteAsync(new TurnEndEvent
                {
                    Message = new AssistantAgentMessage(assistantMessage),
                    ToolResults = toolResults
                }, cancellationToken);

                _turnCount++;

                // Check if done
                if (assistantMessage.StopReason == StopReason.Stop && toolCalls.Count == 0)
                {
                    List<IAgentMessage>? followUpMessages = null;
                    if (_config.GetFollowUpMessages != null)
                    {
                        followUpMessages = await _config.GetFollowUpMessages();
                    }

                    if (followUpMessages == null || followUpMessages.Count == 0)
                    {
                        break;
                    }

                    messages.AddRange(followUpMessages);
                }
            }

            await writer.WriteAsync(new AgentEndEvent { Messages = messages }, cancellationToken);
        }
        finally
        {
            writer.Complete();
        }
    }
}

// Agent message implementations
public record AssistantAgentMessage(AssistantMessage Message) : IAgentMessage
{
    public string Role => "assistant";
    public long Timestamp => Message.Timestamp;
}

public record ToolResultAgentMessage(ToolResultMessage Message) : IAgentMessage
{
    public string Role => "toolResult";
    public long Timestamp => Message.Timestamp;
}
