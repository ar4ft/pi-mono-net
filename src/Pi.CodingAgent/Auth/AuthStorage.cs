using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Pi.CodingAgent.Auth;

/// <summary>
/// Represents stored credentials
/// </summary>
public class StoredCredential
{
    [JsonPropertyName("provider")]
    public string Provider { get; set; } = string.Empty;

    [JsonPropertyName("key")]
    public string Key { get; set; } = string.Empty;

    [JsonPropertyName("encrypted_value")]
    public string EncryptedValue { get; set; } = string.Empty;

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// Represents a cached token with expiration
/// </summary>
public class CachedToken
{
    [JsonPropertyName("provider")]
    public string Provider { get; set; } = string.Empty;

    [JsonPropertyName("token_type")]
    public string TokenType { get; set; } = string.Empty;

    [JsonPropertyName("encrypted_token")]
    public string EncryptedToken { get; set; } = string.Empty;

    [JsonPropertyName("expires_at")]
    public DateTime? ExpiresAt { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Secure storage for credentials and tokens using encryption
/// </summary>
public class AuthStorage
{
    private readonly string _storageDirectory;
    private readonly byte[] _entropy;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public AuthStorage(string? storageDirectory = null)
    {
        _storageDirectory = storageDirectory ?? GetDefaultStorageDirectory();
        EnsureDirectoryExists();
        
        // Use machine-specific entropy for additional security
        _entropy = Encoding.UTF8.GetBytes(Environment.MachineName);
    }

    private static string GetDefaultStorageDirectory()
    {
        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        return Path.Combine(home, ".pi", "auth");
    }

    private void EnsureDirectoryExists()
    {
        if (!Directory.Exists(_storageDirectory))
        {
            Directory.CreateDirectory(_storageDirectory);
            
            // Set directory permissions (Unix-like systems)
            if (!OperatingSystem.IsWindows())
            {
                try
                {
                    File.SetUnixFileMode(_storageDirectory, 
                        UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);
                }
                catch
                {
                    // Ignore if not supported
                }
            }
        }
    }

    private string GetCredentialPath(string provider, string key)
    {
        var fileName = $"{provider}_{key}.json";
        return Path.Combine(_storageDirectory, "credentials", fileName);
    }

    private string GetTokenCachePath(string provider, string tokenType)
    {
        var fileName = $"{provider}_{tokenType}.json";
        return Path.Combine(_storageDirectory, "tokens", fileName);
    }

    /// <summary>
    /// Encrypts sensitive data using DPAPI (Windows) or simple XOR (other platforms)
    /// </summary>
    [SupportedOSPlatform("windows")]
    private string EncryptWindows(string plainText)
    {
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var encryptedBytes = ProtectedData.Protect(plainBytes, _entropy, DataProtectionScope.CurrentUser);
        return Convert.ToBase64String(encryptedBytes);
    }

    private string Encrypt(string plainText)
    {
        if (OperatingSystem.IsWindows())
        {
            return EncryptWindows(plainText);
        }
        else
        {
            // Simple XOR encryption for non-Windows (not as secure, but better than nothing)
            // In production, consider using a proper encryption library
            var plainBytes = Encoding.UTF8.GetBytes(plainText);
            var key = Encoding.UTF8.GetBytes(Environment.MachineName + Environment.UserName);
            var encrypted = new byte[plainBytes.Length];
            
            for (int i = 0; i < plainBytes.Length; i++)
            {
                encrypted[i] = (byte)(plainBytes[i] ^ key[i % key.Length]);
            }

            return Convert.ToBase64String(encrypted);
        }
    }

    /// <summary>
    /// Decrypts sensitive data (Windows-specific)
    /// </summary>
    [SupportedOSPlatform("windows")]
    private string DecryptWindows(string encryptedText)
    {
        var encryptedBytes = Convert.FromBase64String(encryptedText);
        var decryptedBytes = ProtectedData.Unprotect(encryptedBytes, _entropy, DataProtectionScope.CurrentUser);
        return Encoding.UTF8.GetString(decryptedBytes);
    }

    /// <summary>
    /// Decrypts sensitive data
    /// </summary>
    private string Decrypt(string encryptedText)
    {
        if (OperatingSystem.IsWindows())
        {
            return DecryptWindows(encryptedText);
        }
        else
        {
            // Simple XOR decryption for non-Windows
            var encryptedBytes = Convert.FromBase64String(encryptedText);
            var key = Encoding.UTF8.GetBytes(Environment.MachineName + Environment.UserName);
            var decrypted = new byte[encryptedBytes.Length];
            
            for (int i = 0; i < encryptedBytes.Length; i++)
            {
                decrypted[i] = (byte)(encryptedBytes[i] ^ key[i % key.Length]);
            }

            return Encoding.UTF8.GetString(decrypted);
        }
    }

    public async Task StoreCredentialAsync(string provider, string key, string value)
    {
        var credential = new StoredCredential
        {
            Provider = provider,
            Key = key,
            EncryptedValue = Encrypt(value),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var path = GetCredentialPath(provider, key);
        var directory = Path.GetDirectoryName(path);
        
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var json = JsonSerializer.Serialize(credential, JsonOptions);
        await File.WriteAllTextAsync(path, json);
    }

    public async Task<string?> GetCredentialAsync(string provider, string key)
    {
        var path = GetCredentialPath(provider, key);
        
        if (!File.Exists(path))
        {
            return null;
        }

        try
        {
            var json = await File.ReadAllTextAsync(path);
            var credential = JsonSerializer.Deserialize<StoredCredential>(json, JsonOptions);
            
            if (credential == null)
            {
                return null;
            }

            return Decrypt(credential.EncryptedValue);
        }
        catch
        {
            return null;
        }
    }

    public async Task DeleteCredentialAsync(string provider, string key)
    {
        var path = GetCredentialPath(provider, key);
        
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }

    public async Task<List<(string Provider, string Key)>> ListCredentialsAsync()
    {
        var credentials = new List<(string, string)>();
        var credentialsDir = Path.Combine(_storageDirectory, "credentials");

        if (!Directory.Exists(credentialsDir))
        {
            return credentials;
        }

        var files = Directory.GetFiles(credentialsDir, "*.json");
        
        foreach (var file in files)
        {
            try
            {
                var json = await File.ReadAllTextAsync(file);
                var credential = JsonSerializer.Deserialize<StoredCredential>(json, JsonOptions);
                
                if (credential != null)
                {
                    credentials.Add((credential.Provider, credential.Key));
                }
            }
            catch
            {
                // Skip corrupted files
            }
        }

        return credentials;
    }

    public async Task CacheTokenAsync(string provider, string tokenType, string token, DateTime? expiresAt = null)
    {
        var cachedToken = new CachedToken
        {
            Provider = provider,
            TokenType = tokenType,
            EncryptedToken = Encrypt(token),
            ExpiresAt = expiresAt,
            CreatedAt = DateTime.UtcNow
        };

        var path = GetTokenCachePath(provider, tokenType);
        var directory = Path.GetDirectoryName(path);
        
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var json = JsonSerializer.Serialize(cachedToken, JsonOptions);
        await File.WriteAllTextAsync(path, json);
    }

    public async Task<string?> GetCachedTokenAsync(string provider, string tokenType)
    {
        var path = GetTokenCachePath(provider, tokenType);
        
        if (!File.Exists(path))
        {
            return null;
        }

        try
        {
            var json = await File.ReadAllTextAsync(path);
            var cachedToken = JsonSerializer.Deserialize<CachedToken>(json, JsonOptions);
            
            if (cachedToken == null)
            {
                return null;
            }

            // Check if token is expired
            if (cachedToken.ExpiresAt.HasValue && cachedToken.ExpiresAt.Value <= DateTime.UtcNow)
            {
                // Token expired, delete it
                File.Delete(path);
                return null;
            }

            return Decrypt(cachedToken.EncryptedToken);
        }
        catch
        {
            return null;
        }
    }

    public async Task ClearTokenCacheAsync(string provider, string tokenType)
    {
        var path = GetTokenCachePath(provider, tokenType);
        
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }

    public async Task ClearAllTokensAsync()
    {
        var tokensDir = Path.Combine(_storageDirectory, "tokens");
        
        if (Directory.Exists(tokensDir))
        {
            Directory.Delete(tokensDir, true);
        }
    }
}
