using System.Runtime.CompilerServices;
using System.Threading.Channels;
using Pi.AI.Auth;

namespace Pi.AI.Providers;

/// <summary>
/// GitHub Copilot provider using OpenAI-compatible API
/// </summary>
public class GitHubCopilotProvider : OpenAIProvider
{
    private readonly GitHubCopilotAuth _auth;
    private GitHubCopilotCredentials? _credentials;

    public GitHubCopilotProvider(HttpClient? httpClient = null) : base(httpClient)
    {
        _auth = new GitHubCopilotAuth(httpClient);
    }

    /// <summary>
    /// Authenticate with GitHub Copilot and get credentials
    /// </summary>
    public async Task<GitHubCopilotCredentials> AuthenticateAsync(CancellationToken cancellationToken = default)
    {
        var deviceCode = await _auth.StartDeviceFlowAsync(cancellationToken: cancellationToken);
        
        Console.WriteLine($"\nGitHub Copilot Authentication");
        Console.WriteLine($"===============================");
        Console.WriteLine($"Please visit: {deviceCode.VerificationUri}");
        Console.WriteLine($"And enter code: {deviceCode.UserCode}");
        Console.WriteLine($"\nWaiting for authorization...");

        var accessToken = await _auth.PollForAccessTokenAsync(
            "github.com",
            deviceCode.DeviceCode,
            deviceCode.Interval,
            deviceCode.ExpiresIn,
            cancellationToken);

        _credentials = await _auth.GetCopilotTokenAsync(accessToken, cancellationToken: cancellationToken);
        
        Console.WriteLine("✓ Authentication successful!");
        return _credentials;
    }

    /// <summary>
    /// Set credentials manually (e.g., from saved session)
    /// </summary>
    public void SetCredentials(GitHubCopilotCredentials credentials)
    {
        _credentials = credentials;
    }

    /// <summary>
    /// Get current credentials (may be null if not authenticated)
    /// </summary>
    public GitHubCopilotCredentials? GetCredentials() => _credentials;

    public override async IAsyncEnumerable<AssistantMessageEvent> Stream(
        Model model,
        Context context,
        StreamOptions? options = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (_credentials == null)
        {
            throw new InvalidOperationException("Not authenticated. Call AuthenticateAsync() first.");
        }

        // Check if token needs refresh
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        if (now >= _credentials.ExpiresAt - 60) // Refresh 1 minute before expiry
        {
            Console.WriteLine("Refreshing Copilot token...");
            _credentials = await _auth.GetCopilotTokenAsync(_credentials.AccessToken, _credentials.EnterpriseDomain, cancellationToken);
        }

        // Update model with Copilot base URL and token
        var copilotModel = model with
        {
            BaseUrl = GitHubCopilotAuth.GetBaseUrlFromToken(_credentials.CopilotToken)
        };

        var copilotOptions = options ?? new StreamOptions();
        copilotOptions = copilotOptions with
        {
            ApiKey = _credentials.CopilotToken
        };

        // Use base OpenAI provider implementation
        await foreach (var evt in base.Stream(copilotModel, context, copilotOptions, cancellationToken))
        {
            yield return evt;
        }
    }
}
