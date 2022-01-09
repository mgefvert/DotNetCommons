using System;
using System.Collections.Generic;
using System.Linq;

// Written by Mats Gefvert
// Distributed under MIT License: https://opensource.org/licenses/MIT
// ReSharper disable UnusedMember.Global

namespace DotNetCommons.Temporal;

public enum DateRangeBound
{
    CompletelyInside,
    PartiallyInside,
    StartInside,
    EndInside
}

public enum DateRangeType
{
    Undefined,
    Daily,
    Weekly,
    Biweekly,
    WeeklyIso,
    BiweeklyIso,
    Monthly,
    Bimonthly,
    Quarterly,
    Tertile,
    Semiannually,
    Annually,
    Biannually,
    Decade,
    Century,
    Millenium
}

/// <summary>
/// Class that handles date ranges. Not suited for time calculations, assumes that the time part
/// is always 0.
/// </summary>
public class DateRange
{
    protected static readonly TimeSpan SingleDay = TimeSpan.FromDays(1);
    protected static readonly TimeSpan NegativeSingleDay = TimeSpan.FromDays(-1);

    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public TimeSpan Span => (End - Start).Add(SingleDay);
    public DateRangeType Type { get; set; }

    /// <summary>
    /// Create a new, empty DateRange instance.
    /// </summary>
    public DateRange()
    {
    }

    /// <summary>
    /// Create a DateRange with specific start and end points.
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    public DateRange(DateTime start, DateTime end)
    {
        Start = start.Date;
        End = end.Date;
    }

    /// <summary>
    /// Create a DateRange from a collection of objects, searching for min and max dates.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="collection"></param>
    /// <param name="selector"></param>
    /// <returns></returns>
    public static DateRange FromMinMax<T>(IEnumerable<T> collection, Func<T, DateTime> selector)
    {
        var first = true;
        long min = 0, max = 0;

        foreach (var date in collection.Select(selector).Select(x => x.Ticks))
        {
            if (first)
            {
                min = max = date;
                first = false;
            }
            else
            {
                if (date < min)
                    min = date;
                if (date > max)
                    max = date;
            }
        }

        if (first)
            throw new InvalidOperationException("No data in array - at least one item is required.");

        return new DateRange(new DateTime(min), new DateTime(max));
    }

    protected void AssertDateRangeTypeIsSet()
    {
        if (Type == DateRangeType.Undefined)
            throw new InvalidOperationException("Cannot perform date range arithmetic when type is undefined.");
    }

    /// <summary>
    /// Return a new, cloned DateRange with start and end points extended if needed.
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    public DateRange BoundRange(DateTime? start, DateTime? end)
    {
        var result = (DateRange)MemberwiseClone();

        if (start != null)
        {
            if (result.Start < start.Value)
                result.Start = start.Value;
            if (result.End < start.Value)
                result.End = start.Value;
        }

        if (end != null)
        {
            if (result.Start > end.Value)
                result.Start = end.Value;
            if (result.End > end.Value)
                result.End = end.Value;
        }

        return result;
    }

    protected static DateTime GetStartDate(DateRangeType type, DateTime date)
    {
        int y = date.Year, m = date.Month-1;

        switch (type)
        {
            case DateRangeType.Daily:
                return date;

            case DateRangeType.Weekly:
            case DateRangeType.Biweekly:
                return date.StartOfWeek(DayOfWeek.Sunday);

            case DateRangeType.WeeklyIso:
            case DateRangeType.BiweeklyIso:
                // ReSharper disable once RedundantArgumentDefaultValue
                return date.StartOfWeek(DayOfWeek.Monday);

            case DateRangeType.Monthly:
                return new DateTime(y, m + 1, 1);

            case DateRangeType.Bimonthly:
                return new DateTime(y, m/2*2 + 1, 1);

            case DateRangeType.Quarterly:
                return new DateTime(y, m/3*3 + 1, 1);

            case DateRangeType.Tertile:
                return new DateTime(y, m/4*4 + 1, 1);

            case DateRangeType.Semiannually:
                return new DateTime(y, m/6*6 + 1, 1);

            case DateRangeType.Annually:
                return new DateTime(y, 1, 1);

            case DateRangeType.Biannually:
                return new DateTime(y/2*2, 1, 1);

            case DateRangeType.Decade:
                return new DateTime(y/10*10, 1, 1);

            case DateRangeType.Century:
                return new DateTime(y/100*100, 1, 1);

            case DateRangeType.Millenium:
                return new DateTime(y/1000*1000, 1, 1);

            default:
                throw new ArgumentOutOfRangeException(nameof(type), "Date range type is out of range.");
        }
    }

