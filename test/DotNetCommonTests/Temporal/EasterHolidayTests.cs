using DotNetCommons.Temporal;

namespace DotNetCommonTests.Temporal;

[TestClass]
public class EasterHolidayTests
{
    [TestMethod]
    public void Test()
    {
        var holiday = new EasterHoliday("Easter", HolidayType.Holiday);

        Assert.AreEqual("Easter", holiday.Name);
        Assert.AreEqual(HolidayType.Holiday, holiday.Type);

        Assert.AreEqual(new DateTime(1995, 4, 16), holiday.InternalCalculateDate(1995));
        Assert.AreEqual(new DateTime(1996, 4, 7), holiday.InternalCalculateDate(1996));
        Assert.AreEqual(new DateTime(1997, 3, 30), holiday.InternalCalculateDate(1997));
        Assert.AreEqual(new DateTime(2006, 4, 16), holiday.InternalCalculateDate(2006));
        Assert.AreEqual(new DateTime(2007, 4, 8), holiday.InternalCalculateDate(2007));
        Assert.AreEqual(new DateTime(2008, 3, 23), holiday.InternalCalculateDate(2008));
        Assert.AreEqual(new DateTime(2015, 4, 5), holiday.InternalCalculateDate(2015));
        Assert.AreEqual(new DateTime(2016, 3, 27), holiday.InternalCalculateDate(2016));
        Assert.AreEqual(new DateTime(2017, 4, 16), holiday.InternalCalculateDate(2017));
        Assert.AreEqual(new DateTime(2031, 4, 13), holiday.InternalCalculateDate(2031));
        Assert.AreEqual(new DateTime(2032, 3, 28), holiday.InternalCalculateDate(2032));
    }
}