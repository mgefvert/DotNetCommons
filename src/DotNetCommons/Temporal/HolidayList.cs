using System;
using System.Collections.Generic;
using System.Linq;

// ReSharper disable UnusedMember.Global

namespace DotNetCommons.Temporal;

/// <summary>
/// Contains a list of common holidays.
/// </summary>
public class HolidayList : List<Holiday>
{
    public HolidayList()
    {
    }

    public HolidayList(IEnumerable<Holiday> holidays)
    {
        AddRange(holidays);
    }

    /// <summary>
    /// Tests whether a particular day is a holiday.
    /// </summary>
    /// <param name="date">Date to test against. Can be any particular year.</param>
    /// <returns>The given holiday if this date falls on a holiday, otherwise NULL.</returns>
    public Holiday IsHoliday(DateTime date)
    {
        return this.FirstOrDefault(x => x.IsHoliday(date));
    }

    /// <summary>
    /// Tests whether a particular day is an observed holiday.
    /// </summary>
    /// <param name="date">Date to test against. Can be any particular year.</param>
    /// <returns>The given holiday if this date falls on a holiday, otherwise NULL.</returns>
    public Holiday IsObservedHoliday(DateTime date)
    {
        return this.FirstOrDefault(x => x.IsObservedHoliday(date));
    }
}