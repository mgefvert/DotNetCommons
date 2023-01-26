using System;

// Written by Mats Gefvert
// Distributed under MIT License: https://opensource.org/licenses/MIT
// ReSharper disable UnusedMember.Global

namespace DotNetCommons;

public enum ISO8601Format
{
    Date,
    DateTime,
    DateTimeOffset
}

public static class CommonDateTimeExtensions
{
    private static readonly DateTime UnixEpoch = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    private static readonly DateTimeOffset UnixEpochOffset = new(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);

    public static DateTime DayOfMonthDate(this DateTime date, int dayOfMonth)
    {
        return new DateTime(date.Year, date.Month, dayOfMonth);
    }

    /// <summary>
    /// Calculate the end of month (e.g. 2019-06-30).
    /// </summary>
    public static DateTime EndOfMonth(this DateTime date)
    {
        return new DateTime(date.Year, date.Month, DateTime.DaysInMonth(date.Year, date.Month));
    }

    /// <summary>
    /// Calculate the end of week (e.g. Sunday on the given week).
    /// </summary>
    public static DateTime EndOfWeek(this DateTime datetime, DayOfWeek firstDayOfWeek = DayOfWeek.Monday)
    {
        return StartOfWeek(datetime, firstDayOfWeek).AddDays(6);
    }

    /// <summary>
    /// Calculate the end of year (e.g. 2019-12-31).
    /// </summary>
    public static DateTime EndOfYear(this DateTime datetime)
    {
        return StartOfYear(datetime).AddYears(1).AddDays(-1);
    }

    /// <summary>
    /// Return the local datetime from a UTC unix timestamp.
    /// </summary>
    public static DateTime FromUnixSeconds(long timestamp)
    {
        return UnixEpoch.AddSeconds(timestamp).ToLocalTime();
    }

    /// <summary>
    /// Return the local date from a UTC millisecond unix timestamp.
    /// </summary>
    public static DateTime FromUnixMilliseconds(long millisTimestamp)
    {
        return UnixEpoch.AddMilliseconds(millisTimestamp).ToLocalTime();
    }

    /// <summary>
    /// Return a UTC DateTimeOffset from a unix timestamp.
    /// </summary>
    public static DateTimeOffset FromUnixSecondsOffset(long timestamp)
    {
        return UnixEpochOffset.AddSeconds(timestamp);
    }

    /// <summary>
    /// Return a UTC DateTimeOffset from a unix millisecond timestam.
    /// </summary>
    public static DateTimeOffset FromUnixMillisecondsOffset(long timestamp)
    {
        return UnixEpochOffset.AddMilliseconds(timestamp);
    }

    /// <summary>
    /// Verify if a DateTime is between two optional datetimes (endpoint exclusive).
    /// </summary>
    public static bool IsBetween(this DateTime datetime, DateTime? dateStart, DateTime? dateEnd)
    {
        return datetime >= (dateStart ?? DateTime.MinValue) && datetime < (dateEnd ?? DateTime.MaxValue);
    }

    /// <summary>
    /// Return an Excel DateTime value for the current day, time ignored.
    /// </summary>
    public static int OADay(this DateTime datetime)
    {
        return (int)datetime.ToOADate();
    }

    /// <summary>
    /// Calculate the start of the month.
    /// </summary>
    public static DateTime StartOfHour(this DateTime datetime)
    {
        return new DateTime(datetime.Year, datetime.Month, datetime.Day, datetime.Hour, 0, 0);
    }

    /// <summary>
    /// Calculate the start of the month.
    /// </summary>
    public static DateTime StartOfMonth(this DateTime datetime)
    {
        return new DateTime(datetime.Year, datetime.Month, 1);
    }

    /// <summary>
    /// Calculate the start of the week.
    /// </summary>
    public static DateTime StartOfWeek(this DateTime datetime, DayOfWeek firstDayOfWeek = DayOfWeek.Monday)
    {
        datetime = datetime.Date;
        while (datetime.DayOfWeek != firstDayOfWeek)
            datetime = datetime.AddDays(-1);

        return datetime;
    }

    /// <summary>
    /// Calculate the start of the year.
    /// </summary>
    public static DateTime StartOfYear(this DateTime datetime)
    {
        return new DateTime(datetime.Year, 1, 1);
    }

    /// <summary>
    /// Return a UTC unix timestamp.
    /// </summary>
    public static long ToUnixSeconds(this DateTime datetime)
    {
        return (long)(datetime.ToUniversalTime() - UnixEpoch).TotalSeconds;
    }

    /// <summary>
    /// Return a UTC unix millisecond timestamp.
    /// </summary>
    public static long ToUnixMilliseconds(this DateTime datetime)
    {
        return (long)(datetime.ToUniversalTime() - UnixEpoch).TotalMilliseconds;
    }

    /// <summary>
    /// Return a UTC unix timestamp.
    /// </summary>
    public static long ToUnixSeconds(this DateTimeOffset datetime)
    {
        return (long)(datetime - UnixEpochOffset).TotalSeconds;
    }

    /// <summary>
    /// Return a UTC unix millisecond timestamp.
    /// </summary>
    public static long ToUnixMilliseconds(this DateTimeOffset datetime)
    {
        return (long)(datetime - UnixEpochOffset).TotalMilliseconds;
    }

    /// <summary>
    /// Return an ISO-8601 time string (e.g. 2019-06-01T12:00:00-04:00).
    /// </summary>
    /// <param name="datetime"></param>
    /// <param name="format">Format to use, default is datetime including timezone offset.</param>
    public static string ToISO8601String(this DateTime datetime, ISO8601Format format = ISO8601Format.DateTimeOffset)
    {
        return format switch
        {
            ISO8601Format.Date => datetime.ToString("yyyy-MM-dd"),
            ISO8601Format.DateTime => datetime.ToString("yyyy-MM-dd'T'HH:mm:ss"),
            ISO8601Format.DateTimeOffset => datetime.ToString("yyyy-MM-dd'T'HH:mm:ssK"),
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, null)
        };
    }

    /// <summary>
    /// Truncates a DateTime to a specified resolution. A convenient source for resolution is TimeSpan.TicksPerXXXX constants.
    /// From https://stackoverflow.com/questions/1004698/how-to-truncate-milliseconds-off-of-a-net-datetime
    /// </summary>
    /// <param name="date">The DateTime object to truncate</param>
    /// <param name="resolution">e.g. to round to nearest second, TimeSpan.TicksPerSecond</param>
    public static DateTime Truncate(this DateTime date, long resolution)
    {
        return new DateTime(date.Ticks - (date.Ticks % resolution), date.Kind);
    }
}
