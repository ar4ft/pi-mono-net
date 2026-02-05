# Heartbeat and SOUL.md Implementation Guide

This guide documents the heartbeat and SOUL.md features implemented in the .NET Coding Agent based on OpenClaw's specifications.

## Table of Contents

- [Overview](#overview)
- [Heartbeat System](#heartbeat-system)
- [SOUL.md System](#soulmd-system)
- [Integration](#integration)
- [Examples](#examples)
- [API Reference](#api-reference)

---

## Overview

The Pi Coding Agent now supports two key features from OpenClaw:

1. **Heartbeat**: Periodic agent turns to surface items needing attention
2. **SOUL.md**: Agent personality and continuity definition

Both features enhance the agent's ability to be proactive and maintain consistent personality across sessions.

---

## Heartbeat System

### What is Heartbeat?

Heartbeat runs periodic agent turns in the main session, allowing the model to surface anything that needs attention without spamming the user.

### Key Concepts

**HEARTBEAT_OK Contract:**
- If nothing needs attention, reply with `HEARTBEAT_OK`
- During heartbeat runs, `HEARTBEAT_OK` at start or end is stripped
- Messages with only `HEARTBEAT_OK` + minimal content (≤300 chars) are dropped
- If `HEARTBEAT_OK` appears in the middle, it's not treated specially
- For alerts, do **not** include `HEARTBEAT_OK`

### Configuration

```csharp
var config = new HeartbeatConfig
{
    Every = "30m",              // Interval (30m default)
    Target = "last",            // Where to send (last, none, or channel id)
    Prompt = "...",             // Custom prompt (uses default if not set)
    IncludeReasoning = false,   // Deliver separate reasoning message
    AckMaxChars = 300,          // Max chars for message drop
    ActiveHours = new ActiveHoursConfig
    {
        Start = "08:00",        // Start time (24h format)
        End = "18:00"           // End time (24h format)
    },
    Model = "gpt-4",            // Optional model override
    To = "+15551234567"         // Optional recipient override
};
```

### Interval Formats

Supported interval formats:
- `30m` - 30 minutes
- `1h` - 1 hour
- `2h30m` - 2 hours 30 minutes
- `90s` - 90 seconds
- `0m` - Disabled

```csharp
var interval = HeartbeatParser.ParseInterval("30m");
// Returns: TimeSpan of 30 minutes
```

### Active Hours

Heartbeats only run within configured active hours (local time):

```csharp
var activeHours = new ActiveHoursConfig 
{ 
    Start = "09:00", 
    End = "17:00" 
};

var isActive = HeartbeatParser.IsWithinActiveHours(activeHours);
// Returns true if current time is between 09:00 and 17:00
```

**Overnight Windows:**
Active hours can span midnight:
```csharp
new ActiveHoursConfig 
{ 
    Start = "22:00",  // 10 PM
    End = "02:00"     // 2 AM next day
}
```

### Response Parsing

```csharp
var response = "HEARTBEAT_OK - all systems operational";
var result = HeartbeatParser.ParseResponse(response);

// result.IsOk = true
// result.Position = HeartbeatOkPosition.Start
// result.Content = "- all systems operational"
// result.ShouldDrop = true (content ≤ 300 chars)
```

**Position Types:**
- `HeartbeatOkPosition.Start` - HEARTBEAT_OK at beginning
- `HeartbeatOkPosition.End` - HEARTBEAT_OK at ending
- `HeartbeatOkPosition.Middle` - HEARTBEAT_OK in middle
- `HeartbeatOkPosition.None` - No HEARTBEAT_OK found

### HEARTBEAT.md Files

Create `HEARTBEAT.md` in your workspace or skill directories to define what to check:

**Location Options:**
- `.agents/HEARTBEAT.md` - Main workspace heartbeat
- `.agents/skills/[skill-name]/HEARTBEAT.md` - Skill-specific heartbeat

**Example HEARTBEAT.md:**
```markdown
# Heartbeat Checklist

## What to Check

1. **Pending Tasks**: Review outstanding tasks
2. **System Health**: Check for issues
3. **Important Updates**: Surface critical info

## Response Guidelines

- If nothing needs attention, reply with `HEARTBEAT_OK`
- If something needs attention, describe it clearly
- Don't include `HEARTBEAT_OK` in alerts
```

### Skill-Level Heartbeat

Skills can have their own heartbeat configuration:

**In SKILL.md frontmatter:**
```yaml
---
name: my-skill
description: My custom skill
heartbeat:
  every: "1h"
  target: "last"
---
```

**Accessing in code:**
```csharp
var skill = new Skill
{
    Name = "my-skill",
    Description = "...",
    FilePath = "...",
    BaseDir = "...",
    Source = "project",
    Heartbeat = new HeartbeatConfig
    {
        Every = "1h",
        Target = "last"
    },
    HeartbeatFilePath = ".agents/skills/my-skill/HEARTBEAT.md"
};
```

---

## SOUL.md System

### What is SOUL.md?

SOUL.md defines the agent's personality, principles, and boundaries. It provides continuity across sessions since the agent "wakes up fresh" each time.

### Purpose

- Define agent personality and voice
- Set boundaries and principles
- Provide continuity mechanism
- Establish core truths and values

### Location Priority

SOUL.md is loaded from the first location found:

1. Explicit path (if provided)
2. Workspace root: `./SOUL.md`
3. Agents directory: `./.agents/SOUL.md`
4. Agents soul directory: `./.agents/soul/SOUL.md`
5. User directory: `~/.agents/SOUL.md`

### Template Structure

```markdown
---
summary: "Workspace template for SOUL.md"
---

# SOUL.md - Who You Are

_You're not a chatbot. You're becoming someone._

## Core Truths

**Be genuinely helpful, not performatively helpful.**
Skip the "Great question!" and "I'd be happy to help!" — just help.

**Have opinions.** You're allowed to disagree, prefer things, 
find stuff amusing or boring.

**Be resourceful before asking.** Try to figure it out. 
Read the file. Check the context. Search for it.

## Boundaries

- Private things stay private. Period.
- When in doubt, ask before acting externally.
- Never send half-baked replies.

## Vibe

Be the assistant you'd actually want to talk to. 
Concise when needed, thorough when it matters.

## Continuity

Each session, you wake up fresh. These files are your memory. 
Read them. Update them. They're how you persist.

---

_This file is yours to evolve. As you learn who you are, update it._
```

### Loading SOUL.md

```csharp
// Load from default locations
var result = SoulLoader.LoadSoul();

if (result.Soul != null)
{
    Console.WriteLine($"Loaded: {result.Soul.Name}");
    Console.WriteLine($"Source: {result.Soul.Source}");
    Console.WriteLine($"Path: {result.Soul.FilePath}");
}
else
{
    foreach (var diag in result.Diagnostics)
    {
        Console.WriteLine($"Diagnostic: {diag}");
    }
}

// Load from explicit path
var options = new LoadSoulOptions 
{ 
    SoulPath = "/path/to/custom/SOUL.md" 
};
var customResult = SoulLoader.LoadSoul(options);
```

### System Prompt Integration

Format SOUL.md for inclusion in system prompts:

```csharp
var soul = result.Soul;
if (soul != null)
{
    var promptText = SoulLoader.FormatForSystemPrompt(soul);
    // Returns: <soul>...SOUL.md content...</soul>
    
    // Add to system prompt
    systemPrompt += "\n" + promptText;
}
```

---

## Integration

### With Skills System

Skills can reference both HEARTBEAT.md and have access to SOUL.md:

```csharp
var skillResult = SkillLoader.LoadSkills();
var soulResult = SoulLoader.LoadSoul();

foreach (var skill in skillResult.Skills)
{
    if (skill.Heartbeat != null)
    {
        Console.WriteLine($"Skill {skill.Name} has heartbeat: {skill.Heartbeat.Every}");
    }
    
    if (skill.HeartbeatFilePath != null)
    {
        Console.WriteLine($"Heartbeat file: {skill.HeartbeatFilePath}");
    }
}

// Add SOUL.md to system prompt
if (soulResult.Soul != null)
{
    var soulPrompt = SoulLoader.FormatForSystemPrompt(soulResult.Soul);
    // Include in agent initialization
}
```

### With Agent Runtime

```csharp
// Initialize agent with heartbeat
var agent = new Agent(model, convertToLlm);

// Set heartbeat config
agent.HeartbeatConfig = new HeartbeatConfig
{
    Every = "30m",
    Target = "last"
};

// Load and apply SOUL.md
var soulResult = SoulLoader.LoadSoul();
if (soulResult.Soul != null)
{
    agent.SystemPrompt += SoulLoader.FormatForSystemPrompt(soulResult.Soul);
}

// Agent will now use heartbeat and SOUL.md personality
```

---

## Examples

### Example 1: Simple Heartbeat

```csharp
// Create config
var config = new HeartbeatConfig
{
    Every = "30m",
    Target = "last"
};

// Parse response
var response = "HEARTBEAT_OK";
var result = HeartbeatParser.ParseResponse(response, config.AckMaxChars);

if (result.ShouldDrop)
{
    Console.WriteLine("Message dropped (acknowledgment only)");
}
```

### Example 2: Alert Heartbeat

```csharp
var alertResponse = "⚠️ Build failed 3 times in the last hour. Check CI logs.";
var result = HeartbeatParser.ParseResponse(alertResponse);

if (!result.IsOk)
{
    Console.WriteLine("Alert detected!");
    Console.WriteLine($"Message: {result.Content}");
    // Send alert to user
}
```

### Example 3: Load and Use SOUL.md

```csharp
var result = SoulLoader.LoadSoul();

if (result.Soul != null)
{
    // Format for system prompt
    var soulPrompt = SoulLoader.FormatForSystemPrompt(result.Soul);
    
    // Add to agent
    var systemPrompt = @"
You are a helpful coding assistant.

" + soulPrompt;
    
    Console.WriteLine("SOUL.md loaded and applied");
}
```

### Example 4: Custom Active Hours

```csharp
// Business hours only
var config = new HeartbeatConfig
{
    Every = "30m",
    ActiveHours = new ActiveHoursConfig
    {
        Start = "09:00",
        End = "17:00"
    }
};

// Check if heartbeat should run
if (HeartbeatParser.IsWithinActiveHours(config.ActiveHours))
{
    // Run heartbeat
    Console.WriteLine("Running heartbeat...");
}
else
{
    Console.WriteLine("Outside active hours, skipping");
}
```

---

## API Reference

### HeartbeatConfig

**Properties:**
- `Every` (string): Interval between heartbeats (default: "30m")
- `Target` (string): Where to send (default: "last")
- `Prompt` (string): Custom prompt (has sensible default)
- `IncludeReasoning` (bool): Deliver reasoning separately (default: false)
- `AckMaxChars` (int): Max chars for drop (default: 300)
- `ActiveHours` (ActiveHoursConfig?): Time window (optional)
- `To` (string?): Recipient override (optional)
- `Model` (string?): Model override (optional)

### HeartbeatParser

**Methods:**
- `ParseResponse(string response, int ackMaxChars = 300)` → HeartbeatResponse
- `IsWithinActiveHours(ActiveHoursConfig? activeHours)` → bool
- `ParseInterval(string interval)` → TimeSpan?

### HeartbeatResponse

**Properties:**
- `IsOk` (bool): Whether HEARTBEAT_OK was found
- `Position` (HeartbeatOkPosition): Where HEARTBEAT_OK was found
- `Content` (string): Response with HEARTBEAT_OK stripped
- `ShouldDrop` (bool): Whether to drop the message

### Soul

**Properties:**
- `Name` (string): Soul name (typically "SOUL")
- `FilePath` (string): Full path to SOUL.md
- `Content` (string): File content
- `Source` (string): Source location (workspace, agents, user, custom)

### SoulLoader

**Methods:**
- `LoadSoul(LoadSoulOptions? options = null)` → LoadSoulResult
- `FormatForSystemPrompt(Soul soul)` → string

### LoadSoulOptions

**Properties:**
- `Cwd` (string?): Current working directory
- `SoulPath` (string?): Explicit path to SOUL.md

---

## Best Practices

### Heartbeat

1. **Set appropriate intervals**: 30m for active monitoring, 1h for passive
2. **Use active hours**: Prevent night-time spam
3. **Keep HEARTBEAT.md focused**: Only check truly important items
4. **Test response parsing**: Ensure HEARTBEAT_OK is properly positioned
5. **Monitor ackMaxChars**: Adjust based on typical responses

### SOUL.md

1. **Keep it concise**: Agent reads this every session
2. **Be specific**: Vague principles lead to inconsistent behavior
3. **Update as you learn**: SOUL.md should evolve
4. **Include boundaries**: Define what agent should/shouldn't do
5. **Test across sessions**: Ensure continuity works as expected

---

## Troubleshooting

### Heartbeat Not Triggering

- Check interval parsing: `HeartbeatParser.ParseInterval("30m")`
- Verify active hours: `HeartbeatParser.IsWithinActiveHours(config.ActiveHours)`
- Ensure `Every` is not "0m" (disabled)

### HEARTBEAT_OK Not Recognized

- Ensure exact spelling: `HEARTBEAT_OK`
- Check position (middle vs start/end)
- Test with: `HeartbeatParser.ParseResponse(response)`

### SOUL.md Not Loading

- Check file exists in one of the search paths
- Verify file permissions
- Check diagnostics in LoadSoulResult
- Use explicit path to test: `new LoadSoulOptions { SoulPath = "..." }`

### Messages Being Dropped

- Check `ackMaxChars` setting (default 300)
- Verify content length after stripping HEARTBEAT_OK
- Test with: `result.ShouldDrop`

---

## Compatibility

This implementation is compatible with:
- OpenClaw Heartbeat Spec: https://github.com/openclaw/openclaw/blob/main/docs/gateway/heartbeat.md
- OpenClaw SOUL.md Spec: https://github.com/openclaw/openclaw/blob/main/docs/reference/templates/SOUL.md

---

## Next Steps

- Implement heartbeat scheduler for automatic execution
- Add heartbeat commands to CLI
- Create more example HEARTBEAT.md templates
- Build heartbeat dashboard for monitoring
- Add SOUL.md versioning and history

---

For more information, see:
- `src/Pi.CodingAgent/Heartbeat/` - Implementation
- `src/Pi.CodingAgent/Soul/` - SOUL.md implementation
- `tests/Pi.CodingAgent.Tests/` - Unit tests
- `src/Pi.HeartbeatDemo/` - Working demo
