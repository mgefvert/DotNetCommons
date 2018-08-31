using System;
using System.Collections.Generic;
using System.Linq;
using DotNetCommons.Text;

namespace DotNetCommons.Temporal
{
    public enum HolidayType
    {
        Date = 1,
        NthWeek = 2,
        LastWeek = 3,
        Easter = 4
    }

    /// <summary>
    /// Contains the most common U.S. Federal Holidays and a few more.
    /// </summary>
    public static class CommonHolidays
    {
        private static readonly Holiday[] Cache = new Holiday[14];

        public static Holiday NewYearsDay     => Cache[0] ?? (Cache[0] = Holiday.CreateSpecificDate(1, 1, "New Year's Day"));
        public static Holiday MlkBirthday     => Cache[1] ?? (Cache[1] = Holiday.CreateDayInNthWeek(1, 3, DayOfWeek.Monday, 0, "Martin Luther King's Birthday"));
        public static Holiday PresidentsDay   => Cache[2] ?? (Cache[2] = Holiday.CreateDayInNthWeek(2, 3, DayOfWeek.Monday, 0, "Washington's Birthday"));
        public static Holiday Easter          => Cache[3] ?? (Cache[3] = Holiday.CreateEaster("Easter Sunday"));
        public static Holiday MemorialDay     => Cache[4] ?? (Cache[4] = Holiday.CreateDayInLastWeek(5, DayOfWeek.Monday, 0, "Memorial Day"));
        public static Holiday IndependenceDay => Cache[5] ?? (Cache[5] = Holiday.CreateSpecificDate(7, 4, "Independence Day"));
        public static Holiday LaborDay        => Cache[6] ?? (Cache[6] = Holiday.CreateDayInNthWeek(9, 1, DayOfWeek.Monday, 0, "Labor Day"));
        public static Holiday ColumbusDay     => Cache[7] ?? (Cache[7] = Holiday.CreateDayInNthWeek(10, 2, DayOfWeek.Monday, 0, "Columbus Day"));
        public static Holiday VeteransDay     => Cache[8] ?? (Cache[8] = Holiday.CreateSpecificDate(11, 11, "Veteran's Day"));
        public static Holiday Thanksgiving    => Cache[9] ?? (Cache[9] = Holiday.CreateDayInNthWeek(11, 4, DayOfWeek.Thursday, 0, "Thanksgiving"));
        public static Holiday ChristmasEve    => Cache[10] ?? (Cache[10] = Holiday.CreateSpecificDate(12, 24, "Christmas Eve"));
        public static Holiday ChristmasDay    => Cache[11] ?? (Cache[11] = Holiday.CreateSpecificDate(12, 25, "Christmas Day"));
        public static Holiday BoxingDay       => Cache[12] ?? (Cache[12] = Holiday.CreateSpecificDate(12, 26, "Boxing Day"));
        public static Holiday NewYearsEve     => Cache[13] ?? (Cache[13] = Holiday.CreateSpecificDate(12, 31, "New Year's Eve"));
    }

    /// <summary>
    /// Contains a list of common holidays.
    /// </summary>
    public static class Holidays
    {
        private static List<Holiday> _list;

        /// <summary>
        /// Return a collection of all registered holidays.
        /// </summary>
        /// <returns></returns>
        public static ICollection<Holiday> All()
        {
            Initialize();
            return _list.AsReadOnly();
        }

        /// <summary>
        /// Clear the list of predefined holidays.
        /// </summary>
        public static void Clear()
        {
            _list = new List<Holiday>();
        }

        private static void Initialize()
        {
            if (_list != null)
                return;

            _list = new List<Holiday>(new[] {
                CommonHolidays.NewYearsDay, CommonHolidays.MlkBirthday, CommonHolidays.PresidentsDay,
                CommonHolidays.Easter, CommonHolidays.MemorialDay, CommonHolidays.IndependenceDay,
                CommonHolidays.LaborDay, CommonHolidays.ColumbusDay, CommonHolidays.VeteransDay,
                CommonHolidays.Thanksgiving, CommonHolidays.ChristmasEve, CommonHolidays.ChristmasDay,
                CommonHolidays.BoxingDay, CommonHolidays.NewYearsEve
            });
        }

        /// <summary>
        /// Tests whether a particular day is a holiday.
        /// </summary>
        /// <param name="date">Date to test against. Can be any particular year.</param>
        /// <returns>The given holiday if this date falls on a holiday, otherwise NULL.</returns>
        public static Holiday IsHoliday(DateTime date)
        {
            Initialize();
            return _list.FirstOrDefault(x => x.IsHoliday(date));
        }

        /// <summary>
        /// Register a new holiday.
        /// </summary>
        /// <param name="holiday"></param>
        public static void Register(params Holiday[] holiday)
        {
            Initialize();
            _list.AddRange(holiday);
        }

