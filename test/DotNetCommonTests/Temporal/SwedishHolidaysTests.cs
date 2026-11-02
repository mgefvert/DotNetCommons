using DotNetCommons.Temporal;

namespace DotNetCommonTests.Temporal;

[TestClass]
public class SwedishHolidaysTests
{
    [TestMethod]
    public void All_ContainsEachNamedHolidayWithExpected2024DateAndType()
    {
        var cases = new (Holiday Holiday, DateTime Date, HolidayType Type)[]
        {
            (SwedishHolidays.NewYearsDay, new(2024, 1, 1), HolidayType.Holiday),
            (SwedishHolidays.Epiphany, new(2024, 1, 6), HolidayType.Holiday),
            (SwedishHolidays.GoodFriday, new(2024, 3, 29), HolidayType.Holiday),
            (SwedishHolidays.EasterEve, new(2024, 3, 30), HolidayType.Holiday),
            (SwedishHolidays.Easter, new(2024, 3, 31), HolidayType.Holiday),
            (SwedishHolidays.EasterMonday, new(2024, 4, 1), HolidayType.Holiday),
            (SwedishHolidays.MayFirst, new(2024, 5, 1), HolidayType.Holiday),
            (SwedishHolidays.AscensionDay, new(2024, 5, 9), HolidayType.Holiday),
            (SwedishHolidays.Pentecost, new(2024, 5, 19), HolidayType.Holiday),
            (SwedishHolidays.NationalDayOfSweden, new(2024, 6, 6), HolidayType.Holiday),
            (SwedishHolidays.MidsummerEve, new(2024, 6, 21), HolidayType.Unofficial),
            (SwedishHolidays.Midsummer, new(2024, 6, 22), HolidayType.Holiday),
            (SwedishHolidays.AllSaintsDay, new(2024, 11, 2), HolidayType.Holiday),
            (SwedishHolidays.ChristmasEve, new(2024, 12, 24), HolidayType.Unofficial),
            (SwedishHolidays.ChristmasDay, new(2024, 12, 25), HolidayType.Holiday),
            (SwedishHolidays.BoxingDay, new(2024, 12, 26), HolidayType.Holiday),
            (SwedishHolidays.NewYearsEve, new(2024, 12, 31), HolidayType.Unofficial)
        };
        Assert.AreEqual(cases.Length, SwedishHolidays.All.Length);
        foreach (var (holiday, date, type) in cases)
        {
            Assert.AreEqual(1, SwedishHolidays.All.Count(h => ReferenceEquals(h, holiday)), holiday.Name);
            Assert.AreEqual(date, holiday.CalculateDate(2024, false), holiday.Name);
            Assert.AreEqual(type, holiday.Type, holiday.Name);
        }
    }

    [TestMethod]
    public void MovableHolidays_ChangeCorrectlyAcrossYears()
    {
        Assert.AreEqual(new DateTime(2023, 6, 24), SwedishHolidays.Midsummer.CalculateDate(2023, false));
        Assert.AreEqual(new DateTime(2025, 6, 20), SwedishHolidays.MidsummerEve.CalculateDate(2025, false));
        Assert.AreEqual(new DateTime(2020, 10, 31), SwedishHolidays.AllSaintsDay.CalculateDate(2020, false));
        Assert.AreEqual(new DateTime(2021, 11, 6), SwedishHolidays.AllSaintsDay.CalculateDate(2021, false));
    }
}
