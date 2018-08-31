using System;
using DotNetCommons.Temporal;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotNetCommons.Test.Temporal
{
    [TestClass]
    public class HolidayTest
    {
        [TestMethod]
        public void TestHoliday()
        {
            // One day after the second monday in March
            var holiday = Holiday.CreateDayInNthWeek(3, 2, DayOfWeek.Monday, 1, "Bork Day");

            var date = holiday.NextDate;
            Assert.IsTrue(date >= DateTime.Today);
            Assert.IsTrue(date == holiday.NextDate);
            Assert.IsTrue(date == holiday.NextDate);

            Assert.AreEqual(HolidayType.NthWeek, holiday.HolidayType);
            Assert.AreEqual(1, holiday.CalcAddDays);
            Assert.AreEqual(0, holiday.CalcDay);
            Assert.AreEqual(DayOfWeek.Monday, holiday.CalcDayOfWeek);
            Assert.AreEqual(3, holiday.CalcMonth);
            Assert.AreEqual(2, holiday.CalcWeek);
            Assert.AreEqual("Bork Day", holiday.Name);
            Assert.AreEqual("[NthWeek,Bork Day,3,2,0,1,1]", holiday.GetDefinition());
            Assert.IsTrue(holiday.ToString().StartsWith("Bork Day: "));

            var definition = holiday.GetDefinition();

            var newholiday = new Holiday(definition);
            Assert.AreEqual(holiday.ToString(), newholiday.ToString());
            Assert.AreEqual(holiday.GetDefinition(), newholiday.GetDefinition());
            Assert.AreEqual(holiday.NextDate, newholiday.NextDate);
        }

        [TestMethod]
        public void TestHolidays()
        {
            Assert.IsNotNull(Holidays.IsHoliday(new DateTime(2017, 1, 1)));
            Assert.IsNull(Holidays.IsHoliday(new DateTime(2017, 1, 3)));
            Assert.IsNotNull(Holidays.IsHoliday(new DateTime(2017, 12, 25)));
            Assert.IsNull(Holidays.IsHoliday(new DateTime(2017, 12, 30)));
        }

        [TestMethod]
        public void TestCommon()
        {
            // Holidays for 2017
            Assert.AreEqual("2017-01-01", CommonHolidays.NewYearsDay.CalculateDate(2017).ToShortDateString());
            Assert.AreEqual("2017-01-16", CommonHolidays.MlkBirthday.CalculateDate(2017).ToShortDateString());
            Assert.AreEqual("2017-02-20", CommonHolidays.PresidentsDay.CalculateDate(2017).ToShortDateString());
            Assert.AreEqual("2017-04-16", CommonHolidays.Easter.CalculateDate(2017).ToShortDateString());
            Assert.AreEqual("2017-05-29", CommonHolidays.MemorialDay.CalculateDate(2017).ToShortDateString());
            Assert.AreEqual("2017-07-04", CommonHolidays.IndependenceDay.CalculateDate(2017).ToShortDateString());
            Assert.AreEqual("2017-09-04", CommonHolidays.LaborDay.CalculateDate(2017).ToShortDateString());
            Assert.AreEqual("2017-10-09", CommonHolidays.ColumbusDay.CalculateDate(2017).ToShortDateString());
            Assert.AreEqual("2017-11-11", CommonHolidays.VeteransDay.CalculateDate(2017).ToShortDateString());
            Assert.AreEqual("2017-11-23", CommonHolidays.Thanksgiving.CalculateDate(2017).ToShortDateString());
            Assert.AreEqual("2017-12-24", CommonHolidays.ChristmasEve.CalculateDate(2017).ToShortDateString());
            Assert.AreEqual("2017-12-25", CommonHolidays.ChristmasDay.CalculateDate(2017).ToShortDateString());
            Assert.AreEqual("2017-12-26", CommonHolidays.BoxingDay.CalculateDate(2017).ToShortDateString());
            Assert.AreEqual("2017-12-31", CommonHolidays.NewYearsEve.CalculateDate(2017).ToShortDateString());

            // Holidays for 2019
            Assert.AreEqual("2019-01-01", CommonHolidays.NewYearsDay.CalculateDate(2019).ToShortDateString());
            Assert.AreEqual("2019-01-21", CommonHolidays.MlkBirthday.CalculateDate(2019).ToShortDateString());
            Assert.AreEqual("2019-02-18", CommonHolidays.PresidentsDay.CalculateDate(2019).ToShortDateString());
            Assert.AreEqual("2019-04-21", CommonHolidays.Easter.CalculateDate(2019).ToShortDateString());
            Assert.AreEqual("2019-05-27", CommonHolidays.MemorialDay.CalculateDate(2019).ToShortDateString());
            Assert.AreEqual("2019-07-04", CommonHolidays.IndependenceDay.CalculateDate(2019).ToShortDateString());
            Assert.AreEqual("2019-09-02", CommonHolidays.LaborDay.CalculateDate(2019).ToShortDateString());
            Assert.AreEqual("2019-10-14", CommonHolidays.ColumbusDay.CalculateDate(2019).ToShortDateString());
            Assert.AreEqual("2019-11-11", CommonHolidays.VeteransDay.CalculateDate(2019).ToShortDateString());
            Assert.AreEqual("2019-11-28", CommonHolidays.Thanksgiving.CalculateDate(2019).ToShortDateString());
            Assert.AreEqual("2019-12-24", CommonHolidays.ChristmasEve.CalculateDate(2019).ToShortDateString());
            Assert.AreEqual("2019-12-25", CommonHolidays.ChristmasDay.CalculateDate(2019).ToShortDateString());
            Assert.AreEqual("2019-12-26", CommonHolidays.BoxingDay.CalculateDate(2019).ToShortDateString());
            Assert.AreEqual("2019-12-31", CommonHolidays.NewYearsEve.CalculateDate(2019).ToShortDateString());
        }

        [TestMethod]
        public void TestEaster()
        {
            Assert.AreEqual("1995-04-16", Holiday.GetEasterSundayDate(1995).ToString("yyyy-MM-dd"));
            Assert.AreEqual("1996-04-07", Holiday.GetEasterSundayDate(1996).ToString("yyyy-MM-dd"));
            Assert.AreEqual("1997-03-30", Holiday.GetEasterSundayDate(1997).ToString("yyyy-MM-dd"));
            Assert.AreEqual("2006-04-16", Holiday.GetEasterSundayDate(2006).ToString("yyyy-MM-dd"));
            Assert.AreEqual("2007-04-08", Holiday.GetEasterSundayDate(2007).ToString("yyyy-MM-dd"));
            Assert.AreEqual("2008-03-23", Holiday.GetEasterSundayDate(2008).ToString("yyyy-MM-dd"));
            Assert.AreEqual("2015-04-05", Holiday.GetEasterSundayDate(2015).ToString("yyyy-MM-dd"));
            Assert.AreEqual("2016-03-27", Holiday.GetEasterSundayDate(2016).ToString("yyyy-MM-dd"));
            Assert.AreEqual("2017-04-16", Holiday.GetEasterSundayDate(2017).ToString("yyyy-MM-dd"));
            Assert.AreEqual("2031-04-13", Holiday.GetEasterSundayDate(2031).ToString("yyyy-MM-dd"));
            Assert.AreEqual("2032-03-28", Holiday.GetEasterSundayDate(2032).ToString("yyyy-MM-dd"));
        }

        [TestMethod]
        public void TestIsHoliday()
        {
            Assert.IsTrue(CommonHolidays.ChristmasDay.IsHoliday(new DateTime(2014, 12, 25)));
            Assert.IsTrue(CommonHolidays.ChristmasDay.IsHoliday(new DateTime(2017, 12, 25)));
            Assert.IsFalse(CommonHolidays.ChristmasDay.IsHoliday(new DateTime(2014, 1, 1)));
            Assert.IsFalse(CommonHolidays.ChristmasDay.IsHoliday(new DateTime(1, 1, 2)));
        }

        [TestMethod]
        public void TestLastWeek()
        {
            Assert.AreEqual("2017-12-29", Holiday.GetDayInLastWeekOfMonth(2017, 12, DayOfWeek.Friday).ToString("yyyy-MM-dd"));
            Assert.AreEqual("2017-04-30", Holiday.GetDayInLastWeekOfMonth(2017, 4, DayOfWeek.Sunday).ToString("yyyy-MM-dd"));
        }

        [TestMethod]
        public void TestNthWeek()
        {
            Assert.AreEqual("2017-12-06", Holiday.GetDayInNthWeekOfMonth(2017, 12, 1, DayOfWeek.Wednesday).ToString("yyyy-MM-dd"));
            Assert.AreEqual("2017-12-13", Holiday.GetDayInNthWeekOfMonth(2017, 12, 2, DayOfWeek.Wednesday).ToString("yyyy-MM-dd"));
            Assert.AreEqual("2017-12-20", Holiday.GetDayInNthWeekOfMonth(2017, 12, 3, DayOfWeek.Wednesday).ToString("yyyy-MM-dd"));
            Assert.AreEqual("2017-12-27", Holiday.GetDayInNthWeekOfMonth(2017, 12, 4, DayOfWeek.Wednesday).ToString("yyyy-MM-dd"));
        }
    }
}