    protected static DateTime GetNextStartDate(DateRangeType type, DateTime date, int count)
    {
        switch (type)
        {
            case DateRangeType.Daily:
                return date.AddDays(count);

            case DateRangeType.Weekly:
            case DateRangeType.WeeklyIso:
                return date.AddDays(7 * count);

            case DateRangeType.Biweekly:
            case DateRangeType.BiweeklyIso:
                return date.AddDays(14 * count);

            case DateRangeType.Monthly:
                return date.AddMonths(1 * count);

            case DateRangeType.Bimonthly:
                return date.AddMonths(2 * count);

            case DateRangeType.Quarterly:
                return date.AddMonths(3 * count);

            case DateRangeType.Tertile:
                return date.AddMonths(4 * count);

            case DateRangeType.Semiannually:
                return date.AddMonths(6 * count);

            case DateRangeType.Annually:
                return date.AddYears(1 * count);

            case DateRangeType.Biannually:
                return date.AddYears(2 * count);

            case DateRangeType.Decade:
                return date.AddYears(10 * count);

            case DateRangeType.Century:
                return date.AddYears(100 * count);

            case DateRangeType.Millenium:
                return date.AddYears(1000 * count);

            default:
                throw new ArgumentOutOfRangeException(nameof(type), "Date range type is out of range.");
        }
    }

    /// <summary>
    /// Determine if a particular date is within the date range (inclusive endpoint).
    /// </summary>
    /// <param name="date"></param>
    /// <returns></returns>
    public bool InRange(DateTime date)
    {
        date = date.Date;
        return Start <= date && End >= date;
    }

    /// <summary>
    /// Determine if a DateRange is within the date range, overlapping, or outside.
    /// </summary>
    /// <param name="range"></param>
    /// <param name="boundingMode"></param>
    /// <returns></returns>
    public bool InRange(DateRange range, DateRangeBound boundingMode)
    {
        switch (boundingMode)
        {
            case DateRangeBound.CompletelyInside:
                return range.InRange(Start) && range.InRange(End);

            case DateRangeBound.PartiallyInside:
                return range.InRange(Start) || range.InRange(End);

            case DateRangeBound.StartInside:
                return range.InRange(Start);

            case DateRangeBound.EndInside:
                return range.InRange(End);

            default:
                return false;
        }
    }

    /// <summary>
    /// Calculate the next DateRange, using the set DateRangeType.
    /// </summary>
    /// <returns></returns>
    public DateRange Next()
    {
        AssertDateRangeTypeIsSet();

        var start = GetNextStartDate(Type, GetStartDate(Type, Start), 1);
        return new DateRange
        {
            Start = start,
            End = GetNextStartDate(Type, start, 1).Add(NegativeSingleDay),
            Type = Type
        };
    }

    /// <summary>
    /// Calculate the previous DateRange, using the set DateRangeType.
    /// </summary>
    /// <returns></returns>
    public DateRange Previous()
    {
        AssertDateRangeTypeIsSet();

        var start = GetNextStartDate(Type, Start, -1);
        return new DateRange
        {
            Start = start,
            End = GetNextStartDate(Type, start, 1).Add(NegativeSingleDay),
            Type = Type
        };
    }

    /// <summary>
    /// Get a DateRange with calculated start and end points based on the given date
    /// and DateRangeType.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="date"></param>
    /// <returns></returns>
    public static DateRange RangeBasedOnDate(DateRangeType type, DateTime date)
    {
        var start = GetStartDate(type, date.Date);
        return new DateRange
        {
            Type = type,
            Start = start,
            End = GetNextStartDate(type, start, 1).Add(NegativeSingleDay)
        };
    }

    /// <summary>
    /// Get a DateRange with calculated end point based on the given date and DateRangeType.
    /// The start date will always be the given date.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="date"></param>
    /// <returns></returns>
    public static DateRange RangeStartingOnDate(DateRangeType type, DateTime date)
    {
        var start = GetStartDate(type, date.Date);
        return new DateRange
        {
            Type = type,
            Start = date.Date,
            End = GetNextStartDate(type, start, 1).Add(NegativeSingleDay)
        };
    }

    public override string ToString()
    {
        return $"{Start.ToShortDateString()} ... {End.ToShortDateString()} ({Type})";
    }
}