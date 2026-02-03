using Cronos;

namespace Pi.Scheduler;

/// <summary>
/// Scheduled job definition
/// </summary>
public record ScheduledJob
{
    /// <summary>
    /// Job ID
    /// </summary>
    public required string Id { get; init; }
    
    /// <summary>
    /// Job name
    /// </summary>
    public required string Name { get; init; }
    
    /// <summary>
    /// Cron expression (e.g., "*/5 * * * *" = every 5 minutes)
    /// </summary>
    public required string CronExpression { get; init; }
    
    /// <summary>
    /// Job action to execute
    /// </summary>
    public required Func<CancellationToken, Task> Action { get; init; }
    
    /// <summary>
    /// Whether the job is enabled
    /// </summary>
    public bool Enabled { get; init; } = true;
    
    /// <summary>
    /// Maximum execution time in seconds (0 = no limit)
    /// </summary>
    public int TimeoutSeconds { get; init; } = 0;
    
    /// <summary>
    /// Retry count on failure
    /// </summary>
    public int RetryCount { get; init; } = 0;
}

/// <summary>
/// Cron-based task scheduler
/// </summary>
public class CronScheduler : IAsyncDisposable
{
    private readonly List<ScheduledJobContext> _jobs = new();
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private Task? _schedulerTask;
    private bool _isRunning;
    
    /// <summary>
    /// Add a job to the scheduler
    /// </summary>
    public void AddJob(ScheduledJob job)
    {
        var cronExpression = CronExpression.Parse(job.CronExpression);
        var context = new ScheduledJobContext
        {
            Job = job,
            CronExpression = cronExpression,
            NextRun = cronExpression.GetNextOccurrence(DateTime.UtcNow, TimeZoneInfo.Local)
        };
        
        _jobs.Add(context);
    }
    
    /// <summary>
    /// Remove a job from the scheduler
    /// </summary>
    public bool RemoveJob(string jobId)
    {
        return _jobs.RemoveAll(j => j.Job.Id == jobId) > 0;
    }
    
    /// <summary>
    /// Get all jobs
    /// </summary>
    public IReadOnlyList<ScheduledJob> GetJobs()
    {
        return _jobs.Select(j => j.Job).ToList();
    }
    
    /// <summary>
    /// Start the scheduler
    /// </summary>
    public void Start()
    {
        if (_isRunning)
            return;
        
        _isRunning = true;
        _schedulerTask = Task.Run(() => RunSchedulerAsync(_cancellationTokenSource.Token));
    }
    
    /// <summary>
    /// Stop the scheduler
    /// </summary>
    public async Task StopAsync()
    {
        if (!_isRunning)
            return;
        
        _cancellationTokenSource.Cancel();
        
        if (_schedulerTask != null)
        {
            await _schedulerTask;
        }
        
        _isRunning = false;
    }
    
    private async Task RunSchedulerAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var now = DateTime.UtcNow;
            
            foreach (var context in _jobs.Where(j => j.Job.Enabled))
            {
                if (context.NextRun.HasValue && context.NextRun.Value <= now)
                {
                    // Execute job
                    _ = ExecuteJobAsync(context, cancellationToken);
                    
                    // Schedule next run
                    context.NextRun = context.CronExpression.GetNextOccurrence(now, TimeZoneInfo.Local);
                    context.LastRun = now;
                }
            }
            
            // Sleep for 1 second before checking again
            try
            {
                await Task.Delay(1000, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }
    
    private async Task ExecuteJobAsync(ScheduledJobContext context, CancellationToken cancellationToken)
    {
        var job = context.Job;
        var retries = 0;
        
        while (retries <= job.RetryCount)
        {
            try
            {
                var timeoutCts = job.TimeoutSeconds > 0
                    ? new CancellationTokenSource(TimeSpan.FromSeconds(job.TimeoutSeconds))
                    : null;
                
                var linkedCts = timeoutCts != null
                    ? CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token)
                    : CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                
                await job.Action(linkedCts.Token);
                
                context.ExecutionCount++;
                context.LastError = null;
                break; // Success
            }
            catch (Exception ex)
            {
                context.LastError = ex.Message;
                context.ErrorCount++;
                retries++;
                
                if (retries <= job.RetryCount)
                {
                    await Task.Delay(1000 * retries, cancellationToken); // Exponential backoff
                }
            }
        }
    }
    
    public async ValueTask DisposeAsync()
    {
        await StopAsync();
        _cancellationTokenSource.Dispose();
    }
}

/// <summary>
/// Internal job context
/// </summary>
internal class ScheduledJobContext
{
    public required ScheduledJob Job { get; init; }
    public required CronExpression CronExpression { get; init; }
    public DateTime? NextRun { get; set; }
    public DateTime? LastRun { get; set; }
    public int ExecutionCount { get; set; }
    public int ErrorCount { get; set; }
    public string? LastError { get; set; }
}
