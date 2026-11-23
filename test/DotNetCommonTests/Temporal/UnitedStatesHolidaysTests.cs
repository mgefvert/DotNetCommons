using DotNetCommons.Temporal;

namespace DotNetCommonTests.Temporal;

[TestClass]
public class UnitedStatesHolidaysTests
{
    private const string Fmt = "yyyy-MM-dd";

    [TestMethod]
    public void All_ContainsEveryNamedHolidayOnce()
    {
        Holiday[] expected = [UnitedStatesHolidays.NewYearsDay, UnitedStatesHolidays.MlkBirthday,
            UnitedStatesHolidays.PresidentsDay, UnitedStatesHolidays.Easter, UnitedStatesHolidays.MemorialDay,
            UnitedStatesHolidays.Juneteenth, UnitedStatesHolidays.IndependenceDay, UnitedStatesHolidays.LaborDay,
            UnitedStatesHolidays.VeteransDay, UnitedStatesHolidays.Thanksgiving, UnitedStatesHolidays.ChristmasDay];
        CollectionAssert.AreEquivalent(expected, UnitedStatesHolidays.All);
        Assert.AreEqual(new DateTime(2024, 6, 19), UnitedStatesHolidays.Juneteenth.CalculateDate(2024, false));
    }

    [TestMethod]
    public void FederalObservedRules_ShiftWeekendsOnly()
    {
        Assert.AreEqual(new DateTime(2021, 6, 18), UnitedStatesHolidays.FederalObservedRules(new DateTime(2021, 6, 19)));
        Assert.AreEqual(new DateTime(2022, 6, 20), UnitedStatesHolidays.FederalObservedRules(new DateTime(2022, 6, 19)));
        Assert.AreEqual(new DateTime(2024, 6, 19), UnitedStatesHolidays.FederalObservedRules(new DateTime(2024, 6, 19)));
    }

    [TestMethod]
    [DataRow("New Year", 2022, 2021, 12, 31)]
    [DataRow("Juneteenth", 2021, 2021, 6, 18)]
    [DataRow("Christmas", 2021, 2021, 12, 24)]
    public void FixedFederalHolidays_ApplyWeekendObservation(string name, int holidayYear, int year, int month, int day)
    {
        var holiday = name switch
        {
            "New Year" => UnitedStatesHolidays.NewYearsDay,
            "Juneteenth" => UnitedStatesHolidays.Juneteenth,
            _ => UnitedStatesHolidays.ChristmasDay
        };
        Assert.AreEqual(new DateTime(year, month, day), holiday.CalculateDate(holidayYear, true));
    }

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
        Assert.AreEqual("2017-01-02", UnitedStatesHolidays.NewYearsDay.CalculateDate(2017, true).ToString(Fmt));
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
