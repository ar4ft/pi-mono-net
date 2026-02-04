# Advanced Skills

Complex skill patterns and techniques.

## Per-Skill Heartbeats

Add HEARTBEAT.md to your skill:

```markdown
---
every: "1h"
target: "last"
---

# Skill Health Check

- Are dependencies up to date?
- Is test coverage sufficient?
- Are there any TODOs?
```

## Dynamic Skills

Load skills programmatically:

```csharp
var loader = new SkillLoader();
var result = loader.LoadSkillFromFile("path/to/SKILL.md");

if (result.Success)
{
    var skill = result.Skill;
    // Use skill
}
```

## Skill Templates

Create reusable skill templates:

```markdown
---
name: ${SKILL_NAME}
description: ${SKILL_DESCRIPTION}
---

# ${SKILL_NAME}

${SKILL_CONTENT}
```

## Best Practices

1. **Keep skills focused** - One skill, one purpose
2. **Use clear instructions** - Write for clarity
3. **Include examples** - Show don't tell
4. **Test with agent** - Verify behavior
5. **Version control** - Track changes

See [Creating Skills](../technical/creating-skills.md) for technical details.
