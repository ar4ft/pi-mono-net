using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Serialization;

namespace Pi.AI.Auth;

public record GitHubCopilotCredentials
{
    public required string AccessToken { get; init; }
    public required string CopilotToken { get; init; }
    public required long ExpiresAt { get; init; }
    public string? EnterpriseDomain { get; init; }
}

public class GitHubCopilotAuth
{
    private readonly HttpClient _httpClient;
    private static readonly string ClientId = Encoding.UTF8.GetString(Convert.FromBase64String("SXYxLmI1MDdhMDhjODdlY2ZlOTg="));
    
    private static readonly Dictionary<string, string> CopilotHeaders = new()
    {
        ["User-Agent"] = "GitHubCopilotChat/0.35.0",
        ["Editor-Version"] = "vscode/1.107.0",
        ["Editor-Plugin-Version"] = "copilot-chat/0.35.0",
        ["Copilot-Integration-Id"] = "vscode-chat"
    };

    public GitHubCopilotAuth(HttpClient? httpClient = null)
    {
        _httpClient = httpClient ?? new HttpClient();
    }

    public async Task<DeviceCodeResponse> StartDeviceFlowAsync(string domain = "github.com", CancellationToken cancellationToken = default)
    {
        var urls = GetUrls(domain);
        var request = new HttpRequestMessage(HttpMethod.Post, urls.DeviceCodeUrl)
        {
            Content = JsonContent.Create(new { client_id = ClientId, scope = "read:user" })
        };
        
        request.Headers.Add("Accept", "application/json");
        request.Headers.Add("User-Agent", "GitHubCopilotChat/0.35.0");

        var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var data = await response.Content.ReadFromJsonAsync<DeviceCodeResponse>(cancellationToken);
        return data ?? throw new InvalidOperationException("Invalid device code response");
    }

    public async Task<string> PollForAccessTokenAsync(string domain, string deviceCode, int intervalSeconds, int expiresIn, CancellationToken cancellationToken = default)
    {
        var urls = GetUrls(domain);
        var deadline = DateTimeOffset.UtcNow.AddSeconds(expiresIn);
        var intervalMs = Math.Max(1000, intervalSeconds * 1000);

        while (DateTimeOffset.UtcNow < deadline)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var request = new HttpRequestMessage(HttpMethod.Post, urls.AccessTokenUrl)
            {
                Content = JsonContent.Create(new { client_id = ClientId, device_code = deviceCode, grant_type = "urn:ietf:params:oauth:grant-type:device_code" })
            };
            
            request.Headers.Add("Accept", "application/json");
            request.Headers.Add("User-Agent", "GitHubCopilotChat/0.35.0");

            var response = await _httpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<DeviceTokenResponse>(cancellationToken);
            
            if (result?.AccessToken != null) return result.AccessToken;

            if (result?.Error != null)
            {
                if (result.Error == "authorization_pending") { await Task.Delay(intervalMs, cancellationToken); continue; }
                if (result.Error == "slow_down") { intervalMs += 5000; await Task.Delay(intervalMs, cancellationToken); continue; }
                throw new InvalidOperationException($"Device flow failed: {result.Error}");
            }

            await Task.Delay(intervalMs, cancellationToken);
        }

        throw new TimeoutException("Device flow timed out");
    }

    public async Task<GitHubCopilotCredentials> GetCopilotTokenAsync(string accessToken, string? enterpriseDomain = null, CancellationToken cancellationToken = default)
    {
        var domain = enterpriseDomain ?? "github.com";
        var urls = GetUrls(domain);

        var request = new HttpRequestMessage(HttpMethod.Get, urls.CopilotTokenUrl);
        request.Headers.Add("Accept", "application/json");
        request.Headers.Add("Authorization", $"Bearer {accessToken}");
        
        foreach (var header in CopilotHeaders) request.Headers.Add(header.Key, header.Value);

        var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<CopilotTokenResponse>(cancellationToken);
        if (result?.Token == null) throw new InvalidOperationException("Invalid Copilot token response");

        return new GitHubCopilotCredentials { AccessToken = accessToken, CopilotToken = result.Token, ExpiresAt = result.ExpiresAt, EnterpriseDomain = enterpriseDomain };
    }

    public static string GetBaseUrlFromToken(string token)
    {
        var match = System.Text.RegularExpressions.Regex.Match(token, @"proxy-ep=([^;]+)");
        if (match.Success)
        {
            var proxyHost = match.Groups[1].Value;
            return $"https://{proxyHost.Replace("proxy.", "api.")}";
        }
        return "https://api.individual.githubcopilot.com";
    }

    private static (string DeviceCodeUrl, string AccessTokenUrl, string CopilotTokenUrl) GetUrls(string domain) =>
        ($"https://{domain}/login/device/code", $"https://{domain}/login/oauth/access_token", $"https://api.{domain}/copilot_internal/v2/token");
}

public record DeviceCodeResponse
{
    [JsonPropertyName("device_code")] public required string DeviceCode { get; init; }
    [JsonPropertyName("user_code")] public required string UserCode { get; init; }
    [JsonPropertyName("verification_uri")] public required string VerificationUri { get; init; }
    [JsonPropertyName("interval")] public required int Interval { get; init; }
    [JsonPropertyName("expires_in")] public required int ExpiresIn { get; init; }
}

internal record DeviceTokenResponse
{
    [JsonPropertyName("access_token")] public string? AccessToken { get; init; }
    [JsonPropertyName("error")] public string? Error { get; init; }
}

internal record CopilotTokenResponse
{
    [JsonPropertyName("token")] public required string Token { get; init; }
    [JsonPropertyName("expires_at")] public required long ExpiresAt { get; init; }
}
