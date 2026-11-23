// ReSharper disable UnusedMember.Global

namespace DotNetCommons.Temporal;

[Flags]
public enum HolidayType
{
    Holiday = 1,
    Unofficial = 2,
    Private = 4,
    HalfDay = 8
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

    static Holiday()
    {
        RegisterHolidayClass<DateBasedHoliday>("date");
        RegisterHolidayClass<EasterHoliday>("easter");
        RegisterHolidayClass<LastDayHoliday>("last");
        RegisterHolidayClass<NthDayHoliday>("nth");
        RegisterHolidayClass<BetweenDaysHoliday>("between");
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
        if (items.Length < 3)
            throw new InvalidOperationException($"Holiday definition {definition} is invalid.");

        var type = RegisteredTypes.GetValueOrDefault(items[0]);
        if (type == null)
            throw new ArgumentException($"Holiday type '{items[0]}' has not been registered.");

        var holidayType = Enum.Parse<HolidayType>(items[2], true);
        var firstArgument = 3;
        // Older text definitions wrote flag names separated by commas.
        while (firstArgument < items.Length && !int.TryParse(items[firstArgument], out _) &&
               Enum.TryParse<HolidayType>(items[firstArgument], true, out var flag) && Enum.IsDefined(flag))
        {
            holidayType |= flag;
            firstArgument++;
        }

        var args = new List<object>
        {
            items[1],
            holidayType
        };

        args.AddRange(items.Skip(firstArgument).Select(item => int.TryParse(item, out var value)
            ? value
            : (int)Enum.Parse<DayOfWeek>(item, true)).Cast<object>());

        try
        {
            return (Holiday?)Activator.CreateInstance(type, args.ToArray()) ?? throw new InvalidDataException();
        }
        catch (Exception e)
        {
            throw new InvalidDataException("Unable to instantiate holiday from definition: " + definition, e);
        }
    }

    private readonly Dictionary<int, DateTime> _dates = new();

    /// <summary>
    /// Holiday type (its status - half day, federal, unofficial etc).
    /// </summary>
    public HolidayType Type { get; set; }

    /// <summary>
    /// Name of the holiday.
    /// </summary>
    public string? Name { get; }

    /// <summary>
    /// Rule for determining observed dates (when the holiday is shifted due to weekdays etc).
    /// </summary>
    public Func<DateTime, DateTime>? ObservedRule { get; set; }

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
    public int BusinessDaysLeft(TimeProvider? clock = null)
    {
        clock ??= TimeProvider.System;

        var result = 0;
        var dt = clock.Today;
        var next = NextDate(clock);

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
    public int DaysLeft(TimeProvider? clock = null)
    {
        clock ??= TimeProvider.System;

        return (int)(NextDate(clock) - clock.Today).TotalDays;
    }

    /// <summary>
    /// Determine whether the given date falls on this holiday. Occurrences calculated for the
    /// adjacent years are also checked to account for dates shifted across a year boundary.
    /// </summary>
    /// <param name="date">Date to test.</param>
    /// <param name="applyObservedRule">Whether to apply observation rules or not.</param>
    /// <returns>True if the date is the holiday for that year.</returns>
    public bool IsHoliday(DateTime date, bool applyObservedRule)
    {
        var day = date.Date;
        if (day == CalculateDate(date.Year, applyObservedRule))
            return true;

        if ((!applyObservedRule || ObservedRule == null) && IsBoundedToYear())
            return false;

        return (date.Year > 1 && TryCalculateAdjacentDate(date.Year - 1, applyObservedRule, out var previous) && day == previous)
               || (date.Year < 9999 && TryCalculateAdjacentDate(date.Year + 1, applyObservedRule, out var next) && day == next);
    }

    private bool TryCalculateAdjacentDate(int year, bool applyObservedRule, out DateTime date)
    {
        if (!HasOccurrenceInYear(this, year))
        {
            date = default;
            return false;
        }

        date = CalculateDate(year, applyObservedRule);
        return true;
    }

    private static bool HasOccurrenceInYear(Holiday holiday, int year)
    {
        if (holiday.GetType() == typeof(DateBasedHoliday) && holiday is DateBasedHoliday { Month: 2, Day: 29 })
            return DateTime.IsLeapYear(year);

        if (holiday.GetType() == typeof(BetweenDaysHoliday) && holiday is BetweenDaysHoliday between)
        {
            var start = new DateTime(year, between.MinMonth, between.MinDay);
            var end = new DateTime(year, between.MaxMonth, between.MaxDay);
            var daysUntilMatch = ((int)between.DayOfWeek - (int)start.DayOfWeek + 7) % 7;
            return (end - start).TotalDays >= daysUntilMatch;
        }

        return holiday.GetType() != typeof(RelativeHoliday) || holiday is not RelativeHoliday relative ||
               HasOccurrenceInYear(relative.OriginalHoliday, year);
    }

    private bool IsBoundedToYear() => GetType() == typeof(DateBasedHoliday) || GetType() == typeof(BetweenDaysHoliday);

    /// <summary>
    /// When the next holiday occurs, considering occurrences calculated for the adjacent years.
    /// </summary>
    public DateTime NextDate(TimeProvider? clock = null)
    {
        var today = (clock ?? TimeProvider.System).Today;

        if (ObservedRule == null && IsBoundedToYear())
        {
            var current = CalculateDate(today.Year, true);
            return current >= today ? current : CalculateDate(today.Year + 1, true);
        }

        DateTime? next = null;
        for (var year = Math.Max(1, today.Year - 1); year <= Math.Min(9999, today.Year + 1); year++)
        {
            DateTime candidate;
            if (year == today.Year)
                candidate = CalculateDate(year, true);
            else if (!TryCalculateAdjacentDate(year, true, out candidate))
                continue;

            if (candidate >= today && (next == null || candidate < next.Value))
                next = candidate;
        }

        return next ?? throw new InvalidOperationException("No future holiday date is available.");
    }

    /// <summary>
    /// A string representation on the format "Holiday: Date".
    /// </summary>
    /// <returns>String containing the name of the holiday and date it occurs on.</returns>
    public override string ToString()
    {
        return (Name != null ? Name + ": " : "") + NextDate().ToShortDateString();
    }
}
