using DotNetCommons.Temporal;

namespace DotNetCommonTests.Temporal;

[TestClass]
public class HolidayTest
{
    [TestMethod]
    public void TestHoliday()
    {
        var holiday = new NthDayHoliday("Bork Day", HolidayType.Holiday, 3, 2, DayOfWeek.Monday);

        var date = holiday.NextDate();
        Assert.IsTrue(date >= DateTime.Today);
        Assert.IsTrue(date == holiday.NextDate());
    }

    [TestMethod]
    public void TestHolidays()
    {
        var holidays = new HolidayList();
        holidays.AddRange(UnitedStatesHolidays.All);

        Assert.IsNotNull(holidays.IsHoliday(new DateTime(2017, 1, 1), false));
        Assert.IsNull(holidays.IsHoliday(new DateTime(2017, 1, 3), false));
        Assert.IsNotNull(holidays.IsHoliday(new DateTime(2017, 12, 25), false));
        Assert.IsNull(holidays.IsHoliday(new DateTime(2017, 12, 30), false));
    }

    [TestMethod]
    public void TestHolidaysObserved()
    {
        var holidays = new HolidayList();
        holidays.AddRange(UnitedStatesHolidays.All);

        Assert.IsNotNull(holidays.IsHoliday(new DateTime(2020, 7, 3), true));
        Assert.IsNull(holidays.IsHoliday(new DateTime(2020, 7, 4), true));
    }

    [TestMethod]
    public void TestIsHoliday()
    {
        Assert.IsTrue(UnitedStatesHolidays.ChristmasDay.IsHoliday(new DateTime(2014, 12, 25), false));
        Assert.IsTrue(UnitedStatesHolidays.ChristmasDay.IsHoliday(new DateTime(2017, 12, 25), false));
        Assert.IsFalse(UnitedStatesHolidays.ChristmasDay.IsHoliday(new DateTime(2014, 1, 1), false));
        Assert.IsFalse(UnitedStatesHolidays.ChristmasDay.IsHoliday(new DateTime(1, 1, 2), false));
    }

    [TestMethod]
    public void TestIsObservedHoliday()
    {
        Assert.IsTrue(UnitedStatesHolidays.ChristmasDay.IsHoliday(new DateTime(2014, 12, 25), true));

        Assert.IsTrue(UnitedStatesHolidays.IndependenceDay.IsHoliday(new DateTime(2020, 7, 3), true));
        Assert.IsFalse(UnitedStatesHolidays.IndependenceDay.IsHoliday(new DateTime(2020, 7, 4), true));

        Assert.IsTrue(UnitedStatesHolidays.VeteransDay.IsHoliday(new DateTime(2020, 11, 11), true));
        Assert.IsTrue(UnitedStatesHolidays.VeteransDay.IsHoliday(new DateTime(2019, 11, 11), true));
        Assert.IsTrue(UnitedStatesHolidays.VeteransDay.IsHoliday(new DateTime(2018, 11, 12), true));
        Assert.IsTrue(UnitedStatesHolidays.VeteransDay.IsHoliday(new DateTime(2017, 11, 10), true));
        Assert.IsTrue(UnitedStatesHolidays.VeteransDay.IsHoliday(new DateTime(2016, 11, 11), true));
    }
}