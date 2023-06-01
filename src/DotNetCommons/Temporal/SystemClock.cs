using System;

namespace DotNetCommons.Temporal;

// ReSharper disable once UnusedType.Global
public class SystemClock : IClock
{
    public DateTime Now => DateTime.Now;
    public DateTime UtcNow => DateTime.UtcNow;
    public DateTime Today => DateTime.Today;
    public DateOnly TodayDate => DateOnly.FromDateTime(DateTime.Today);
}