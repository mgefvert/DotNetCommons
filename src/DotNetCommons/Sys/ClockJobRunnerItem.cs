using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DotNetCommons.Sys;

/// Represents the context for a job execution, providing necessary dependencies
/// and configurations such as service provider, logging interface, and cancellation token.
public record JobContext(string Name, IServiceProvider ServiceProvider, ILogger Logger, CancellationToken CancellationToken);

/// Represents an individual job definition within the ClockJobRunner, encapsulating
/// the job's execution details, its schedule, and its current state.
internal class ClockJobRunnerItem
{
    private readonly Func<JobContext, Task> _jobAction;

    private CancellationTokenSource? _cancellationTokenSource;

    public Task? RunningTask { get; private set; }
    public string Name { get; }
    public ClockSchedule Schedule { get; }
    public DateTime LastRun { get; private set; }

    public ClockJobRunnerItem(string name, Func<JobContext, Task> jobAction, ClockSchedule schedule, bool runImmediately)
    {
        _jobAction = jobAction;
        Name       = name;
        Schedule   = schedule;
        LastRun    = runImmediately ? DateTime.MinValue : DateTime.Now;
    }

    /// Determines whether the job is currently running or not; not running means either never started or run to completion or failure.
    public bool IsRunning()
    {
        lock (this)
            return RunningTask is { IsCompleted: false };
    }

    /// Determines whether the job should initiate execution based on its schedule and current state.
    public bool ShouldStart()
    {
        var resolution = Schedule switch
        {
            ClockSchedule.Daily        => TimeSpan.TicksPerDay,
            ClockSchedule.EveryHour    => TimeSpan.TicksPerHour,
            ClockSchedule.EveryMinute  => TimeSpan.TicksPerMinute,
            ClockSchedule.Continuously => 1,
            _                          => TimeSpan.TicksPerDay
        };

        return !IsRunning() && DateTime.Now.Truncate(resolution) != LastRun.Truncate(resolution);
    }

    /// <summary>
    /// Attempts to start the execution of the job if it is not already running. Initializes cancellation tokens, begins execution
    /// of the job on a separate task, and returns true if the job was successfully started.
    /// </summary>
    /// <param name="logger">An instance of <see cref="ILogger"/> used for logging events during job execution.</param>
    /// <param name="services">An instance of <see cref="IServiceProvider"/> that provides the required dependencies for the job.</param>
    /// <returns>Returns true if the job was successfully started; otherwise, returns false if the job is already running.</returns>
    public bool Start(ILogger<ClockJobRunner> logger, IServiceProvider services)
    {
        if (IsRunning())
            return false;

        lock (this)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            RunningTask = Task.Run(() => TaskRunner(logger, services));
            return true;
        }
    }

    /// Stops the currently executing job if it is running, by canceling its associated cancellation token
    /// and waiting for the job to complete its execution.
    public Task Stop()
    {
        if (!IsRunning())
            return Task.CompletedTask;

        _cancellationTokenSource!.Cancel();
        return RunningTask!.WaitAsync(Timeout.InfiniteTimeSpan);
    }

    /// Executes the specified job logic in an asynchronous task and handles logging, lifecycle, and exception management for
    /// the job execution. Updates the job's last run time and captures its runtime duration.
    private async Task TaskRunner(ILogger<ClockJobRunner> logger, IServiceProvider services)
    {
        LastRun = DateTime.Now;
        var t0  = DateTime.UtcNow;
        logger.LogInformation("Starting job {name}", Name);
        try
        {
            await using var scope = services.CreateAsyncScope();

            var context = new JobContext(Name, scope.ServiceProvider, logger, _cancellationTokenSource!.Token);
            await _jobAction(context);
            logger.LogInformation("Job {name} finished in {time}", Name, DateTime.UtcNow - t0);
        }
        catch (TaskCanceledException)
        {
            logger.LogInformation("Job {name} canceled after {time}", Name, DateTime.UtcNow - t0);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Exception occured in job {name}", Name);
        }
    }
}
