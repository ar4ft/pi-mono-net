using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Channels;

namespace Pi.AI.Providers;

/// <summary>
/// OpenAI API provider implementation
/// </summary>
public class OpenAIProvider : StreamFunctionBase
{
    private readonly HttpClient _httpClient;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public OpenAIProvider(HttpClient? httpClient = null)
    {
        _httpClient = httpClient ?? new HttpClient();
    }

    public override async IAsyncEnumerable<AssistantMessageEvent> Stream(
        Model model,
        Context context,
        StreamOptions? options = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var apiKey = options?.ApiKey ?? Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        if (string.IsNullOrEmpty(apiKey))
        {
            throw new InvalidOperationException("OpenAI API key not provided");
        }

        var request = BuildRequest(model, context, options);
        var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{model.BaseUrl}/chat/completions")
        {
            Content = JsonContent.Create(request, options: JsonOptions)
        };
        httpRequest.Headers.Add("Authorization", $"Bearer {apiKey}");

        var startEvent = CreateStartEvent(model);
        yield return startEvent;

        var channel = Channel.CreateUnbounded<AssistantMessageEvent>();
        var streamTask = StreamToChannelAsync(httpRequest, model, startEvent.Partial, channel.Writer, cancellationToken);

        await foreach (var evt in channel.Reader.ReadAllAsync(cancellationToken))
        {
            yield return evt;
        }

        await streamTask;
    }

    private async Task StreamToChannelAsync(
        HttpRequestMessage httpRequest,
        Model model,
        AssistantMessage initialOutput,
        ChannelWriter<AssistantMessageEvent> writer,
        CancellationToken cancellationToken)
    {
        var output = initialOutput;
        
        try
        {
            using var response = await _httpClient.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            response.EnsureSuccessStatusCode();

            using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var reader = new StreamReader(stream);

            string? line;
            while ((line = await reader.ReadLineAsync(cancellationToken)) != null)
            {
                if (string.IsNullOrWhiteSpace(line) || !line.StartsWith("data: "))
                    continue;

                var data = line.Substring(6);
                if (data == "[DONE]")
                    break;

                OpenAIStreamChunk? chunk;
                try
                {
                    chunk = JsonSerializer.Deserialize<OpenAIStreamChunk>(data, JsonOptions);
                }
                catch
                {
                    continue;
                }
                
                if (chunk?.Choices == null || chunk.Choices.Count == 0)
                    continue;

                var choice = chunk.Choices[0];
                var delta = choice.Delta;

                if (!string.IsNullOrEmpty(delta?.Content))
                {
                    var contentIndex = output.Content.Count;
                    
                    if (output.Content.Count == 0 || output.Content[^1] is not TextContent)
                    {
                        output.Content.Add(new TextContent { Text = "" });
                        await writer.WriteAsync(new TextStartEvent { ContentIndex = contentIndex, Partial = output }, cancellationToken);
                    }

                    var existingText = (output.Content[^1] as TextContent)!;
                    output.Content[^1] = existingText with { Text = existingText.Text + delta.Content };
                    await writer.WriteAsync(new TextDeltaEvent { ContentIndex = contentIndex, Delta = delta.Content, Partial = output }, cancellationToken);
                }

                if (choice.FinishReason != null)
                {
                    output = output with { StopReason = MapFinishReason(choice.FinishReason) };
                }

                if (chunk.Usage != null)
                {
                    output = output with
                    {
                        Usage = new Usage
                        {
                            Input = chunk.Usage.PromptTokens,
                            Output = chunk.Usage.CompletionTokens,
                            CacheRead = 0,
                            CacheWrite = 0,
                            TotalTokens = chunk.Usage.TotalTokens,
                            Cost = CalculateCost(model, chunk.Usage)
                        }
                    };
                }
            }

            await writer.WriteAsync(new DoneEvent { Reason = output.StopReason, Message = output }, cancellationToken);
        }
        catch (Exception ex)
        {
            output = output with { ErrorMessage = ex.Message, StopReason = StopReason.Error };
            await writer.WriteAsync(new ErrorEvent { Reason = StopReason.Error, Error = output }, cancellationToken);
        }
        finally
        {
            writer.Complete();
        }
    }

