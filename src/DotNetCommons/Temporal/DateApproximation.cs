using System.Globalization;

namespace DotNetCommons.Temporal;

/// <summary>
/// Represents an approximation of a date that may contain partial or complete date information such as year,
/// year and month, or year, month, and day.
/// </summary>
/// <remarks>
/// This record provides functionality for representing and manipulating partial dates, including comparison,
/// formatting, and conversion. It ensures date validity by enforcing logical constraints such as not allowing
/// a day to be defined without a month or a month without a year.
/// </remarks>
public record DateApproximation : IComparable<DateApproximation>, IComparable
{
    /// Gets the year component. It must be a value between 0 and 9999, where 0 indicates that it is unknown.
    public int Year { get; }

    /// Gets the month component. It must be a value between 0 and 12, where 0 indicates that it is unknown.
    public int Month { get; }

    /// Gets the day component. It must be a value between 0 and 28-31 (depending on the month property),
    /// where 0 indicates that it is unknown.
    public int Day { get; }

    /// Parses a string representation of a date. The string may contain partial or complete date information
    /// in the format "YYYY-MM-DD", "YYYY-MM", or "YYYY"; and either YYYY, MM or DD may be all zeroes.
    public static DateApproximation FromString(string date)
    {
        var parts = date.Split('-');
        if (parts.Length > 2)
            throw new ArgumentException($"Invalid date '{date}'");

        return new DateApproximation(
            parts.ElementAtOrDefault(0).ParseInt(),
            parts.ElementAtOrDefault(1).ParseInt(),
            parts.ElementAtOrDefault(2).ParseInt());
    }

    /// Represents an approximation of a date that may contain partial or complete date information, such as year;
    /// year and month; or year, month, and day.
    public DateApproximation(int y = 0, int m = 0, int d = 0)
    {
        // Verify that dates are valid, even if undefined
        if (d != 0 && m == 0)
            throw new ArgumentException("Invalid date: Day cannot be defined without Month");
        if (m != 0 && y == 0)
            throw new ArgumentException("Invalid date: Month cannot be defined without Year");

        if (y < 0 || y > 9999)
            throw new ArgumentOutOfRangeException(nameof(y));

        if (m < 0 || m > 12)
            throw new ArgumentOutOfRangeException(nameof(m));

        if (d != 0)
            _ = new DateTime(y, m, d);

        Year  = y;
        Month = m;
        Day   = d;
    }

    public DateApproximation(DateTime date) : this(date.Year, date.Month, date.Day)
    {
    }

    public DateApproximation(DateOnly date) : this(date.Year, date.Month, date.Day)
    {
    }

    /// Returns a string representation of the year. If the year is not defined, it returns "Unknown".
    public string FormatYear()
    {
        return Year != 0 ? Year.ToString() : "Unknown";
    }

    /// Formats the year and month components of the date approximation into a readable string.
    /// If the year is unspecified, returns "Unknown". If the month is unspecified but the year is present,
    /// returns "Unknown {Year}". Otherwise, returns the full month name and year in the format
    /// "{MonthName} {Year}".
    public string FormatYearMonth()
    {
        if (Year == 0)
            return "Unknown";

        if (Month == 0)
            return $"Unknown {Year}";

        var monthName = CultureInfo.InvariantCulture.DateTimeFormat.GetMonthName(Month);
        return $"{monthName} {Year}";
    }

    /// Returns a new instance with only the year component retained, and the rest zero.
    public DateApproximation ToYOnly() => new(Year);

    /// Returns a new instance with the year and month components retained, and the day zero.
    public DateApproximation ToYMOnly() => new(Year, Month);

    /// Converts the year component to a string representation formatted as "YYYY".
    /// If the year is unknown, it will return "0000".
    public string ToYString() => $"{Year:D4}";

    /// Converts the year and month components to a string representation formatted as "YYYY-MM".
    /// If any component is unknown, it will return zeroes, e.g. "0000-00" or "2026-00".
    public string ToYMString()  => $"{Year:D4}-{Month:D2}";

    /// Converts the date components to a string representation formatted as "YYYY-MM-DD".
    /// If any component is unknown, it will return zeroes, e.g. "0000-00", "2026-00", or "2026-04-00".
    public string ToYMDString() => $"{Year:D4}-{Month:D2}-{Day:D2}";

    public int CompareTo(DateApproximation? other)
    {
        if (ReferenceEquals(this, other))
            return 0;

        if (other is null)
            return 1;

        var yearComparison = Year - other.Year;
        if (yearComparison != 0)
            return yearComparison;

        var monthComparison = Month - other.Month;
        if (monthComparison != 0)
            return monthComparison;

        return Day - other.Day;
    }

    public int CompareTo(object? obj)
    {
        if (obj is null)
            return 1;

        if (obj is DateApproximation other)
            return CompareTo(other);

        throw new ArgumentException($"Object must be of type {nameof(DateApproximation)}", nameof(obj));
    }
}