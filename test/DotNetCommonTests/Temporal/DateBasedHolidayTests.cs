using DotNetCommons.Temporal;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotNetCommonTests.Temporal;

[TestClass]
public class DateBasedHolidayTests
{
    [TestMethod]
    public void Test()
    {
        var holiday = new DateBasedHoliday("Bork Day", HolidayType.HalfDay, 3, 5);

        Assert.AreEqual("Bork Day", holiday.Name);
        Assert.AreEqual(HolidayType.HalfDay, holiday.Type);

        Assert.AreEqual(new DateTime(2020, 3, 5), holiday.InternalCalculateDate(2020));
        Assert.AreEqual(new DateTime(2021, 3, 5), holiday.InternalCalculateDate(2021));
        Assert.AreEqual(new DateTime(2022, 3, 5), holiday.InternalCalculateDate(2022));
        Assert.AreEqual(new DateTime(2023, 3, 5), holiday.InternalCalculateDate(2023));
    }
}