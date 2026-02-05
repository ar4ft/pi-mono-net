---
name: git-workflow
description: Git workflow and best practices for version control operations.
---

# Git Workflow Skill

This skill provides guidance on using Git effectively in development workflows.

## Commit Messages

Follow the conventional commits format:

```
<type>(<scope>): <description>
```

Types: feat, fix, docs, refactor, test, chore

## Common Operations

### Creating a Feature

```bash
git checkout -b feature/new-feature
git add .
git commit -m "feat: add new feature"
git push origin feature/new-feature
```

## Best Practices

1. Commit often with meaningful messages
2. Keep commits focused
3. Pull/rebase regularly
4. Review changes before committing
5. Don't commit sensitive data
