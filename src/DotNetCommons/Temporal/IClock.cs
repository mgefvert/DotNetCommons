using System;

namespace DotNetCommons.Temporal;

public interface IClock
{
    DateTime Now { get; }
    DateTime UtcNow { get; }
    DateTime Today { get; }
    DateOnly TodayDate { get; }
}