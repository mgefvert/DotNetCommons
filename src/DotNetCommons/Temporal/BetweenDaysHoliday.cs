namespace DotNetCommons.Temporal;

/// <summary>
/// Class that represents a holiday as a given weekday between two possible dates.
/// </summary>
public class BetweenDaysHoliday : Holiday
{
    public int MaxDay { get; }
    public int MaxMonth { get; }
    public int MinDay { get; }
    public int MinMonth { get; }
    public DayOfWeek DayOfWeek { get; }

    public BetweenDaysHoliday(string name, HolidayType type, int minMonth, int minDay, int maxMonth, int maxDay, int dayOfWeek)
        : this(name, type, minMonth, minDay, maxMonth, maxDay, (DayOfWeek)dayOfWeek)
    {
    }

    public BetweenDaysHoliday(string name, HolidayType type, int minMonth, int minDay, int maxMonth, int maxDay, DayOfWeek dayOfWeek)
        : base(name, type)
    {
        MinMonth = minMonth;
        MinDay = minDay;
        MaxMonth = maxMonth;
        MaxDay = maxDay;
        DayOfWeek = dayOfWeek;

        try
        {
            // Use 1900 as sample year, since the parameters have to be valid for any year
            _ = new DateTime(1900, MinMonth, MinDay);
            _ = new DateTime(1900, MaxMonth, MaxDay);
        }
        catch (Exception ex)
        {
            throw new ArgumentException($"Minimum ({minMonth}-{minDay}) or maximum ({maxMonth}-{maxDay}) month/day is out of range.", ex);
        }
    }

    public override string TextDefinition() => $"[between,{Name},{Type},{MinMonth},{MinDay},{MaxMonth},{MaxDay},{DayOfWeek}]";

    protected internal override DateTime InternalCalculateDate(int year)
    {
        var d0 = new DateTime(year, MinMonth, MinDay);
        var d1 = new DateTime(year, MaxMonth, MaxDay);
        var d = d0;

        while (d <= d1)
        {
            if (d.DayOfWeek == DayOfWeek)
                return d;

            d = d.AddDays(1);
        }

        throw new Exception($"No holiday found between {d0:d} and {d1:d}");
    }
}