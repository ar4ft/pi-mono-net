using System.Text.Json;
using System.Text.Json.Serialization;

namespace Pi.CodingAgent.Settings;

/// <summary>
/// User settings and preferences
/// </summary>
public class UserSettings
{
    [JsonPropertyName("default_model")]
    public string DefaultModel { get; set; } = "gpt-4";

    [JsonPropertyName("default_provider")]
    public string DefaultProvider { get; set; } = "github-copilot";

    [JsonPropertyName("theme")]
    public string Theme { get; set; } = "dark";

    [JsonPropertyName("max_history_messages")]
    public int MaxHistoryMessages { get; set; } = 1000;

    [JsonPropertyName("auto_save_interval")]
    public int AutoSaveInterval { get; set; } = 30;

    [JsonPropertyName("working_directory")]
    public string? WorkingDirectory { get; set; }

    [JsonPropertyName("enable_skills")]
    public bool EnableSkills { get; set; } = true;

    [JsonPropertyName("enable_heartbeat")]
    public bool EnableHeartbeat { get; set; } = true;

    [JsonPropertyName("heartbeat_interval")]
    public string HeartbeatInterval { get; set; } = "30m";

    [JsonPropertyName("editor")]
    public EditorSettings Editor { get; set; } = new();

    [JsonPropertyName("custom")]
    public Dictionary<string, JsonElement> Custom { get; set; } = new();
}

/// <summary>
/// Editor-specific settings
/// </summary>
public class EditorSettings
{
    [JsonPropertyName("tab_size")]
    public int TabSize { get; set; } = 4;

    [JsonPropertyName("use_spaces")]
    public bool UseSpaces { get; set; } = true;

    [JsonPropertyName("word_wrap")]
    public bool WordWrap { get; set; } = true;

    [JsonPropertyName("max_visible_lines")]
    public int MaxVisibleLines { get; set; } = 20;
}
