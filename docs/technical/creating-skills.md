# Creating Skills
Write custom agent skills with SKILL.md files.

## Skill Structure
```markdown
---
name: skill-name
description: Brief description
disable-model-invocation: false
---

# Skill Name

Detailed instructions for the agent...
```

## Validation Rules
- Name: lowercase a-z, 0-9, hyphens
- Max 64 characters
- Must match directory name

## Example: Python Testing Skill
```markdown
---
name: python-testing
description: Python testing best practices
---

# Python Testing

When writing Python tests:
- Use pytest framework
- Follow Arrange-Act-Assert pattern
- Use descriptive test names
- Mock external dependencies
- Aim for >80% coverage
```

## Skill Locations
1. Project: `.agents/skills/`
2. User: `~/.agents/skills/`

## HEARTBEAT.md (Optional)
Add per-skill heartbeat monitoring:
```markdown
---
every: "1h"
target: "last"
---

# Heartbeat Checklist

- Check for outdated dependencies
- Review test coverage
- Verify documentation
```

See [Skills Guide](../user-guide/skills.md) for usage.
