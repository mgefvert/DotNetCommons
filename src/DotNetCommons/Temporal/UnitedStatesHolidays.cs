using System;

// ReSharper disable UnusedMember.Global

namespace DotNetCommons.Temporal
{
    /// <summary>
    /// Contains the most common U.S. Federal Holidays and a few more.
    /// </summary>
    public class UnitedStatesHolidays
    {
        private readonly Holiday[] _list;

        public static DateTime FederalObservedRules(DateTime date)
        {
            if (date.DayOfWeek == DayOfWeek.Saturday)
                return date.AddDays(-1);

            if (date.DayOfWeek == DayOfWeek.Sunday)
                return date.AddDays(1);

            return date;
        }

        public UnitedStatesHolidays()
        {
            _list = new Holiday[]
            {
                new DateBasedHoliday("New Year's Day", HolidayType.Holiday, 1, 1),
                new NthDayHoliday("Martin Luther King's Birthday", HolidayType.Holiday, 1, 3, DayOfWeek.Monday),
                new NthDayHoliday("Washington's Birthday", HolidayType.Holiday, 2, 3, DayOfWeek.Monday),
                new EasterHoliday("Easter Sunday", HolidayType.Holiday),
                new LastDayHoliday("Memorial Day", HolidayType.Holiday, 5, DayOfWeek.Monday),
                new DateBasedHoliday("Independence Day", HolidayType.Holiday, 7, 4),
                new NthDayHoliday("Labor Day", HolidayType.Holiday, 9, 1, DayOfWeek.Monday),
                new NthDayHoliday("Columbus Day", HolidayType.Holiday, 10, 2, DayOfWeek.Monday),
                new DateBasedHoliday("Veteran's Day", HolidayType.Holiday, 11, 11),
                new NthDayHoliday("Thanksgiving", HolidayType.Holiday, 11, 4, DayOfWeek.Thursday),
                new DateBasedHoliday("Christmas Eve", HolidayType.Holiday, 12, 24),
                new DateBasedHoliday("Christmas Day", HolidayType.Holiday, 12, 25)
            };

            MlkBirthday.ObservedRule = FederalObservedRules;
            PresidentsDay.ObservedRule = FederalObservedRules;
            MemorialDay.ObservedRule = FederalObservedRules;
            IndependenceDay.ObservedRule = FederalObservedRules;
            LaborDay.ObservedRule = FederalObservedRules;
            ColumbusDay.ObservedRule = FederalObservedRules;
            VeteransDay.ObservedRule = FederalObservedRules;
        }

        public Holiday[] All => _list;

        public Holiday NewYearsDay => _list[0];
        public Holiday MlkBirthday => _list[1];
        public Holiday PresidentsDay => _list[2];
        public Holiday Easter => _list[3];
        public Holiday MemorialDay => _list[4];
        public Holiday IndependenceDay => _list[5];
        public Holiday LaborDay => _list[6];
        public Holiday ColumbusDay => _list[7];
        public Holiday VeteransDay => _list[8];
        public Holiday Thanksgiving => _list[9];
        public Holiday ChristmasEve => _list[10];
        public Holiday ChristmasDay => _list[11];
    }
}
