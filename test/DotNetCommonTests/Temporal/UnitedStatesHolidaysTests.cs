using DotNetCommons.Temporal;

namespace DotNetCommonTests.Temporal;

[TestClass]
public class UnitedStatesHolidaysTests
{
    private const string Fmt = "yyyy-MM-dd";

    [TestMethod]
    public void TestCommon2017()
    {
        Assert.AreEqual("2017-01-01", UnitedStatesHolidays.NewYearsDay.CalculateDate(2017, false).ToString(Fmt));
        Assert.AreEqual("2017-01-16", UnitedStatesHolidays.MlkBirthday.CalculateDate(2017, false).ToString(Fmt));
        Assert.AreEqual("2017-02-20", UnitedStatesHolidays.PresidentsDay.CalculateDate(2017, false).ToString(Fmt));
        Assert.AreEqual("2017-04-16", UnitedStatesHolidays.Easter.CalculateDate(2017, false).ToString(Fmt));
        Assert.AreEqual("2017-05-29", UnitedStatesHolidays.MemorialDay.CalculateDate(2017, false).ToString(Fmt));
        Assert.AreEqual("2017-07-04", UnitedStatesHolidays.IndependenceDay.CalculateDate(2017, false).ToString(Fmt));
        Assert.AreEqual("2017-09-04", UnitedStatesHolidays.LaborDay.CalculateDate(2017, false).ToString(Fmt));
        Assert.AreEqual("2017-11-11", UnitedStatesHolidays.VeteransDay.CalculateDate(2017, false).ToString(Fmt));
        Assert.AreEqual("2017-11-23", UnitedStatesHolidays.Thanksgiving.CalculateDate(2017, false).ToString(Fmt));
        Assert.AreEqual("2017-12-25", UnitedStatesHolidays.ChristmasDay.CalculateDate(2017, false).ToString(Fmt));
    }

    [TestMethod]
    public void TestCommon2019()
    {
        Assert.AreEqual("2019-01-01", UnitedStatesHolidays.NewYearsDay.CalculateDate(2019, false).ToString(Fmt));
        Assert.AreEqual("2019-01-21", UnitedStatesHolidays.MlkBirthday.CalculateDate(2019, false).ToString(Fmt));
        Assert.AreEqual("2019-02-18", UnitedStatesHolidays.PresidentsDay.CalculateDate(2019, false).ToString(Fmt));
        Assert.AreEqual("2019-04-21", UnitedStatesHolidays.Easter.CalculateDate(2019, false).ToString(Fmt));
        Assert.AreEqual("2019-05-27", UnitedStatesHolidays.MemorialDay.CalculateDate(2019, false).ToString(Fmt));
        Assert.AreEqual("2019-07-04", UnitedStatesHolidays.IndependenceDay.CalculateDate(2019, false).ToString(Fmt));
        Assert.AreEqual("2019-09-02", UnitedStatesHolidays.LaborDay.CalculateDate(2019, false).ToString(Fmt));
        Assert.AreEqual("2019-11-11", UnitedStatesHolidays.VeteransDay.CalculateDate(2019, false).ToString(Fmt));
        Assert.AreEqual("2019-11-28", UnitedStatesHolidays.Thanksgiving.CalculateDate(2019, false).ToString(Fmt));
        Assert.AreEqual("2019-12-25", UnitedStatesHolidays.ChristmasDay.CalculateDate(2019, false).ToString(Fmt));
    }

    [TestMethod]
    public void TestObserved2017()
    {
        Assert.AreEqual("2017-01-01", UnitedStatesHolidays.NewYearsDay.CalculateDate(2017, true).ToString(Fmt));
        Assert.AreEqual("2017-01-16", UnitedStatesHolidays.MlkBirthday.CalculateDate(2017, true).ToString(Fmt));
        Assert.AreEqual("2017-02-20", UnitedStatesHolidays.PresidentsDay.CalculateDate(2017, true).ToString(Fmt));
        Assert.AreEqual("2017-04-16", UnitedStatesHolidays.Easter.CalculateDate(2017, true).ToString(Fmt));
        Assert.AreEqual("2017-05-29", UnitedStatesHolidays.MemorialDay.CalculateDate(2017, true).ToString(Fmt));
        Assert.AreEqual("2017-07-04", UnitedStatesHolidays.IndependenceDay.CalculateDate(2017, true).ToString(Fmt));
        Assert.AreEqual("2017-09-04", UnitedStatesHolidays.LaborDay.CalculateDate(2017, true).ToString(Fmt));
        Assert.AreEqual("2017-11-10", UnitedStatesHolidays.VeteransDay.CalculateDate(2017, true).ToString(Fmt));
        Assert.AreEqual("2017-11-23", UnitedStatesHolidays.Thanksgiving.CalculateDate(2017, true).ToString(Fmt));
        Assert.AreEqual("2017-12-25", UnitedStatesHolidays.ChristmasDay.CalculateDate(2017, true).ToString(Fmt));
    }

    [TestMethod]
    public void TestObserved2019()
    {
        Assert.AreEqual("2019-01-01", UnitedStatesHolidays.NewYearsDay.CalculateDate(2019, true).ToString(Fmt));
        Assert.AreEqual("2019-01-21", UnitedStatesHolidays.MlkBirthday.CalculateDate(2019, true).ToString(Fmt));
        Assert.AreEqual("2019-02-18", UnitedStatesHolidays.PresidentsDay.CalculateDate(2019, true).ToString(Fmt));
        Assert.AreEqual("2019-04-21", UnitedStatesHolidays.Easter.CalculateDate(2019, true).ToString(Fmt));
        Assert.AreEqual("2019-05-27", UnitedStatesHolidays.MemorialDay.CalculateDate(2019, true).ToString(Fmt));
        Assert.AreEqual("2019-07-04", UnitedStatesHolidays.IndependenceDay.CalculateDate(2019, true).ToString(Fmt));
        Assert.AreEqual("2019-09-02", UnitedStatesHolidays.LaborDay.CalculateDate(2019, true).ToString(Fmt));
        Assert.AreEqual("2019-11-11", UnitedStatesHolidays.VeteransDay.CalculateDate(2019, true).ToString(Fmt));
        Assert.AreEqual("2019-11-28", UnitedStatesHolidays.Thanksgiving.CalculateDate(2019, true).ToString(Fmt));
        Assert.AreEqual("2019-12-25", UnitedStatesHolidays.ChristmasDay.CalculateDate(2019, true).ToString(Fmt));
    }
}