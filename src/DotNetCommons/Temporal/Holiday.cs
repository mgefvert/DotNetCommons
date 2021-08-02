using System;
using System.Collections.Generic;
using System.Linq;
using DotNetCommons.Collections;
using DotNetCommons.Text;

// ReSharper disable UnusedMember.Global

namespace DotNetCommons.Temporal
{
    [Flags]
    public enum HolidayType
    {
        Holiday = 1,
        Unofficial = 2,
        Private = 4,
        Halfday = 8
    }

    /// <summary>
    /// Class that encapsulates functionality for a particular holiday.
    /// </summary>
    public abstract class Holiday
    {
        private static readonly Dictionary<string, Type> RegisteredTypes = new(StringComparer.OrdinalIgnoreCase);

        public static void RegisterHolidayClass<T>(string shortcut) where T : Holiday
        {
            RegisteredTypes[shortcut] = typeof(T);
        }

        /// <summary>
        /// Create a new holiday according to a given definition.
        /// </summary>
        /// <param name="definition">Definition in a persisted format.</param>
        public static Holiday Create(string definition)
        {
            if (string.IsNullOrEmpty(definition))
                throw new ArgumentNullException(nameof(definition));

            definition = definition.Trim('[', ']');
            var items = definition.Split(',').TrimAndFilter().ToArray();
            if (items.Length < 2)
                throw new InvalidOperationException($"Holiday definition {definition} is invalid.");

            var name = items[0];
            var holidayType = (HolidayType)int.Parse(items[1]);

            var type = RegisteredTypes.GetOrDefault(name);
            if (type == null)
                throw new ArgumentException($"Holiday type '{name}' has not been registered.");

            var args = new List<object>
            {
                name,
                holidayType
            };

            args.AddRange(items.Skip(2).Select(int.Parse).Cast<object>());

            return (Holiday)Activator.CreateInstance(type, args);
        }

        private readonly Dictionary<int, DateTime> _dates = new();
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
        /// Holiday type (its status - half day, federal, unofficial etc).
        /// </summary>
        public HolidayType Type { get; set; }

        /// <summary>
        /// Name of the holiday.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Rule for determining observed dates (when the holiday is shifted due to weekdays etc).
        /// </summary>
        public Func<DateTime, DateTime> ObservedRule { get; set; }

        protected internal abstract DateTime InternalCalculateDate(int year);
        public abstract string TextDefinition();

        protected Holiday(string name, HolidayType type)
        {
            Name = name;
            Type = type;
        }

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
        /// <param name="applyObservedRule">Apply any observation rules</param>
        /// <returns>The date on which this holiday occurred that year.</returns>
        public DateTime CalculateDate(int year, bool applyObservedRule)
        {
            if (!_dates.TryGetValue(year, out var result))
            {
                result = InternalCalculateDate(year);
                _dates[year] = result;
            }

            return applyObservedRule && ObservedRule != null ? ObservedRule(result) : result;
        }

        /// <summary>
        /// Number of whole days left until the holiday. Does not include the holiday itself.
        /// </summary>
        /// <returns>Days left.</returns>
        public int DaysLeft()
        {
            return (int)(NextDate - DateTime.Today).TotalDays;
        }

        /// <summary>
        /// Determine whether the given date falls on this holiday.
        /// </summary>
        /// <param name="date">Date to test.</param>
        /// <returns>True if the date is the holiday for that year.</returns>
        public bool IsHoliday(DateTime date)
        {
            return date.Date == CalculateDate(date.Year, false);
        }

        /// <summary>
        /// Determine whether the given date falls on this observed holiday.
        /// </summary>
        /// <param name="date">Date to test.</param>
        /// <returns>True if the date is the holiday for that year.</returns>
        public bool IsObservedHoliday(DateTime date)
        {
            return date.Date == CalculateDate(date.Year, true);
        }

        private void Recalculate()
        {
            var year = DateTime.Today.Year;
            if (_nextDate.Year == year)
                _nextDate = CalculateDate(year + 1, false);
            else
            {
                _nextDate = CalculateDate(year, false);
                if (_nextDate < DateTime.Today)
                    _nextDate = CalculateDate(year + 1, false);
            }
        }

        /// <summary>
        /// A string representation on the format "Holiday: Date".
        /// </summary>
        /// <returns>String containing the name of the holiday and date it occurs on.</returns>
        public override string ToString()
        {
            return (Name != null ? Name + ": " : "") + NextDate.ToShortDateString();
        }
    }
}
