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

    /// <summary>
    /// Calculate the age of a person in years, months, and days. For instance, a given person could be
    /// 47 years, 2 months, and 18 days old.
    /// </summary>
    public static (int Years, int Months, int Days) Age(this DateTime birthDate, DateTime? today = null)
    {
        var t = today?.Date ?? DateTime.Today;
        birthDate = birthDate.Date;

        if (t < birthDate)
            return (0, 0, 0);

        var y = AgeYears(birthDate, today);
        birthDate = birthDate.AddYears(y);
        var m = AgeMonths(birthDate, today);
        birthDate = birthDate.AddMonths(m);
        var d = AgeDays(birthDate, today);

        return (y, m, d);
    }

    /// <summary>
    /// Calculate the age of a person in number of years from the birth date.
    /// </summary>
    public static int AgeYears(this DateTime birthDate, DateTime? today = null)
    {
        var t = today?.Date ?? DateTime.Today;
        birthDate = birthDate.Date;

        if (t < birthDate)
            return 0;

        var years = t.Year - birthDate.Year;
        if (t.Month < birthDate.Month || (t.Month == birthDate.Month && t.Day < birthDate.Day))
            years--;

        return years;
    }

    /// <summary>
    /// Calculate the age of a person in number of months from the birth date (e.g. 520 months returned equals 43 years and 3 months).
    /// </summary>
    public static int AgeMonths(this DateTime birthDate, DateTime? today = null)
    {
        var t = today?.Date ?? DateTime.Today;
        birthDate = birthDate.Date;

        if (t < birthDate)
            return 0;

        var birthMonths = birthDate.Year * 12 + birthDate.Month - 1;
        var todayMonths = t.Year * 12 + t.Month - 1;

        var months = todayMonths - birthMonths;
        return t.Day >= birthDate.Day ? months : months - 1;
    }

    /// <summary>
    /// Calculate the age of a person in number of days from the birth date (e.g. 3650 days returned equals about 10 years, may differ
    /// due to leap years).
    /// </summary>
    public static int AgeDays(this DateTime birthDate, DateTime? today = null)
    {
        var t = today?.Date ?? DateTime.Today;
        birthDate = birthDate.Date;

        if (t < birthDate)
            return 0;

        return (int)t.ToOADate() - (int)birthDate.ToOADate();
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
    /// Compares two datetimes with a given precision, default TimeSpan.TicksPerSecond. Other precision can be used,
    /// based in tick counts, like TimeSpan.TicksPerMinute, TimeSpan.TicksPerDay etc.
    /// </summary>
    public static bool IsCloseTo(this DateTime datetime, DateTime another, long precision = TimeSpan.TicksPerSecond)
    {
        return datetime.Truncate(precision) == another.Truncate(precision);
    }

    /// <summary>
    /// Return an Excel DateTime value for the current day, time ignored.
    /// </summary>
    public static int OADay(this DateTime datetime)
    {
        return (int)datetime.ToOADate();
    }

    /// <summary>
    /// Modify a date and set it to a different day of the month.
    /// </summary>
    public static DateTime SetDayOfMonth(this DateTime date, int dayOfMonth)
    {
        return new DateTime(date.Year, date.Month, dayOfMonth);
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