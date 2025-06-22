// ReSharper disable UnusedMember.Global

namespace DotNetCommons.Temporal;

/// <summary>
/// Helper class for working with datetime values and timezones.
/// </summary>
public static class DateTimeTools
{
    /// <summary>
    /// Generates a sequence of DateTime values starting at a specified value,
    /// and creating subsequent values using the provided function until a specified end value is reached or exceeded.
    /// </summary>
    /// <param name="start">The starting DateTime of the sequence.</param>
    /// <param name="end">The upper limit for the sequence. The sequence stops when the generated DateTime exceeds this value.</param>
    /// <param name="next">A function that defines how to calculate the next DateTime in the sequence from the current one.</param>
    /// <returns>
    /// An enumerable sequence of DateTime values starting at <paramref name="start"/>
    /// and continuing up to <paramref name="end"/>, calculated using <paramref name="next"/>.
    /// </returns>
    public static IEnumerable<DateTime> Generate(DateTime start, DateTime end, Func<DateTime, DateTime> next)
    {
        while (start <= end)
        {
            yield return start;
            start = next(start);
        }
    }

    /// <summary>
    /// Generates a sequence of DateTime values starting at a specified value,
    /// and creating a specified number of subsequent values using the provided function.
    /// </summary>
    /// <param name="start">The starting DateTime of the sequence.</param>
    /// <param name="count">The number of DateTime values to generate in the sequence.</param>
    /// <param name="next">A function that defines how to calculate the next DateTime in the sequence from the current one.</param>
    /// <returns>
    /// An enumerable sequence of DateTime values starting at <paramref name="start"/>
    /// and containing <paramref name="count"/> values, calculated using <paramref name="next"/>.
    /// </returns>
    public static IEnumerable<DateTime> Generate(DateTime start, int count, Func<DateTime, DateTime> next)
    {
        for (var i = 0; i < count; i++)
        {
            yield return start;
            start = next(start);
        }
    }

    /// <summary>
    /// Convert an individual DateTime to Unspecified without changing the actual time.
    /// </summary>
    public static DateTime ForceKind(this DateTime date, DateTimeKind kind)
    {
        return new DateTime(date.Ticks, kind);
    }

    /// <summary>
    /// Convert an individual DateTime to Unspecified without changing the actual time.
    /// </summary>
    public static DateTime? ForceKind(this DateTime? date, DateTimeKind kind)
    {
        return date != null ? new DateTime(date.Value.Ticks, kind) : null;
    }

    /// <summary>
    /// Return the lesser of two datetimes.
    /// </summary>
    public static DateTime MinDate(DateTime datetime1, DateTime datetime2)
    {
        return datetime1.Ticks < datetime2.Ticks ? datetime1 : datetime2;
    }

    /// <summary>
    /// Return the lesser of two datetimes.
    /// </summary>
    public static DateTimeOffset MinDate(DateTimeOffset datetime1, DateTimeOffset datetime2)
    {
        return datetime1.Ticks < datetime2.Ticks ? datetime1 : datetime2;
    }

    /// <summary>
    /// Return the minimum date from a sequence of datetimes.
    /// </summary>
    public static DateTime? MinDate(IEnumerable<DateTime?> dateTimes)
    {
        var all = dateTimes.Where(x => x.HasValue).ToList();
        return all.Any() ? all.Min() : null;
    }

    /// <summary>
    /// Return the minimum date from a sequence of datetimes.
    /// </summary>
    public static DateTimeOffset? MinDate(IEnumerable<DateTimeOffset?> dateTimes)
    {
        var all = dateTimes.Where(x => x.HasValue).ToList();
        return all.Any() ? all.Min() : null;
    }

    /// <summary>
    /// Return the greater of two datetimes.
    /// </summary>
    public static DateTime MaxDate(DateTime datetime1, DateTime datetime2)
    {
        return datetime1.Ticks > datetime2.Ticks ? datetime1 : datetime2;
    }

    /// <summary>
    /// Return the greater of two datetimes.
    /// </summary>
    public static DateTimeOffset MaxDate(DateTimeOffset datetime1, DateTimeOffset datetime2)
    {
        return datetime1.Ticks > datetime2.Ticks ? datetime1 : datetime2;
    }

    /// <summary>
    /// Return the maximum date from a sequence of datetimes.
    /// </summary>
    public static DateTime? MaxDate(IEnumerable<DateTime?> dateTimes)
    {
        var all = dateTimes.Where(x => x.HasValue).ToList();
        return all.Any() ? all.Max() : null;
    }

    /// <summary>
    /// Return the maximum date from a sequence of datetimes.
    /// </summary>
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