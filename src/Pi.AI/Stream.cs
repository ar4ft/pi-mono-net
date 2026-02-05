using System.Runtime.CompilerServices;

namespace Pi.AI;

/// <summary>
/// Interface for LLM streaming functions
/// </summary>
public interface IStreamFunction
{
    IAsyncEnumerable<AssistantMessageEvent> Stream(
        Model model,
        Context context,
        StreamOptions? options = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Base class for provider-specific stream implementations
/// </summary>
public abstract class StreamFunctionBase : IStreamFunction
{
    public abstract IAsyncEnumerable<AssistantMessageEvent> Stream(
        Model model,
        Context context,
        StreamOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Helper to create start event
    /// </summary>
    protected static AssistantMessageStartEvent CreateStartEvent(Model model)
    {
        return new AssistantMessageStartEvent
        {
            Partial = new AssistantMessage
            {
                Content = new List<object>(),
                Api = model.Api,
                Provider = model.Provider,
                Model = model.Id,
                Usage = new Usage
                {
                    Input = 0,
                    Output = 0,
                    CacheRead = 0,
                    CacheWrite = 0,
                    TotalTokens = 0,
                    Cost = new UsageCost
                    {
                        Input = 0,
                        Output = 0,
                        CacheRead = 0,
                        CacheWrite = 0,
                        Total = 0
                    }
                },
                StopReason = StopReason.Stop,
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            }
        };
    }
}

/// <summary>
/// Simple stream function for basic usage
/// </summary>
public class SimpleStreamFunction
{
    private readonly Dictionary<string, IStreamFunction> _providers = new();

    public void RegisterProvider(string api, IStreamFunction streamFunction)
    {
        _providers[api] = streamFunction;
    }

    public async IAsyncEnumerable<AssistantMessageEvent> StreamSimple(
        Model model,
        Context context,
        SimpleStreamOptions? options = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (!_providers.TryGetValue(model.Api, out var provider))
        {
            throw new NotSupportedException($"API '{model.Api}' is not supported");
        }

        await foreach (var evt in provider.Stream(model, context, options, cancellationToken))
        {
            yield return evt;
        }
    }
}
