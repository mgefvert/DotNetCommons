using DotNetCommons.Temporal;

namespace DotNetCommonTests.Temporal;

[TestClass]
public class HolidayCoverageTests
{
    [TestMethod]
    public void BetweenDays_IncludesBothEndpointsAndCrossesMonths()
    {
        var holiday = new BetweenDaysHoliday("Saturday", HolidayType.Holiday, 10, 31, 11, 6, DayOfWeek.Saturday);
        Assert.AreEqual(new DateTime(2020, 10, 31), holiday.CalculateDate(2020, false));
        Assert.AreEqual(new DateTime(2021, 11, 6), holiday.CalculateDate(2021, false));
        Assert.AreEqual(new DateTime(2024, 11, 2), holiday.CalculateDate(2024, false));
        Assert.ThrowsExactly<ArgumentException>(() => new BetweenDaysHoliday("Invalid", HolidayType.Holiday, 2, 29, 3, 5, DayOfWeek.Monday));
        var noMatch = new BetweenDaysHoliday("No match", HolidayType.Holiday, 1, 1, 1, 1, DayOfWeek.Monday);
        Assert.ThrowsExactly<Exception>(() => noMatch.CalculateDate(2024 + 1, false));
    }

    [TestMethod]
    public void RelativeHoliday_UsesActualOriginalDateAndItsOwnObservedRule()
    {
        var original = new DateBasedHoliday("Original", HolidayType.Holiday, 1, 1) { ObservedRule = d => d.AddDays(1) };
        var relative = new RelativeHoliday("Eve", HolidayType.Unofficial, original, -1) { ObservedRule = d => d.AddDays(-1) };
        Assert.AreSame(original, relative.OriginalHoliday);
        Assert.AreEqual(new DateTime(2023, 12, 31), relative.CalculateDate(2024, false));
        Assert.AreEqual(new DateTime(2023, 12, 30), relative.CalculateDate(2024, true));
    }

