using Microsoft.Extensions.Logging;

namespace DotNetCommons.Sys;

public enum ClockSchedule
{
    Daily,
    EveryHour,
    EveryMinute,
    Continuously
}

/// <summary>
/// Class responsible for scheduling and executing periodic jobs based on a clock schedule. Works well as a Singleton in an application.
/// </summary>
public class ClockJobRunner : IDisposable
{
    private readonly ILogger<ClockJobRunner> _logger;
    private readonly IServiceProvider _services;
    private readonly Timer _timer;
    private readonly List<ClockJobRunnerItem> _jobs = [];

    private volatile bool _inTimer;
    private readonly object _lock = new();

    /// Whether the job scheduler is enabled or not. If disabled, jobs will have to be checked manually by calling <see cref="Run"/>.
    public bool SchedulerEnabled { get; set; } = true;

    public ClockJobRunner(ILogger<ClockJobRunner> logger, IServiceProvider serviceProvider)
    {
        _logger   = logger;
        _services = serviceProvider;
        _timer    = new Timer(OnTimer, null, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5));
    }

    /// Releases all resources used by the ClockJobRunner instance and stops any running jobs. If jobs are still in progress,
    /// this method waits for their completion before disposing.
    public void Dispose()
    {
        StopAll().ConfigureAwait(false).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Adds a new job to the ClockJobRunner instance with the specified parameters.
    /// </summary>
    /// <param name="name">The unique name of the job. Not required to be unique.</param>
    /// <param name="job">The function representing the job logic to be executed. This function accepts a <see cref="JobContext"/>
    /// parameter that contains a logger, service provider, cancellation token, etc; and runs asynchronously.</param>
    /// <param name="schedule">The schedule specifying when the job should run. </param>
    /// <param name="runImmediately">A boolean indicating whether the job should run immediately after being added. If set to true,
    /// an hourly job will run once immediately, and then at the next hour again. If false, it will wait for the next hour to start
    /// the job.</param>
    public void AddJob(string name, Func<JobContext, Task> job, ClockSchedule schedule, bool runImmediately)
    {
        _jobs.Add(new ClockJobRunnerItem(name, job, schedule, runImmediately));
    }

    private List<ClockJobRunnerItem> InternalStartJobs()
    {
        lock (_lock)
        {
            var starting = _jobs.Where(x => x.ShouldStart()).ToList();
            foreach (var job in starting)
                job.Start(_logger, _services);

            return starting;
        }
    }

    private void OnTimer(object? state)
    {
        if (_inTimer || !SchedulerEnabled)
            return;

        _inTimer = true;
        try
        {
            InternalStartJobs();
        }
        finally
        {
            _inTimer = false;
        }
    }

    /// Remove a job from the scheduler.
    public void RemoveJob(string name)
    {
        _jobs.RemoveAll(x => x.Name == name);
    }

    /// <summary>
    /// Force a manual check to see if any jobs should start. Does not guarantee that any jobs will actually start. Waits for
    /// the completion of those tasks up until the given timeout period.
    /// </summary>
    /// <returns>A list of any jobs started by this call.</returns>
    /// <remarks>
    /// This is useful if you want to make sure that the initialization tasks for an application have completed before moving on.
    /// </remarks>
    /// <param name="timeout">Timeout to wait for jobs to complete, or <see cref="Timeout.Infinite"/> to wait forever.</param>
    /// <param name="waitForCompletion">Wait for the started jobs to complete. Note that this will not consider any job with the
    /// ClockSchedule.Continuously setting as those are probably not going to ever exit or their own.</param>
    public async Task Run(TimeSpan timeout, bool waitForCompletion)
    {
        var started = InternalStartJobs();
        if (!waitForCompletion)
            return;

        var tasks = started
            .Where(x => x.Schedule != ClockSchedule.Continuously)
            .Select(x => x.RunningTask)
            .NotNulls()
            .Select(x => x.WaitAsync(timeout))
            .ToArray();

        await Task.WhenAll(tasks);
    }

    /// Stops all running instances of the specified job by its name. This method identifies and stops all jobs matching the given name
    /// by canceling their execution and ensuring they are properly terminated.
    public Task Stop(string name)
    {
        var jobs = _jobs.Where(x => x.Name == name).ToList();
        return Task.WhenAll(jobs.Select(x => x.Stop()));
    }

    /// <summary>
    /// Stops all jobs managed by the ClockJobRunner and halts the timer used for scheduling. Ensures that any running jobs are
    /// gracefully stopped by waiting for their completion before returning control.
    /// </summary>
    /// <remarks>
    /// Once StopAll() has been called, this will permanently disable the internal scheduler and no jobs will be started again.
    /// </remarks>
    public async Task StopAll()
    {
        _timer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
        while (_inTimer)
            await Task.Delay(1);

        await Task.WhenAll(_jobs.Select(x => x.Stop()));
    }
}
