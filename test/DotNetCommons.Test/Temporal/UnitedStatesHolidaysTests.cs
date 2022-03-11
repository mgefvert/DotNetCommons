using DotNetCommons.Temporal;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotNetCommons.Test.Temporal
{
    [TestClass]
    public class UnitedStatesHolidaysTests
    {
        private UnitedStatesHolidays _holidays;
        private const string Fmt = "yyyy-MM-dd";

        [TestInitialize]
        public void Setup()
        {
            _holidays = new UnitedStatesHolidays();
        }

        [TestMethod]
        public void TestCommon2017()
        {
            Assert.AreEqual("2017-01-01", _holidays.NewYearsDay.CalculateDate(2017, false).ToString(Fmt));
            Assert.AreEqual("2017-01-16", _holidays.MlkBirthday.CalculateDate(2017, false).ToString(Fmt));
            Assert.AreEqual("2017-02-20", _holidays.PresidentsDay.CalculateDate(2017, false).ToString(Fmt));
            Assert.AreEqual("2017-04-16", _holidays.Easter.CalculateDate(2017, false).ToString(Fmt));
            Assert.AreEqual("2017-05-29", _holidays.MemorialDay.CalculateDate(2017, false).ToString(Fmt));
            Assert.AreEqual("2017-07-04", _holidays.IndependenceDay.CalculateDate(2017, false).ToString(Fmt));
            Assert.AreEqual("2017-09-04", _holidays.LaborDay.CalculateDate(2017, false).ToString(Fmt));
            Assert.AreEqual("2017-10-09", _holidays.ColumbusDay.CalculateDate(2017, false).ToString(Fmt));
            Assert.AreEqual("2017-11-11", _holidays.VeteransDay.CalculateDate(2017, false).ToString(Fmt));
            Assert.AreEqual("2017-11-23", _holidays.Thanksgiving.CalculateDate(2017, false).ToString(Fmt));
            Assert.AreEqual("2017-12-24", _holidays.ChristmasEve.CalculateDate(2017, false).ToString(Fmt));
            Assert.AreEqual("2017-12-25", _holidays.ChristmasDay.CalculateDate(2017, false).ToString(Fmt));
        }

        [TestMethod]
        public void TestCommon2019()
        {
            Assert.AreEqual("2019-01-01", _holidays.NewYearsDay.CalculateDate(2019, false).ToString(Fmt));
            Assert.AreEqual("2019-01-21", _holidays.MlkBirthday.CalculateDate(2019, false).ToString(Fmt));
            Assert.AreEqual("2019-02-18", _holidays.PresidentsDay.CalculateDate(2019, false).ToString(Fmt));
            Assert.AreEqual("2019-04-21", _holidays.Easter.CalculateDate(2019, false).ToString(Fmt));
            Assert.AreEqual("2019-05-27", _holidays.MemorialDay.CalculateDate(2019, false).ToString(Fmt));
            Assert.AreEqual("2019-07-04", _holidays.IndependenceDay.CalculateDate(2019, false).ToString(Fmt));
            Assert.AreEqual("2019-09-02", _holidays.LaborDay.CalculateDate(2019, false).ToString(Fmt));
            Assert.AreEqual("2019-10-14", _holidays.ColumbusDay.CalculateDate(2019, false).ToString(Fmt));
            Assert.AreEqual("2019-11-11", _holidays.VeteransDay.CalculateDate(2019, false).ToString(Fmt));
            Assert.AreEqual("2019-11-28", _holidays.Thanksgiving.CalculateDate(2019, false).ToString(Fmt));
            Assert.AreEqual("2019-12-24", _holidays.ChristmasEve.CalculateDate(2019, false).ToString(Fmt));
            Assert.AreEqual("2019-12-25", _holidays.ChristmasDay.CalculateDate(2019, false).ToString(Fmt));
        }

        [TestMethod]
        public void TestObserved2017()
        {
            Assert.AreEqual("2017-01-01", _holidays.NewYearsDay.CalculateDate(2017, true).ToString(Fmt));
            Assert.AreEqual("2017-01-16", _holidays.MlkBirthday.CalculateDate(2017, true).ToString(Fmt));
            Assert.AreEqual("2017-02-20", _holidays.PresidentsDay.CalculateDate(2017, true).ToString(Fmt));
            Assert.AreEqual("2017-04-16", _holidays.Easter.CalculateDate(2017, true).ToString(Fmt));
            Assert.AreEqual("2017-05-29", _holidays.MemorialDay.CalculateDate(2017, true).ToString(Fmt));
            Assert.AreEqual("2017-07-04", _holidays.IndependenceDay.CalculateDate(2017, true).ToString(Fmt));
            Assert.AreEqual("2017-09-04", _holidays.LaborDay.CalculateDate(2017, true).ToString(Fmt));
            Assert.AreEqual("2017-10-09", _holidays.ColumbusDay.CalculateDate(2017, true).ToString(Fmt));
            Assert.AreEqual("2017-11-10", _holidays.VeteransDay.CalculateDate(2017, true).ToString(Fmt));
            Assert.AreEqual("2017-11-23", _holidays.Thanksgiving.CalculateDate(2017, true).ToString(Fmt));
            Assert.AreEqual("2017-12-24", _holidays.ChristmasEve.CalculateDate(2017, true).ToString(Fmt));
            Assert.AreEqual("2017-12-25", _holidays.ChristmasDay.CalculateDate(2017, true).ToString(Fmt));
        }

        [TestMethod]
        public void TestObserved2019()
        {
            Assert.AreEqual("2019-01-01", _holidays.NewYearsDay.CalculateDate(2019, true).ToString(Fmt));
            Assert.AreEqual("2019-01-21", _holidays.MlkBirthday.CalculateDate(2019, true).ToString(Fmt));
            Assert.AreEqual("2019-02-18", _holidays.PresidentsDay.CalculateDate(2019, true).ToString(Fmt));
            Assert.AreEqual("2019-04-21", _holidays.Easter.CalculateDate(2019, true).ToString(Fmt));
            Assert.AreEqual("2019-05-27", _holidays.MemorialDay.CalculateDate(2019, true).ToString(Fmt));
            Assert.AreEqual("2019-07-04", _holidays.IndependenceDay.CalculateDate(2019, true).ToString(Fmt));
            Assert.AreEqual("2019-09-02", _holidays.LaborDay.CalculateDate(2019, true).ToString(Fmt));
            Assert.AreEqual("2019-10-14", _holidays.ColumbusDay.CalculateDate(2019, true).ToString(Fmt));
            Assert.AreEqual("2019-11-11", _holidays.VeteransDay.CalculateDate(2019, true).ToString(Fmt));
            Assert.AreEqual("2019-11-28", _holidays.Thanksgiving.CalculateDate(2019, true).ToString(Fmt));
            Assert.AreEqual("2019-12-24", _holidays.ChristmasEve.CalculateDate(2019, true).ToString(Fmt));
            Assert.AreEqual("2019-12-25", _holidays.ChristmasDay.CalculateDate(2019, true).ToString(Fmt));
        }
    }
}
