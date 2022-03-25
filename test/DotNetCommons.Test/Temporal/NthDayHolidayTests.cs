using System;
using DotNetCommons.Temporal;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotNetCommons.Test.Temporal;

[TestClass]
public class NthDayHolidayTests
{
    [TestMethod]
    public void Test()
    {
        var holiday = new NthDayHoliday("Bork Day", HolidayType.Halfday, 12, 2, DayOfWeek.Wednesday);

        Assert.AreEqual("Bork Day", holiday.Name);
        Assert.AreEqual(HolidayType.Halfday, holiday.Type);
    }

    [TestMethod]
    public void TestFirst()
    {
        var holiday = new NthDayHoliday("Bork Day", HolidayType.Halfday, 12, 1, DayOfWeek.Wednesday);

        Assert.AreEqual(new DateTime(2020, 12, 2), holiday.InternalCalculateDate(2020));
        Assert.AreEqual(new DateTime(2021, 12, 1), holiday.InternalCalculateDate(2021));
        Assert.AreEqual(new DateTime(2022, 12, 7), holiday.InternalCalculateDate(2022));
        Assert.AreEqual(new DateTime(2023, 12, 6), holiday.InternalCalculateDate(2023));
    }

    [TestMethod]
    public void TestSecond()
    {
        var holiday = new NthDayHoliday("Bork Day", HolidayType.Halfday, 12, 2, DayOfWeek.Wednesday);

        Assert.AreEqual(new DateTime(2020, 12, 9), holiday.InternalCalculateDate(2020));
        Assert.AreEqual(new DateTime(2021, 12, 8), holiday.InternalCalculateDate(2021));
        Assert.AreEqual(new DateTime(2022, 12, 14), holiday.InternalCalculateDate(2022));
        Assert.AreEqual(new DateTime(2023, 12, 13), holiday.InternalCalculateDate(2023));
    }
}