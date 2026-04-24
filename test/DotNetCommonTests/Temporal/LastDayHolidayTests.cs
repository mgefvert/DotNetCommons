using DotNetCommons.Temporal;

namespace DotNetCommonTests.Temporal;

[TestClass]
public class LastDayHolidayTests
{
    [TestMethod]
    public void Test()
    {
        var holiday = new LastDayHoliday("Bork Day", HolidayType.HalfDay, 12, DayOfWeek.Wednesday);

        Assert.AreEqual("Bork Day", holiday.Name);
        Assert.AreEqual(HolidayType.HalfDay, holiday.Type);

        Assert.AreEqual(new DateTime(2020, 12, 30), holiday.InternalCalculateDate(2020));
        Assert.AreEqual(new DateTime(2021, 12, 29), holiday.InternalCalculateDate(2021));
        Assert.AreEqual(new DateTime(2022, 12, 28), holiday.InternalCalculateDate(2022));
        Assert.AreEqual(new DateTime(2023, 12, 27), holiday.InternalCalculateDate(2023));
    }
}