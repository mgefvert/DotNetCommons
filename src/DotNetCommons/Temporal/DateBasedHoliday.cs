using System;

namespace DotNetCommons.Temporal;

public class DateBasedHoliday : Holiday
{
    public int Month { get; }
    public int Day { get; }

    public DateBasedHoliday(string name, HolidayType type, int month, int day)
        : base(name, type)
    {
        Month = month;
        Day = day;
    }

    public override string TextDefinition() => $"[date,{Name},{Type},{Month},{Day}]";

    protected internal override DateTime InternalCalculateDate(int year) => new(year, Month, Day);
}