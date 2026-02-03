# Git Workflow Heartbeat

This skill monitors git workflow and repository health.

## What to Check

1. **Branch Status**: Check for uncommitted changes or stale branches
2. **PR Status**: Look for pending pull requests that need attention
3. **Merge Conflicts**: Check for any merge conflicts
4. **Commits**: Review recent commits for proper formatting

## Response Guidelines

- Reply `HEARTBEAT_OK` if repository is in good state
- Alert if there are issues requiring attention
- Provide actionable recommendations

## Example Alert

```
⚠️ Git repository issues:
- 3 uncommitted files in working directory
- Branch 'feature/new-feature' is 15 commits behind main
- Consider syncing before making new changes
```
