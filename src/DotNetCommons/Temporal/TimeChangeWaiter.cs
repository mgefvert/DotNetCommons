namespace DotNetCommons.Temporal;

/// <summary>
/// Class that waits for a time change. Instead of relying on Windows timers, which are imprecise, use a custom logic to efficiently wait
/// for time unit changes and return as close as possible after the time change. Can be used to display time with seconds with high
/// precision. Has both sync and async methods. 
/// </summary>
public class TimeChangeWaiter
{
    public long Resolution { get; }
    public long Last { get; private set; }

    private long GetCurrent() => DateTime.UtcNow.Truncate(Resolution).Ticks;

    /// <param name="resolution">Ticks to wait. Must be at least equal to 100 milliseconds.</param>
    public TimeChangeWaiter(long resolution = TimeSpan.TicksPerSecond)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(resolution, TimeSpan.TicksPerMillisecond * 100, nameof(resolution));
        Resolution = resolution;
    }

    private int GetTimeUntilNext()
    {
        // Figure out a rough timespan to wait by checking the milliseconds to the next time mark, and then 
        // subtracting enough time to give us two or three time slice waits. One UI tick should be 16 ms. 
        var value = (int)new TimeSpan(Last + Resolution - DateTime.UtcNow.Ticks).TotalMilliseconds - 16 * 2;
        return Math.Max(value, 1);
    }

    /// <summary>
    /// Wait synchronously one time unit, returning as soon as possible after the time change. Suitable for use in background threads.
    /// </summary>
    public void WaitOne()
    {
        var current = GetCurrent();
        try
        {
            if (current != Last)
                return;

            Thread.Sleep(GetTimeUntilNext());
            for(;;)
            {
                current = GetCurrent();
                if (Last == current)
                    Thread.Sleep(1);
                else
                    return;
            }
        }
        finally
        {
            Last = current;
        }
    }

    /// <summary>
    /// Wait asynchronously one time unit, returning as soon as possible after the time change. Suitable for use in async processes.
    /// </summary>
    public async Task WaitOneAsync()
    {
        var current = GetCurrent();
        try
        {
            if (current != Last)
                return;

            await Task.Delay(GetTimeUntilNext()).ConfigureAwait(false);
            for(;;)
            {
                current = GetCurrent();
                if (Last == current)
                    await Task.Delay(1).ConfigureAwait(false);
                else
                    return;
            }
        }
        finally
        {
            Last = current;
        }
    }
}