        /// <summary>
        /// Remove a particular holiday.
        /// </summary>
        /// <param name="holiday"></param>
        public static void Remove(Holiday holiday)
        {
            Initialize();
            _list.Remove(holiday);
        }

        /// <summary>
        /// Remove a particular holiday by name.
        /// </summary>
        /// <param name="name">Name to remove, case insensitive.</param>
        public static void Remove(string name)
        {
            Initialize();
            _list.RemoveAll(x => x.Name.Like(name));
        }
    }

    /// <summary>
    /// Class that encapsulates functionality for a particular holiday.
    /// </summary>
    public class Holiday
    {
        private DateTime _nextDate = DateTime.MinValue;

        /// <summary>
        /// When the next holiday occurs.
        /// </summary>
        public DateTime NextDate
        {
            get
            {
                if (_nextDate < DateTime.Today)
                    Recalculate();

                return _nextDate;
            }
        }

        /// <summary>
        /// Holiday definition type (how it's calculcated).
        /// </summary>
        public HolidayType HolidayType { get; private set; }

        /// <summary>
        /// Name of the holiday.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// What month it occurs in.
        /// </summary>
        public int CalcMonth { get; private set; }

        /// <summary>
        /// What week, if any, it occurs in.
        /// </summary>
        public int CalcWeek { get; private set; }

        /// <summary>
        /// What day of the month it occurs in, if given.
        /// </summary>
        public int CalcDay { get; private set; }

        /// <summary>
        /// What day of the week this holiday occurs on, if any.
        /// </summary>
        public DayOfWeek CalcDayOfWeek { get; private set; }

        /// <summary>
        /// Any days to add or subtract to the calculation.
        /// </summary>
        public int CalcAddDays { get; private set; }

        /// <summary>
        /// Get a definition for the holiday that can be persisted and given to new instances.
        /// </summary>
        /// <returns>A string representation of the holiday definition.</returns>
        public string GetDefinition()
        {
            return "[" + string.Join(",", HolidayType.ToString(), Name, CalcMonth, CalcWeek, CalcDay, (int)CalcDayOfWeek, CalcAddDays) + "]";
        }

        /// <summary>
        /// A string representation on the format "Holiday: Date".
        /// </summary>
        /// <returns>String containing the name of the holiday and date it occurs on.</returns>
        public override string ToString()
        {
            return (Name != null ? Name + ": " : "") + NextDate.ToShortDateString();
        }

        /// <summary>
        /// Create a new holiday according to a given definition.
        /// </summary>
        /// <param name="definition">Definition in a persisted format.</param>
        public Holiday(string definition = null)
        {
            if (string.IsNullOrEmpty(definition))
                return;

            definition = definition.Trim('[', ']');
            var items = definition.Split(',').TrimAndFilter().ToArray();
            if (items.Length != 7)
                throw new InvalidOperationException($"Holiday definition {definition} is invalid.");

            HolidayType = (HolidayType)Enum.Parse(typeof(HolidayType), items[0], true);
            Name = items[1];
            CalcMonth = int.Parse(items[2]);
            CalcWeek = int.Parse(items[3]);
            CalcDay = int.Parse(items[4]);
            CalcDayOfWeek = (DayOfWeek)int.Parse(items[5]);
            CalcAddDays = int.Parse(items[6]);
        }

        /// <summary>
        /// Create a new holiday with the easter formula calculation.
        /// </summary>
        /// <param name="name">Name of the holiday.</param>
        /// <returns>A new holiday object.</returns>
        public static Holiday CreateEaster(string name = null) 
            => new Holiday
            {
                HolidayType = HolidayType.Easter,
                Name = name
            };

        /// <summary>
        /// Create a new holiday with a specific date of the year.
        /// </summary>
        /// <param name="month">Month it occurs in.</param>
        /// <param name="day">Day of the month.</param>
        /// <param name="name">Optional holiday name.</param>
        /// <returns>A new holiday object.</returns>
        public static Holiday CreateSpecificDate(int month, int day, string name = null) 
            => new Holiday
            {
                HolidayType = HolidayType.Date,
                CalcMonth = month,
                CalcDay = day,
                Name = name
            };

        /// <summary>
        /// Create a new holiday that occurs in the Nth week of a given month (e.g. first Sunday of the August).
        /// </summary>
        /// <param name="month">Month it occurs in.</param>
        /// <param name="week">Week number (starting with 1).</param>
        /// <param name="day">Which day of the week.</param>
        /// <param name="dayOffset">Any days added or subtracted to the given weekday.</param>
        /// <param name="name">Optional holiday name.</param>
        /// <returns>A new holiday object.</returns>
        public static Holiday CreateDayInNthWeek(int month, int week, DayOfWeek day, int dayOffset = 0, string name = null) 
            => new Holiday
            {
                HolidayType = HolidayType.NthWeek,
                CalcMonth = month,
                CalcWeek = week,
                CalcDayOfWeek = day,
                CalcAddDays = dayOffset,
                Name = name
            };

