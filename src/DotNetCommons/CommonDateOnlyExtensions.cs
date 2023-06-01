using System;

// Written by Mats Gefvert
// Distributed under MIT License: https://opensource.org/licenses/MIT
// ReSharper disable UnusedMember.Global

namespace DotNetCommons;

public static class CommonDateOnlyExtensions
{
    /// <summary>
    /// Calculate the end of month (e.g. 2019-06-30).
    /// </summary>
    public static DateOnly EndOfMonth(this DateOnly date)
    {
        return new DateOnly(date.Year, date.Month, DateTime.DaysInMonth(date.Year, date.Month));
    }

    /// <summary>
    /// Calculate the end of week (e.g. Sunday on the given week).
    /// </summary>
    public static DateOnly EndOfWeek(this DateOnly date, DayOfWeek firstDayOfWeek = DayOfWeek.Monday)
    {
        return StartOfWeek(date, firstDayOfWeek).AddDays(6);
    }

    /// <summary>
    /// Calculate the end of year (e.g. 2019-12-31).
    /// </summary>
    public static DateOnly EndOfYear(this DateOnly date)
    {
        return StartOfYear(date).AddYears(1).AddDays(-1);
    }

    /// <summary>
    /// Verify if a DateOnly is between two optional dates (endpoint exclusive).
    /// </summary>
    public static bool IsBetween(this DateOnly date, DateOnly? dateStart, DateOnly? dateEnd)
    {
        return date >= (dateStart ?? DateOnly.MinValue) && date < (dateEnd ?? DateOnly.MaxValue);
    }

    /// <summary>
    /// Return an Excel DateOnly value for the current day, time ignored.
    /// </summary>
    public static int OADay(this DateOnly date)
    {
        return (int)date.ToDateTime(TimeOnly.MinValue).ToOADate();
    }

    /// <summary>
    /// Modify a date and set it to a different day of the month.
    /// </summary>
    public static DateOnly SetDayOfMonth(this DateOnly date, int dayOfMonth)
    {
        return new DateOnly(date.Year, date.Month, dayOfMonth);
    }

    /// <summary>
    /// Calculate the start of the month.
    /// </summary>
    public static DateOnly StartOfMonth(this DateOnly date)
    {
        return new DateOnly(date.Year, date.Month, 1);
    }

    /// <summary>
    /// Calculate the start of the week.
    /// </summary>
    public static DateOnly StartOfWeek(this DateOnly date, DayOfWeek firstDayOfWeek = DayOfWeek.Monday)
    {
        while (date.DayOfWeek != firstDayOfWeek)
            date = date.AddDays(-1);

        return date;
    }

    /// <summary>
    /// Calculate the start of the year.
    /// </summary>
    public static DateOnly StartOfYear(this DateOnly date)
    {
        return new DateOnly(date.Year, 1, 1);
    }

    /// <summary>
    /// Return an ISO-8601 time string (e.g. 2019-06-01).
    /// </summary>
    public static string ToISO8601String(this DateOnly date)
    {
        return date.ToString("yyyy-MM-dd");
    }
}