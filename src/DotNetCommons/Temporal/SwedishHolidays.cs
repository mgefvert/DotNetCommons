namespace DotNetCommons.Temporal;

/// <summary>
/// Contains the most common Swedish holidays.
/// </summary>
public static class SwedishHolidays
{
    public static Holiday NewYearsDay { get; }
    public static Holiday Epiphany { get; }
    public static Holiday GoodFriday { get; }
    public static Holiday Easter { get; }
    public static Holiday EasterMonday { get; }
    public static Holiday MayFirst { get; }
    public static Holiday AscensionDay { get; }
    public static Holiday Pentecost { get; }
    public static Holiday NationalDayOfSweden { get; }
    public static Holiday Midsummer { get; }
    public static Holiday AllSaintsDay { get; }
    public static Holiday ChristmasDay { get; }
    public static Holiday BoxingDay { get; }

    public static Holiday[] All { get; }

    static SwedishHolidays()
    {
        All =
        [
            NewYearsDay         = new DateBasedHoliday("Nyårsdagen", HolidayType.Holiday, 1, 1),
            Epiphany            = new DateBasedHoliday("Trettondedag jul", HolidayType.Holiday, 1, 6),
            GoodFriday          = new EasterHoliday("Långfredag", HolidayType.Holiday, -2),
            Easter              = new EasterHoliday("Påskdagen", HolidayType.Holiday),
            EasterMonday        = new EasterHoliday("Annandag påsk", HolidayType.Holiday, 1),
            MayFirst            = new DateBasedHoliday("Första maj", HolidayType.Holiday, 5, 1),
            AscensionDay        = new EasterHoliday("Kristi himmelfärdsdag", HolidayType.Holiday, 39),
            Pentecost           = new EasterHoliday("Pingstdagen", HolidayType.Holiday, 49),
            NationalDayOfSweden = new DateBasedHoliday("Sveriges nationaldag", HolidayType.Holiday, 6, 6),
            Midsummer           = new BetweenDaysHoliday("Midsommar", HolidayType.Holiday, 6, 20, 6, 26, DayOfWeek.Saturday),
            AllSaintsDay        = new BetweenDaysHoliday("Alla helgons dag", HolidayType.Holiday, 10, 31, 11, 6, DayOfWeek.Saturday),
            ChristmasDay        = new DateBasedHoliday("Juldagen", HolidayType.Holiday, 12, 25),
            BoxingDay           = new DateBasedHoliday("Annandag jul", HolidayType.Holiday, 12, 26)
        ];
    }
}