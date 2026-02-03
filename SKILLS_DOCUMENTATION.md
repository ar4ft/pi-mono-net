# Skills System Documentation

## Overview

The Pi .NET Coding Agent now includes a comprehensive skills system that allows specialized instructions to be loaded from markdown files. This system is fully compatible with the Agent Skills specification (https://agentskills.io).

## Quick Start

### Creating a Skill

1. Create a directory in `.agents/skills/`:
   ```bash
   mkdir -p .agents/skills/my-skill
   ```

2. Create `SKILL.md` with frontmatter:
   ```markdown
   ---
   name: my-skill
   description: Brief description of what the skill does
   ---

   # My Skill

   Detailed instructions for the agent go here...
   ```

3. The skill is automatically discovered on next run!

### Using Skills

```csharp
using Pi.CodingAgent.Skills;

// Load all skills from default locations
var result = SkillLoader.LoadSkills(new LoadSkillsOptions
{
    Cwd = Directory.GetCurrentDirectory(),
    IncludeDefaults = true
});

// Display loaded skills
foreach (var skill in result.Skills)
{
    Console.WriteLine($"{skill.Name}: {skill.Description}");
}

// Format for system prompt
var promptText = SkillFormatter.FormatSkillsForPrompt(result.Skills);
```

## Directory Structure

Skills are loaded from multiple locations with precedence:

```
.agents/skills/           # Project-specific skills (highest priority)
~/.agents/skills/         # User-global skills
```

Each skill is a directory containing a `SKILL.md` file:

```
.agents/skills/
├── dotnet-coding/
│   └── SKILL.md
├── git-workflow/
│   └── SKILL.md
└── example-skill/
    └── SKILL.md
```

## SKILL.md Format

### Minimal Example

```markdown
---
name: my-skill
description: Brief description
---

# Skill Content

Instructions for the agent...
```

### Complete Example

```markdown
---
name: my-skill
description: Brief description of what this skill does
license: MIT
compatibility: pi-coding-agent >= 1.0.0
disable-model-invocation: false
metadata:
  author: Your Name
  version: 1.0.0
allowed-tools:
  - read
  - write
---

# My Skill

## Purpose

Explain what this skill helps with...

## Usage

Provide clear instructions...

## Examples

Show concrete examples...
```

## Frontmatter Fields

### Required Fields

- **name**: Skill identifier (lowercase, alphanumeric + hyphens, max 64 chars)
- **description**: Brief description (required, max 1024 chars)

### Optional Fields

- **license**: License identifier (e.g., "MIT", "Apache-2.0")
- **compatibility**: Version compatibility string
- **disable-model-invocation**: If true, skill is not included in system prompt (only invoked explicitly)
- **metadata**: Additional metadata (dictionary)
- **allowed-tools**: List of tools this skill can use

## Validation Rules

### Name Validation

- Must match parent directory name
- Lowercase letters (a-z), numbers (0-9), hyphens (-) only
- No consecutive hyphens (--)
- Must not start or end with hyphen
- Maximum 64 characters

### Description Validation

- Required (cannot be empty or whitespace)
- Maximum 1024 characters

### Examples

✅ **Valid Names:**
- `my-skill`
- `dotnet-coding`
- `git-workflow`
- `skill-123`

❌ **Invalid Names:**
- `My_Skill` (uppercase, underscore)
- `-my-skill` (starts with hyphen)
- `my--skill` (consecutive hyphens)
- `a-very-long-skill-name-that-exceeds-the-maximum-allowed-length-limit` (too long)

## Loading Skills

### Default Locations

```csharp
var result = SkillLoader.LoadSkills();
```

Loads from:
1. `.agents/skills/` (project directory)
2. `~/.agents/skills/` (user directory)

### Custom Locations

```csharp
var result = SkillLoader.LoadSkills(new LoadSkillsOptions
{
    Cwd = "/path/to/project",
    AgentDir = "/custom/agent/dir",
    SkillPaths = new List<string> { "/path/to/skills", "/another/path" },
    IncludeDefaults = false
});
```

### Skill Paths

You can specify:
- **Directories**: Scanned recursively for SKILL.md files
- **Files**: Direct path to a .md file
- **Relative paths**: Resolved from Cwd
- **~ paths**: Resolved from user home directory

## Collision Handling

When multiple skills have the same name:
- First skill wins (based on loading order)
- Collision diagnostic is emitted
- Both winner and loser paths are recorded

Example:
```csharp
var result = SkillLoader.LoadSkills();

foreach (var diag in result.Diagnostics)
{
    if (diag.Type == "collision")
    {
        Console.WriteLine($"Collision: {diag.Collision.Name}");
        Console.WriteLine($"  Winner: {diag.Collision.WinnerPath}");
        Console.WriteLine($"  Loser: {diag.Collision.LoserPath}");
    }
}
```

## System Prompt Integration

Skills are formatted as XML for inclusion in system prompts:

```csharp
var promptText = SkillFormatter.FormatSkillsForPrompt(result.Skills);
```

Output:
```xml
The following skills provide specialized instructions for specific tasks.
Use the read tool to load a skill's file when the task matches its description.

<available_skills>
  <skill>
    <name>git-workflow</name>
    <description>Git workflow and best practices...</description>
    <location>/path/to/.agents/skills/git-workflow/SKILL.md</location>
  </skill>
  ...
</available_skills>
```

### Disabled Skills

Skills with `disable-model-invocation: true` are excluded from the system prompt but can be invoked explicitly via commands.

## Diagnostics

The loader emits diagnostics for various issues:

### Warning Types

- **Invalid name**: Name doesn't follow validation rules
- **Name mismatch**: Name doesn't match directory
- **Missing description**: Description is required
- **Unknown field**: Frontmatter has unrecognized fields
- **Path not found**: Specified skill path doesn't exist
- **Parse error**: YAML or file read error

### Collision Type

- **Name collision**: Multiple skills with same name

### Example

```csharp
var result = SkillLoader.LoadSkills();

foreach (var diag in result.Diagnostics)
{
    Console.WriteLine($"[{diag.Type}] {diag.Message}");
    Console.WriteLine($"  Path: {diag.Path}");
}
```

## Best Practices

### Skill Design

1. **Focus**: Each skill should address one specific task or domain
2. **Clarity**: Write clear, actionable instructions
3. **Examples**: Include concrete examples
4. **Tools**: Reference any tools or resources needed
5. **Paths**: Use relative paths, resolved against skill directory

### Directory Organization

```
.agents/skills/
├── backend/
│   ├── database-queries/
│   │   └── SKILL.md
│   └── api-design/
│       └── SKILL.md
└── frontend/
    ├── react-components/
    │   └── SKILL.md
    └── css-styling/
        └── SKILL.md
```

### Naming Conventions

- Use descriptive, hyphenated names
- Group related skills in subdirectories
- Keep names concise but meaningful

## Example Skills

### .NET Coding

```markdown
---
name: dotnet-coding
description: Best practices for C# and .NET development
---

# .NET Coding Standards

## Code Style
- Use PascalCase for types and public members
- Use camelCase with underscore for private fields
- Prefer var when type is obvious

## Modern C#
- Use record types for immutable data
- Leverage pattern matching
- Use nullable reference types
...
```

### Git Workflow

```markdown
---
name: git-workflow
description: Git workflow and version control best practices
---

# Git Workflow

## Commit Messages
Follow conventional commits format:
- feat: New feature
- fix: Bug fix
- docs: Documentation changes
...
```

## API Reference

### SkillLoader

```csharp
// Load skills from default locations
LoadSkillsResult LoadSkills(LoadSkillsOptions? options = null)

// Load skills from a specific directory
LoadSkillsResult LoadSkillsFromDir(string dir, string source)
```

### SkillFormatter

```csharp
// Format skills for system prompt (XML)
string FormatSkillsForPrompt(List<Skill> skills)
```

### SkillValidator

```csharp
// Validate skill name
List<string> ValidateName(string name, string parentDirName)

// Validate description
List<string> ValidateDescription(string? description)

// Validate frontmatter fields
List<string> ValidateFrontmatterFields(List<string> keys)
```

### FrontmatterParser

```csharp
// Parse frontmatter from markdown
(SkillFrontmatter Frontmatter, string Body) ParseFrontmatter(string content)

// Strip frontmatter, return body only
string StripFrontmatter(string content)

// Get all frontmatter keys
List<string> GetFrontmatterKeys(string content)
```

## Testing

Run skills system tests:

```bash
dotnet test tests/Pi.CodingAgent.Tests/Pi.CodingAgent.Tests.csproj
```

Test coverage includes:
- Name validation (11 tests)
- Frontmatter parsing (5 tests)
- Skill formatting (4 tests)

## Troubleshooting

### Skill Not Loading

1. **Check name format**: Must match directory and validation rules
2. **Check description**: Required in frontmatter
3. **Check YAML**: Frontmatter must be valid YAML between `---` markers
4. **Check path**: Verify skill is in `.agents/skills/` or `~/.agents/skills/`

### Diagnostics Help

```csharp
var result = SkillLoader.LoadSkills();

if (result.Diagnostics.Count > 0)
{
    Console.WriteLine($"Found {result.Diagnostics.Count} issues:");
    foreach (var diag in result.Diagnostics)
    {
        Console.WriteLine($"  {diag.Type}: {diag.Message} ({diag.Path})");
    }
}
```

## Resources

- Agent Skills Specification: https://agentskills.io
- OpenClaw Documentation: https://docs.openclaw.ai/tools/skills
- AgentSkills Repository: https://github.com/agentskills/agentskills

## Support

For issues or questions:
1. Check diagnostics output
2. Verify SKILL.md format
3. Review validation rules
4. Check example skills in `.agents/skills/`