    [TestMethod]
    public void NthAndLastDay_ApplyOffsetsAndValidateOccurrence()
    {
        var nth = new NthDayHoliday("Fourth Thursday", HolidayType.Holiday, 11, 4, (int)DayOfWeek.Thursday, 1);
        var last = new LastDayHoliday("Last Monday", HolidayType.Holiday, 5, (int)DayOfWeek.Monday, -1);
        Assert.AreEqual(new DateTime(2024, 11, 29), nth.CalculateDate(2024, false));
        Assert.AreEqual(new DateTime(2024, 5, 26), last.CalculateDate(2024, false));
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new NthDayHoliday("Invalid", HolidayType.Holiday, 1, 0, DayOfWeek.Monday));
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new NthDayHoliday("Invalid", HolidayType.Holiday, 1, 6, DayOfWeek.Monday));
    }

    [TestMethod]
    public void Easter_OffsetsWorkAcrossMonthBoundary()
    {
        Assert.AreEqual(new DateTime(2024, 3, 29), new EasterHoliday("Good Friday", HolidayType.Holiday, -2).CalculateDate(2024, false));
        Assert.AreEqual(new DateTime(2024, 4, 1), new EasterHoliday("Easter Monday", HolidayType.Holiday, 1).CalculateDate(2024, false));
    }

    [TestMethod]
    public void NextDateAndDaysLeft_UseObservedDateAndIncludeToday()
    {
        var holiday = new DateBasedHoliday("Holiday", HolidayType.Holiday, 7, 4) { ObservedRule = UnitedStatesHolidays.FederalObservedRules };
        var clock = new FakeTimeProvider(2020, 7, 1);
        Assert.AreEqual(new DateTime(2020, 7, 3), holiday.NextDate(clock));
        Assert.AreEqual(2, holiday.DaysLeft(clock));
        Assert.AreEqual(2, holiday.BusinessDaysLeft(clock));
        clock.Value = new DateTime(2020, 7, 3);
        Assert.AreEqual(0, holiday.DaysLeft(clock));
        Assert.AreEqual(0, holiday.BusinessDaysLeft(clock));
        clock.Value = new DateTime(2020, 7, 4);
        Assert.AreEqual(new DateTime(2021, 7, 5), holiday.NextDate(clock));
    }

    [TestMethod]
    public void HolidayList_ReturnsFirstMatchAndHonorsObservedRule()
    {
        var first = new DateBasedHoliday("First", HolidayType.Holiday, 7, 4) { ObservedRule = UnitedStatesHolidays.FederalObservedRules };
        var second = new DateBasedHoliday("Second", HolidayType.Private, 7, 4);
        var list = new HolidayList(new[] { first, second });
        Assert.AreSame(first, list.IsHoliday(new DateTime(2020, 7, 4, 12, 0, 0), false));
        Assert.AreSame(first, list.IsHoliday(new DateTime(2020, 7, 3), true));
        Assert.AreSame(second, list.IsHoliday(new DateTime(2020, 7, 4), true));
        Assert.IsNull(list.IsHoliday(new DateTime(2020, 7, 5), false));
    }

    [TestMethod]
    public void IsHoliday_FindsObservedDateInPreviousCalendarYear()
    {
        var holiday = new DateBasedHoliday("New Year", HolidayType.Holiday, 1, 1) { ObservedRule = UnitedStatesHolidays.FederalObservedRules };
        Assert.AreEqual(new DateTime(2021, 12, 31), holiday.CalculateDate(2022, true));
        Assert.IsTrue(holiday.IsHoliday(new DateTime(2021, 12, 31), true));
    }

    [TestMethod]
    public void NextDate_FindsObservedDateInPreviousCalendarYear()
    {
        var holiday = new DateBasedHoliday("New Year", HolidayType.Holiday, 1, 1) { ObservedRule = UnitedStatesHolidays.FederalObservedRules };
        Assert.AreEqual(new DateTime(2021, 12, 31), holiday.NextDate(new FakeTimeProvider(2021, 12, 30)));
    }

    [TestMethod]
    public void IsHoliday_FindsObservedDateInFollowingCalendarYear()
    {
        var holiday = new DateBasedHoliday("Year end", HolidayType.Holiday, 12, 31) { ObservedRule = d => d.AddDays(1) };
        Assert.IsTrue(holiday.IsHoliday(new DateTime(2025, 1, 1), true));
    }

    [TestMethod]
    public void NextDate_FindsObservedDateInFollowingCalendarYear()
    {
        var holiday = new DateBasedHoliday("Year end", HolidayType.Holiday, 12, 31) { ObservedRule = d => d.AddDays(1) };
        Assert.AreEqual(new DateTime(2025, 1, 1), holiday.NextDate(new FakeTimeProvider(2025, 1, 1)));
    }

    [TestMethod]
    public void LeapDayHoliday_DoesNotFailWhenCheckingAdjacentNonLeapYears()
    {
        var holiday = new DateBasedHoliday("Leap day", HolidayType.Holiday, 2, 29) { ObservedRule = d => d };
        Assert.IsFalse(holiday.IsHoliday(new DateTime(2024, 3, 1), true));
        Assert.AreEqual(new DateTime(2024, 2, 29), holiday.NextDate(new FakeTimeProvider(2024, 2, 1)));
    }

    [TestMethod]
    public void BetweenDays_DoesNotRequireAdjacentYearsToHaveAMatch()
    {
        var holiday = new BetweenDaysHoliday("Monday", HolidayType.Holiday, 1, 1, 1, 1, DayOfWeek.Monday);
        Assert.IsFalse(holiday.IsHoliday(new DateTime(2024, 1, 2), false));
        Assert.AreEqual(new DateTime(2024, 1, 1), holiday.NextDate(new FakeTimeProvider(2024, 1, 1)));
        holiday.ObservedRule = d => d;
        Assert.IsFalse(holiday.IsHoliday(new DateTime(2024, 1, 2), true));
        Assert.AreEqual(new DateTime(2024, 1, 1), holiday.NextDate(new FakeTimeProvider(2024, 1, 1)));
    }

    [TestMethod]
    public void RelativeToLeapDay_DoesNotRequireAdjacentYearsToHaveAnOccurrence()
    {
        var holiday = new RelativeHoliday("Leap eve", HolidayType.Holiday,
            new DateBasedHoliday("Leap day", HolidayType.Holiday, 2, 29), -1);
        Assert.IsFalse(holiday.IsHoliday(new DateTime(2024, 3, 1), false));
        Assert.AreEqual(new DateTime(2024, 2, 28), holiday.NextDate(new FakeTimeProvider(2024, 2, 1)));
    }

    [TestMethod]
    public void RelativeHoliday_IsFoundAcrossYearBoundaryWithoutAnObservedRule()
    {
        var holiday = new RelativeHoliday("Eve", HolidayType.Unofficial,
            new DateBasedHoliday("New Year", HolidayType.Holiday, 1, 1), -1);
        Assert.IsTrue(holiday.IsHoliday(new DateTime(2023, 12, 31), false));
    }

    [TestMethod]
    [DataRow("date,Example,1,3,5", 2024, 3, 5)]
    [DataRow("easter,Example,1,-2", 2024, 3, 29)]
    [DataRow("last,Example,1,5,1", 2024, 5, 27)]
    [DataRow("nth,Example,1,11,4,4,1", 2024, 11, 29)]
    [DataRow("between,Example,1,10,31,11,6,6", 2024, 11, 2)]
    public void Create_ParsesRegisteredDefinitions(string definition, int year, int month, int day)
    {
        var holiday = Holiday.Create("[" + definition + "]");
        Assert.AreEqual("Example", holiday.Name);
        Assert.AreEqual(HolidayType.Holiday, holiday.Type);
        Assert.AreEqual(new DateTime(year, month, day), holiday.CalculateDate(year, false));
    }

    [TestMethod]
    [DataRow("date")]
    [DataRow("easter")]
    [DataRow("nth")]
    [DataRow("last")]
    [DataRow("between")]
    public void TextDefinitions_CanBeReadByCreate(string kind)
    {
        Holiday original = kind switch
        {
            "date" => new DateBasedHoliday("Fixed", HolidayType.Holiday, 3, 5),
            "easter" => new EasterHoliday("Easter", HolidayType.Holiday, -2),
            "nth" => new NthDayHoliday("Nth", HolidayType.HalfDay, 11, 4, DayOfWeek.Thursday, 1),
            "last" => new LastDayHoliday("Last", HolidayType.Holiday, 5, DayOfWeek.Monday),
            _ => new BetweenDaysHoliday("Between", HolidayType.Holiday, 10, 31, 11, 6, DayOfWeek.Saturday)
        };
        var copy = Holiday.Create(original.TextDefinition());
        Assert.AreEqual(original.GetType(), copy.GetType());
        Assert.AreEqual(original.Name, copy.Name);
        Assert.AreEqual(original.Type, copy.Type);
        Assert.AreEqual(original.CalculateDate(2024, false), copy.CalculateDate(2024, false));
    }

    [TestMethod]
    public void Create_ReadsLegacyNamedEnumsAndCombinedFlags()
    {
        var holiday = Holiday.Create("[nth,Example,Holiday, HalfDay,11,4,Thursday,1]");
        Assert.AreEqual(HolidayType.Holiday | HolidayType.HalfDay, holiday.Type);
        Assert.AreEqual(new DateTime(2024, 11, 29), holiday.CalculateDate(2024, false));
        var copy = Holiday.Create(holiday.TextDefinition());
        Assert.AreEqual(holiday.Type, copy.Type);
        Assert.AreEqual(holiday.CalculateDate(2024, false), copy.CalculateDate(2024, false));
    }

    [TestMethod]
    public void Create_RejectsInvalidDefinitions()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => Holiday.Create(""));
        Assert.ThrowsExactly<InvalidOperationException>(() => Holiday.Create("date"));
        Assert.ThrowsExactly<InvalidOperationException>(() => Holiday.Create("date,Example"));
        Assert.ThrowsExactly<ArgumentException>(() => Holiday.Create("unknown,Example,1"));
    }
}