    private static OpenAIRequest BuildRequest(Model model, Context context, StreamOptions? options)
    {
        var messages = new List<OpenAIMessage>();
        if (!string.IsNullOrEmpty(context.SystemPrompt))
        {
            messages.Add(new OpenAIMessage { Role = "system", Content = context.SystemPrompt });
        }

        foreach (var msg in context.Messages)
        {
            if (msg is UserMessage userMsg)
            {
                messages.Add(new OpenAIMessage { Role = "user", Content = userMsg.Content is string str ? str : "[complex content]" });
            }
            else if (msg is AssistantMessage assistantMsg)
            {
                var textContent = string.Join("", assistantMsg.Content.OfType<TextContent>().Select(t => t.Text));
                messages.Add(new OpenAIMessage { Role = "assistant", Content = textContent });
            }
        }

        return new OpenAIRequest { Model = model.Id, Messages = messages, Temperature = options?.Temperature, MaxTokens = options?.MaxTokens, Stream = true };
    }

    private static StopReason MapFinishReason(string reason) => reason switch { "stop" => StopReason.Stop, "length" => StopReason.Length, "tool_calls" => StopReason.ToolUse, _ => StopReason.Stop };

    private static UsageCost CalculateCost(Model model, OpenAIUsage usage)
    {
        var inputCost = (usage.PromptTokens / 1_000_000.0) * model.Cost.Input;
        var outputCost = (usage.CompletionTokens / 1_000_000.0) * model.Cost.Output;
        return new UsageCost { Input = inputCost, Output = outputCost, CacheRead = 0, CacheWrite = 0, Total = inputCost + outputCost };
    }
}

internal record OpenAIRequest
{
    [JsonPropertyName("model")] public required string Model { get; init; }
    [JsonPropertyName("messages")] public required List<OpenAIMessage> Messages { get; init; }
    [JsonPropertyName("temperature")] public double? Temperature { get; init; }
    [JsonPropertyName("max_tokens")] public int? MaxTokens { get; init; }
    [JsonPropertyName("stream")] public bool Stream { get; init; }
}

internal record OpenAIMessage
{
    [JsonPropertyName("role")] public required string Role { get; init; }
    [JsonPropertyName("content")] public required string Content { get; init; }
}

internal record OpenAIStreamChunk
{
    [JsonPropertyName("choices")] public List<OpenAIChoice>? Choices { get; init; }
    [JsonPropertyName("usage")] public OpenAIUsage? Usage { get; init; }
}

internal record OpenAIChoice
{
    [JsonPropertyName("delta")] public OpenAIDelta? Delta { get; init; }
    [JsonPropertyName("finish_reason")] public string? FinishReason { get; init; }
}

internal record OpenAIDelta
{
    [JsonPropertyName("content")] public string? Content { get; init; }
    [JsonPropertyName("tool_calls")] public List<OpenAIToolCall>? ToolCalls { get; init; }
}

internal record OpenAIToolCall
{
    [JsonPropertyName("index")] public int Index { get; init; }
    [JsonPropertyName("id")] public string? Id { get; init; }
    [JsonPropertyName("function")] public OpenAIFunction? Function { get; init; }
}

internal record OpenAIFunction
{
    [JsonPropertyName("name")] public string? Name { get; init; }
    [JsonPropertyName("arguments")] public string? Arguments { get; init; }
}

internal record OpenAIUsage
{
    [JsonPropertyName("prompt_tokens")] public int PromptTokens { get; init; }
    [JsonPropertyName("completion_tokens")] public int CompletionTokens { get; init; }
    [JsonPropertyName("total_tokens")] public int TotalTokens { get; init; }
}
