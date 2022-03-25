using System;
using System.Collections.Generic;
using System.Linq;

// ReSharper disable UnusedMember.Global

namespace DotNetCommons.Temporal;

/// <summary>
/// Helper class for working with datetime values and timezones.
/// </summary>
public static class DateTimeTools
{
    public static IEnumerable<DateTime> Generate(DateTime start, DateTime end, Func<DateTime, DateTime> next)
    {
        while (start <= end)
        {
            yield return start;
            start = next(start);
        }
    }

    public static IEnumerable<DateTime> Generate(DateTime start, int count, Func<DateTime, DateTime> next)
    {
        for (int i = 0; i < count; i++)
        {
            yield return start;
            start = next(start);
        }
    }

    /// <summary>
    /// Convert an individual DateTimeOffset to UTC without changing the actual time, and
    /// then converts to local time.
    /// </summary>
    /// <param name="date"></param>
    /// <returns></returns>
    public static DateTimeOffset ForceUtcAndConvert(this DateTimeOffset date)
    {
        return new DateTimeOffset(date.Ticks, TimeSpan.Zero).ToLocalTime();
    }

    /// <summary>
    /// Convert an individual DateTime to Unspecified without changing the actual time.
    /// </summary>
    /// <param name="date"></param>
    /// <param name="kind"></param>
    /// <returns></returns>
    public static DateTime ForceKind(this DateTime date, DateTimeKind kind)
    {
        return new DateTime(date.Ticks, kind);
    }

    /// <summary>
    /// Convert an individual DateTime to Unspecified without changing the actual time.
    /// </summary>
    /// <param name="date"></param>
    /// <param name="kind"></param>
    /// <returns></returns>
    public static DateTime? ForceKind(this DateTime? date, DateTimeKind kind)
    {
        return date != null ? new DateTime(date.Value.Ticks, kind) : null;
    }

    /// <summary>
    /// Return the lesser of two datetimes.
    /// </summary>
    /// <param name="datetime1"></param>
    /// <param name="datetime2"></param>
    /// <returns></returns>
    public static DateTime MinDate(DateTime datetime1, DateTime datetime2)
    {
        return datetime1.Ticks < datetime2.Ticks ? datetime1 : datetime2;
    }

    /// <summary>
    /// Return the lesser of two datetimes.
    /// </summary>
    /// <param name="datetime1"></param>
    /// <param name="datetime2"></param>
    /// <returns></returns>
    public static DateTimeOffset MinDate(DateTimeOffset datetime1, DateTimeOffset datetime2)
    {
        return datetime1.Ticks < datetime2.Ticks ? datetime1 : datetime2;
    }

    /// <summary>
    /// Return the minimum date from a sequence of datetimes.
    /// </summary>
    /// <param name="dateTimes"></param>
    /// <returns></returns>
    public static DateTime? MinDate(IEnumerable<DateTime?> dateTimes)
    {
        var all = dateTimes.Where(x => x.HasValue).ToList();
        return all.Any() ? all.Min() : null;
    }

    /// <summary>
    /// Return the minimum date from a sequence of datetimes.
    /// </summary>
    /// <param name="dateTimes"></param>
    /// <returns></returns>
    public static DateTimeOffset? MinDate(IEnumerable<DateTimeOffset?> dateTimes)
    {
        var all = dateTimes.Where(x => x.HasValue).ToList();
        return all.Any() ? all.Min() : null;
    }

    /// <summary>
    /// Return the greater of two datetimes.
    /// </summary>
    /// <param name="datetime1"></param>
    /// <param name="datetime2"></param>
    /// <returns></returns>
    public static DateTime MaxDate(DateTime datetime1, DateTime datetime2)
    {
        return datetime1.Ticks > datetime2.Ticks ? datetime1 : datetime2;
    }

    /// <summary>
    /// Return the greater of two datetimes.
    /// </summary>
    /// <param name="datetime1"></param>
    /// <param name="datetime2"></param>
    /// <returns></returns>
    public static DateTimeOffset MaxDate(DateTimeOffset datetime1, DateTimeOffset datetime2)
    {
        return datetime1.Ticks > datetime2.Ticks ? datetime1 : datetime2;
    }

    /// <summary>
    /// Return the maximum date from a sequence of datetimes.
    /// </summary>
    /// <param name="dateTimes"></param>
    /// <returns></returns>
    public static DateTime? MaxDate(IEnumerable<DateTime?> dateTimes)
    {
        var all = dateTimes.Where(x => x.HasValue).ToList();
        return all.Any() ? all.Max() : null;
    }

    /// <summary>
    /// Return the maximum date from a sequence of datetimes.
    /// </summary>
    /// <param name="dateTimes"></param>
    /// <returns></returns>
    public static DateTimeOffset? MaxDate(IEnumerable<DateTimeOffset?> dateTimes)
    {
        var all = dateTimes.Where(x => x.HasValue).ToList();
        return all.Any() ? all.Max() : null;
    }

    /// <summary>
    /// Return a timezone from a given ID without throwing exceptions, or null if none found.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public static TimeZoneInfo? FindTimeZone(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return null;

        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(id);
        }
        catch (InvalidTimeZoneException)
        {
            return null;
        }
    }
}