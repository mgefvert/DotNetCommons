using System;

namespace DotNetCommons.Temporal;

/// <summary>
/// Class that encapsulates the last day of the month calculations (e.g. last Thursday of October).
/// </summary>
public class LastDayHoliday : Holiday
{
    public int Month { get; }
    public int Offset { get; }
    public DayOfWeek DayOfWeek { get; }

    public LastDayHoliday(string name, HolidayType type, int month, int dayOfWeek)
        : this(name, type, month, (DayOfWeek)dayOfWeek, 0)
    {
    }

    public LastDayHoliday(string name, HolidayType type, int month, int dayOfWeek, int offset)
        : this(name, type, month, (DayOfWeek)dayOfWeek, offset)
    {
    }

    public LastDayHoliday(string name, HolidayType type, int month, DayOfWeek dayOfWeek)
        : this(name, type, month, dayOfWeek, 0)
    {
    }

    public LastDayHoliday(string name, HolidayType type, int month, DayOfWeek dayOfWeek, int offset)
        : base(name, type)
    {
        Month = month;
        DayOfWeek = dayOfWeek;
        Offset = offset;
    }

    public override string TextDefinition() => $"[last,{Name},{Type},{Month},{DayOfWeek}{(Offset == 0 ? "" : "," + Offset)}]";

    protected internal override DateTime InternalCalculateDate(int year)
    {
        var result = new DateTime(year, Month, 1).AddMonths(1).AddDays(-1);
        while (result.DayOfWeek != DayOfWeek)
            result = result.AddDays(-1);

        return result.AddDays(Offset);
    }
}