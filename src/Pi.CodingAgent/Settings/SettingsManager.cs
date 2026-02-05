using System.Text.Json;

namespace Pi.CodingAgent.Settings;

/// <summary>
/// Manages user settings persistence and access
/// </summary>
public class SettingsManager
{
    private readonly string _settingsPath;
    private UserSettings _settings;
    private FileSystemWatcher? _watcher;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public UserSettings Settings => _settings;

    public event EventHandler<UserSettings>? SettingsChanged;

    public SettingsManager(string? settingsPath = null)
    {
        _settingsPath = settingsPath ?? GetDefaultSettingsPath();
        _settings = new UserSettings();
    }

    private static string GetDefaultSettingsPath()
    {
        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var piDir = Path.Combine(home, ".pi");
        
        if (!Directory.Exists(piDir))
        {
            Directory.CreateDirectory(piDir);
        }

        return Path.Combine(piDir, "settings.json");
    }

    public async Task LoadAsync()
    {
        if (!File.Exists(_settingsPath))
        {
            // Create default settings
            _settings = new UserSettings();
            await SaveAsync();
            return;
        }

        try
        {
            var json = await File.ReadAllTextAsync(_settingsPath);
            _settings = JsonSerializer.Deserialize<UserSettings>(json, JsonOptions) ?? new UserSettings();
        }
        catch (Exception)
        {
            // If settings are corrupted, use defaults
            _settings = new UserSettings();
            await SaveAsync();
        }
    }

    public async Task SaveAsync()
    {
        try
        {
            var json = JsonSerializer.Serialize(_settings, JsonOptions);
            await File.WriteAllTextAsync(_settingsPath, json);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to save settings: {ex.Message}", ex);
        }
    }

    public void StartWatching()
    {
        var directory = Path.GetDirectoryName(_settingsPath);
        var fileName = Path.GetFileName(_settingsPath);

        if (string.IsNullOrEmpty(directory))
        {
            return;
        }

        _watcher = new FileSystemWatcher(directory, fileName)
        {
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size
        };

        _watcher.Changed += async (s, e) =>
        {
            // Debounce: wait a bit before reloading
            await Task.Delay(500);
            await LoadAsync();
            SettingsChanged?.Invoke(this, _settings);
        };

        _watcher.EnableRaisingEvents = true;
    }

    public void StopWatching()
    {
        _watcher?.Dispose();
        _watcher = null;
    }

    public T? GetCustomSetting<T>(string key, T? defaultValue = default)
    {
        if (_settings.Custom.TryGetValue(key, out var element))
        {
            try
            {
                return JsonSerializer.Deserialize<T>(element.GetRawText(), JsonOptions);
            }
            catch
            {
                return defaultValue;
            }
        }

        return defaultValue;
    }

    public void SetCustomSetting<T>(string key, T value)
    {
        var json = JsonSerializer.SerializeToElement(value, JsonOptions);
        _settings.Custom[key] = json;
    }

    public void Dispose()
    {
        StopWatching();
    }
}