        /// <summary>
        /// Create a new holiday that occurs in the last week of a given month (e.g. last Tuesday in April)
        /// </summary>
        /// <param name="month">Month it occurs in.</param>
        /// <param name="day">Which day of the week.</param>
        /// <param name="dayOffset">Any days added or subtracted to the given weekday.</param>
        /// <param name="name">Optional holiday name.</param>
        /// <returns>A new holiday object.</returns>
        public static Holiday CreateDayInLastWeek(int month, DayOfWeek day, int dayOffset = 0, string name = null)
            => new Holiday
            {
                HolidayType = HolidayType.LastWeek,
                CalcMonth = month,
                CalcDayOfWeek = day,
                CalcAddDays = dayOffset,
                Name = name
            };

        /// <summary>
        /// Number of business days (Mondays - Fridays) left until the holiday. Does not include the
        /// holiday itself, nor any other holidays.
        /// </summary>
        /// <returns>Business days left.</returns>
        public int BusinessDaysLeft()
        {
            var result = 0;
            var dt = DateTime.Today;
            var next = NextDate;

            while (dt < next)
            {
                if (dt.DayOfWeek != DayOfWeek.Sunday && dt.DayOfWeek != DayOfWeek.Saturday)
                    result++;

                dt = dt.AddDays(1);
            }

            return result;
        }

        /// <summary>
        /// Calculate the date of this holiday for a given year.
        /// </summary>
        /// <param name="year">Year.</param>
        /// <returns>The date on which this holiday occurred that year.</returns>
        public DateTime CalculateDate(int year)
        {
            switch (HolidayType)
            {
                case HolidayType.Date:
                    return new DateTime(year, CalcMonth, CalcDay);
                case HolidayType.Easter:
                    return GetEasterSundayDate(year);
                case HolidayType.LastWeek:
                    return GetDayInLastWeekOfMonth(year, CalcMonth, CalcDayOfWeek).AddDays(CalcAddDays);
                case HolidayType.NthWeek:
                    return GetDayInNthWeekOfMonth(year, CalcMonth, CalcWeek, CalcDayOfWeek).AddDays(CalcAddDays);
                default:
                    throw new InvalidOperationException("Unrecognized holiday type " + HolidayType);
            }
        }

        /// <summary>
        /// Number of whole days left until the holiday. Does not include the holiday itself.
        /// </summary>
        /// <returns>Days left.</returns>
        public int DaysLeft()
        {
            return (int) (NextDate - DateTime.Today).TotalDays;
        }

        public static DateTime GetDayInLastWeekOfMonth(int year, int month, DayOfWeek day)
        {
            var result = new DateTime(year, month, 1).AddMonths(1).AddDays(-1);
            while (result.DayOfWeek != day)
                result = result.AddDays(-1);

            return result;
        }

        public static DateTime GetDayInNthWeekOfMonth(int year, int month, int week, DayOfWeek day)
        {
            if (week < 1)
                week = 1;
            if (week > 5)
                week = 5;

            var result = new DateTime(year, month, 1);
            while (result.DayOfWeek != day)
                result = result.AddDays(1);

            return result.AddDays((week - 1) * 7);
        }

        public static DateTime GetEasterSundayDate(int year)
        {
            var a = year % 19;
            var b = year / 100;
            var c = year % 100;
            var d = b / 4;
            var e = b % 4;
            var f = (b + 8) / 25;
            var g = (b - f + 1) / 3;
            var h = (19 * a + b - d - g + 15) % 30;
            var i = c / 4;
            var k = c % 4;
            var l = (32 + 2 * e + 2 * i - h - k) % 7;
            var m = (a + 11 * h + 22 * l) / 451;
            var n = (h + l - 7 * m + 114) / 31;
            var p = (h + l - 7 * m + 114) % 31;

            return new DateTime(year, n, p + 1);
        }

        /// <summary>
        /// Determine whether the given date falls on this holiday.
        /// </summary>
        /// <param name="date">Date to test.</param>
        /// <returns>True if the date is the holiday for that year.</returns>
        public bool IsHoliday(DateTime date)
        {
            return date.Date == CalculateDate(date.Year);
        }

        private void Recalculate()
        {
            var year = DateTime.Today.Year;
            if (_nextDate.Year == year)
            {
                _nextDate = CalculateDate(year + 1);
            }
            else
            {
                _nextDate = CalculateDate(year);
                if (_nextDate < DateTime.Today)
                    _nextDate = CalculateDate(year + 1);
            }
        }
    }
}
