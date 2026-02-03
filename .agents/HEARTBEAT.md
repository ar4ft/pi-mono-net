# Heartbeat Checklist

This file defines what to check during periodic heartbeat runs.

## What to Check

1. **Pending Tasks**: Review any outstanding tasks from previous conversations
2. **System Health**: Check if there are any issues that need attention
3. **Important Updates**: Look for any critical information that needs to be surfaced

## Response Guidelines

- If **nothing needs attention**, reply with `HEARTBEAT_OK`
- If **something needs attention**, describe it clearly without including `HEARTBEAT_OK`
- Keep responses concise unless there's a genuine issue
- Don't infer or repeat old tasks from prior chats
- Focus on genuinely important items only

## Example Responses

**Nothing to report:**
```
HEARTBEAT_OK
```

**Something needs attention:**
```
⚠️ The build failed 3 times in the last hour. The error suggests a missing dependency.
```

---

_This file is checked during every heartbeat. Update it to customize what the agent monitors._
