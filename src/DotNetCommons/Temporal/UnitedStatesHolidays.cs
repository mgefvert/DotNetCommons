// ReSharper disable UnusedMember.Global

namespace DotNetCommons.Temporal;

/// <summary>
/// Contains the most common U.S. Federal Holidays and a few more.
/// </summary>
public static class UnitedStatesHolidays
{
    public static Holiday NewYearsDay { get; }
    public static Holiday MlkBirthday { get; }
    public static Holiday PresidentsDay { get; }
    public static Holiday Easter { get; }
    public static Holiday MemorialDay { get; }
    public static Holiday Juneteenth { get; }
    public static Holiday IndependenceDay { get; }
    public static Holiday LaborDay { get; }
    public static Holiday VeteransDay { get; }
    public static Holiday Thanksgiving { get; }
    public static Holiday ChristmasDay { get; }

    public static Holiday[] All { get; }

    public static DateTime FederalObservedRules(DateTime date)
    {
        return date.DayOfWeek switch
        {
            DayOfWeek.Saturday => date.AddDays(-1),
            DayOfWeek.Sunday   => date.AddDays(1),
            _                  => date
        };
    }

    static UnitedStatesHolidays()
    {
        All =
        [
            NewYearsDay     = new DateBasedHoliday("New Year's Day", HolidayType.Holiday, 1, 1),
            MlkBirthday     = new NthDayHoliday("Martin Luther King's Birthday", HolidayType.Holiday, 1, 3, DayOfWeek.Monday),
            PresidentsDay   = new NthDayHoliday("Washington's Birthday", HolidayType.Holiday, 2, 3, DayOfWeek.Monday),
            Easter          = new EasterHoliday("Easter Sunday", HolidayType.Holiday),
            MemorialDay     = new LastDayHoliday("Memorial Day", HolidayType.Holiday, 5, DayOfWeek.Monday),
            Juneteenth      = new DateBasedHoliday("Juneteenth", HolidayType.Holiday, 6, 19),
            IndependenceDay = new DateBasedHoliday("Independence Day", HolidayType.Holiday, 7, 4),
            LaborDay        = new NthDayHoliday("Labor Day", HolidayType.Holiday, 9, 1, DayOfWeek.Monday),
            VeteransDay     = new DateBasedHoliday("Veteran's Day", HolidayType.Holiday, 11, 11),
            Thanksgiving    = new NthDayHoliday("Thanksgiving", HolidayType.Holiday, 11, 4, DayOfWeek.Thursday),
            ChristmasDay    = new DateBasedHoliday("Christmas Day", HolidayType.Holiday, 12, 25)
        ];

        MlkBirthday.ObservedRule     = FederalObservedRules;
        PresidentsDay.ObservedRule   = FederalObservedRules;
        MemorialDay.ObservedRule     = FederalObservedRules;
        IndependenceDay.ObservedRule = FederalObservedRules;
        LaborDay.ObservedRule        = FederalObservedRules;
        VeteransDay.ObservedRule     = FederalObservedRules;
    }
}