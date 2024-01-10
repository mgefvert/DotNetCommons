using System;

namespace DotNetCommons.Temporal;

/// <summary>
/// Class that represents a holiday occurring relative to another holiday (days after or before).
/// </summary>
public class RelativeHoliday : Holiday
{
    public Holiday OriginalHoliday { get; }
    public int DaysOffset { get; }

    public RelativeHoliday(string name, HolidayType type, Holiday originalHoliday, int daysOffset)
        : base(name, type)
    {
        OriginalHoliday = originalHoliday;
        DaysOffset = daysOffset;
    }

    public override string TextDefinition() => $"[relative,{Name},{Type},{OriginalHoliday.Name},{DaysOffset}]";

    protected internal override DateTime InternalCalculateDate(int year)
    {
        var date = OriginalHoliday.InternalCalculateDate(year);
        return date.AddDays(DaysOffset);
    }
}