using DotNetCommons.Temporal;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotNetCommonTests.Temporal;

[TestClass]
public class HolidayTest
{
    [TestMethod]
    public void TestHoliday()
    {
        var holiday = new NthDayHoliday("Bork Day", HolidayType.Holiday, 3, 2, DayOfWeek.Monday);

        var date = holiday.NextDate;
        Assert.IsTrue(date >= DateTime.Today);
        Assert.IsTrue(date == holiday.NextDate);
    }

    public void TestHolidayCreateFromDefinition()
    {
        var holiday = new NthDayHoliday("Bork Day", HolidayType.Holiday, 3, 2, DayOfWeek.Monday);
        var definition = holiday.TextDefinition();

        var newholiday = Holiday.Create(definition);
        Assert.AreEqual(holiday.ToString(), newholiday.ToString());
        Assert.AreEqual(definition, newholiday.TextDefinition());
        Assert.AreEqual(holiday.NextDate, newholiday.NextDate);
    }

    [TestMethod]
    public void TestHolidays()
    {
        var holidays = new HolidayList();
        holidays.AddRange(new UnitedStatesHolidays().All);

        Assert.IsNotNull(holidays.IsHoliday(new DateTime(2017, 1, 1)));
        Assert.IsNull(holidays.IsHoliday(new DateTime(2017, 1, 3)));
        Assert.IsNotNull(holidays.IsHoliday(new DateTime(2017, 12, 25)));
        Assert.IsNull(holidays.IsHoliday(new DateTime(2017, 12, 30)));
    }

    [TestMethod]
    public void TestHolidaysObserved()
    {
        var holidays = new HolidayList();
        holidays.AddRange(new UnitedStatesHolidays().All);

        Assert.IsNotNull(holidays.IsObservedHoliday(new DateTime(2020, 7, 3)));
        Assert.IsNull(holidays.IsObservedHoliday(new DateTime(2020, 7, 4)));
    }

    [TestMethod]
    public void TestIsHoliday()
    {
        var holidays = new UnitedStatesHolidays();

        Assert.IsTrue(holidays.ChristmasDay.IsHoliday(new DateTime(2014, 12, 25)));
        Assert.IsTrue(holidays.ChristmasDay.IsHoliday(new DateTime(2017, 12, 25)));
        Assert.IsFalse(holidays.ChristmasDay.IsHoliday(new DateTime(2014, 1, 1)));
        Assert.IsFalse(holidays.ChristmasDay.IsHoliday(new DateTime(1, 1, 2)));
    }

    [TestMethod]
    public void TestIsObservedHoliday()
    {
        var holidays = new UnitedStatesHolidays();

        Assert.IsTrue(holidays.ChristmasDay.IsObservedHoliday(new DateTime(2014, 12, 25)));

        Assert.IsTrue(holidays.IndependenceDay.IsObservedHoliday(new DateTime(2020, 7, 3)));
        Assert.IsFalse(holidays.IndependenceDay.IsObservedHoliday(new DateTime(2020, 7, 4)));

        Assert.IsTrue(holidays.VeteransDay.IsObservedHoliday(new DateTime(2020, 11, 11)));
        Assert.IsTrue(holidays.VeteransDay.IsObservedHoliday(new DateTime(2019, 11, 11)));
        Assert.IsTrue(holidays.VeteransDay.IsObservedHoliday(new DateTime(2018, 11, 12)));
        Assert.IsTrue(holidays.VeteransDay.IsObservedHoliday(new DateTime(2017, 11, 10)));
        Assert.IsTrue(holidays.VeteransDay.IsObservedHoliday(new DateTime(2016, 11, 11)));
    }
}