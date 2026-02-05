using Pi.CodingAgent.Skills;

Console.WriteLine("╔═════════════════════════════════════════╗");
Console.WriteLine("║  Pi Skills System Demo                 ║");
Console.WriteLine("╚═════════════════════════════════════════╝");
Console.WriteLine();

// Load skills from default locations
Console.WriteLine("Loading skills from default locations...");
Console.WriteLine();

var result = SkillLoader.LoadSkills(new LoadSkillsOptions
{
    Cwd = Directory.GetCurrentDirectory(),
    IncludeDefaults = true
});

Console.WriteLine($"✓ Loaded {result.Skills.Count} skill(s)");
Console.WriteLine();

if (result.Diagnostics.Count > 0)
{
    Console.WriteLine($"⚠ {result.Diagnostics.Count} diagnostic(s):");
    foreach (var diag in result.Diagnostics)
    {
        Console.WriteLine($"  [{diag.Type}] {diag.Message}");
        Console.WriteLine($"           Path: {diag.Path}");
        if (diag.Collision != null)
        {
            Console.WriteLine($"           Winner: {diag.Collision.WinnerPath}");
            Console.WriteLine($"           Loser: {diag.Collision.LoserPath}");
        }
    }
    Console.WriteLine();
}

Console.WriteLine("Skills discovered:");
Console.WriteLine();

foreach (var skill in result.Skills)
{
    Console.WriteLine($"┌─ {skill.Name}");
    Console.WriteLine($"│  Description: {skill.Description}");
    Console.WriteLine($"│  Source: {skill.Source}");
    Console.WriteLine($"│  Location: {skill.FilePath}");
    Console.WriteLine($"│  Base Dir: {skill.BaseDir}");
    Console.WriteLine($"│  Disable Model Invocation: {skill.DisableModelInvocation}");
    Console.WriteLine("└─");
    Console.WriteLine();
}

// Format skills for system prompt
Console.WriteLine("═══════════════════════════════════════════");
Console.WriteLine("System Prompt Format:");
Console.WriteLine("═══════════════════════════════════════════");
var promptText = SkillFormatter.FormatSkillsForPrompt(result.Skills);
Console.WriteLine(promptText);
Console.WriteLine();

Console.WriteLine("✓ Skills system demo complete!");
