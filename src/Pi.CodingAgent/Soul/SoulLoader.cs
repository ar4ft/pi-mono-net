namespace Pi.CodingAgent.Soul;

/// <summary>
/// Loads SOUL.md files from workspace or agents directory.
/// SOUL.md defines agent personality, principles, and continuity.
/// </summary>
public static class SoulLoader
{
    /// <summary>
    /// Load SOUL.md from default locations or specified path
    /// </summary>
    public static LoadSoulResult LoadSoul(LoadSoulOptions? options = null)
    {
        options ??= new LoadSoulOptions();
        var cwd = options.Cwd ?? Directory.GetCurrentDirectory();
        var diagnostics = new List<string>();

        // Try explicit path first
        if (!string.IsNullOrEmpty(options.SoulPath))
        {
            var soul = TryLoadFromPath(options.SoulPath, "custom", diagnostics);
            if (soul != null)
                return new LoadSoulResult { Soul = soul, Diagnostics = diagnostics };
        }

        // Try workspace root (SOUL.md)
        var workspaceSoulPath = Path.Combine(cwd, "SOUL.md");
        var workspaceSoul = TryLoadFromPath(workspaceSoulPath, "workspace", diagnostics);
        if (workspaceSoul != null)
            return new LoadSoulResult { Soul = workspaceSoul, Diagnostics = diagnostics };

        // Try .agents/SOUL.md
        var agentsSoulPath = Path.Combine(cwd, ".agents", "SOUL.md");
        var agentsSoul = TryLoadFromPath(agentsSoulPath, "agents", diagnostics);
        if (agentsSoul != null)
            return new LoadSoulResult { Soul = agentsSoul, Diagnostics = diagnostics };

        // Try .agents/soul/SOUL.md
        var agentsSoulDirPath = Path.Combine(cwd, ".agents", "soul", "SOUL.md");
        var agentsSoulDir = TryLoadFromPath(agentsSoulDirPath, "agents", diagnostics);
        if (agentsSoulDir != null)
            return new LoadSoulResult { Soul = agentsSoulDir, Diagnostics = diagnostics };

        // Try home directory (~/.agents/SOUL.md)
        var homePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var homeSoulPath = Path.Combine(homePath, ".agents", "SOUL.md");
        var homeSoul = TryLoadFromPath(homeSoulPath, "user", diagnostics);
        if (homeSoul != null)
            return new LoadSoulResult { Soul = homeSoul, Diagnostics = diagnostics };

        diagnostics.Add("No SOUL.md found in workspace, .agents/, or ~/.agents/");
        return new LoadSoulResult { Soul = null, Diagnostics = diagnostics };
    }

    private static Soul? TryLoadFromPath(string path, string source, List<string> diagnostics)
    {
        try
        {
            if (!File.Exists(path))
                return null;

            var content = File.ReadAllText(path);
            
            if (string.IsNullOrWhiteSpace(content))
            {
                diagnostics.Add($"SOUL.md at {path} is empty");
                return null;
            }

            return new Soul
            {
                Name = "SOUL",
                FilePath = path,
                Content = content,
                Source = source
            };
        }
        catch (Exception ex)
        {
            diagnostics.Add($"Error loading SOUL.md from {path}: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Format SOUL.md content for inclusion in system prompt
    /// </summary>
    public static string FormatForSystemPrompt(Soul soul)
    {
        return $@"<soul>
{soul.Content}
</soul>";
    }
}
