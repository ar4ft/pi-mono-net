# Pi.Scheduler - Task Scheduling

Cron-based task scheduling system.

## Features
- Cron expression parsing
- Job scheduling
- Background execution
- Timeout and retry support
- Error tracking

## Usage
```csharp
var scheduler = new CronScheduler();

scheduler.AddJob(new ScheduledJob
{
    Id = "heartbeat",
    Name = "Heartbeat Check",
    CronExpression = "*/30 * * * *", // Every 30 min
    Action = async (ct) => {
        await PerformHeartbeatAsync(ct);
    }
});

scheduler.Start();
```

## Cron Expressions
- `*/30 * * * *` - Every 30 minutes
- `0 */2 * * *` - Every 2 hours
- `0 9 * * 1-5` - Weekdays at 9 AM

## See Also
- [Heartbeat Guide](../../HEARTBEAT_SOUL_GUIDE.md)
