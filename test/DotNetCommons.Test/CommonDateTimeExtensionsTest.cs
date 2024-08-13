using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;

namespace DotNetCommons.Test;

[TestClass]
public class CommonDateTimeExtensionsTest
{
    [TestMethod]
    public void Age_Works()
    {
        var birthDate = new DateTime(1974, 12, 9);

        birthDate.Age(new DateTime(1974, 12, 9)).Should().Be((0, 0, 0));
        birthDate.Age(new DateTime(1974, 12, 31)).Should().Be((0, 0, 22));
        birthDate.Age(new DateTime(1975, 1, 9)).Should().Be((0, 1, 0));
        birthDate.Age(new DateTime(1975, 2, 9)).Should().Be((0, 2, 0));
        birthDate.Age(new DateTime(1975, 12, 8)).Should().Be((0, 11, 29));

        birthDate.Age(new DateTime(2022, 12, 8)).Should().Be((47, 11, 29));
        birthDate.Age(new DateTime(2022, 12, 9)).Should().Be((48, 0, 0));
    }

    [TestMethod]
    public void AgeYears_Works()
    {
        var birthDate = new DateTime(1974, 12, 9);

        birthDate.AgeYears(new DateTime(1974, 1, 1)).Should().Be(0);
        birthDate.AgeYears(new DateTime(1974, 12, 1)).Should().Be(0);
        birthDate.AgeYears(new DateTime(1974, 12, 9)).Should().Be(0);
        birthDate.AgeYears(new DateTime(1975, 1, 1)).Should().Be(0);
        birthDate.AgeYears(new DateTime(1975, 12, 8)).Should().Be(0);

        birthDate.AgeYears(new DateTime(1975, 12, 9)).Should().Be(1);
        birthDate.AgeYears(new DateTime(1976, 12, 8)).Should().Be(1);

        // Note: 1976 is a leap year
        birthDate.AgeYears(new DateTime(1976, 12, 9)).Should().Be(2);

        birthDate.AgeYears(new DateTime(2022, 12, 1)).Should().Be(47);
        birthDate.AgeYears(new DateTime(2022, 12, 9)).Should().Be(48);
    }

    [TestMethod]
    public void AgeMonths_Works()
    {
        var birthDate = new DateTime(1974, 12, 9);

        birthDate.AgeMonths(new DateTime(1974, 1, 1)).Should().Be(0);
        birthDate.AgeMonths(new DateTime(1974, 12, 1)).Should().Be(0);
        birthDate.AgeMonths(new DateTime(1974, 12, 9)).Should().Be(0);
        birthDate.AgeMonths(new DateTime(1974, 12, 31)).Should().Be(0);
        birthDate.AgeMonths(new DateTime(1975, 1, 1)).Should().Be(0);
        birthDate.AgeMonths(new DateTime(1975, 1, 8)).Should().Be(0);

        birthDate.AgeMonths(new DateTime(1975, 1, 9)).Should().Be(1);
        birthDate.AgeMonths(new DateTime(1975, 2, 8)).Should().Be(1);

        birthDate.AgeMonths(new DateTime(1975, 2, 9)).Should().Be(2);
        birthDate.AgeMonths(new DateTime(1975, 3, 8)).Should().Be(2);

        birthDate.AgeMonths(new DateTime(1975, 3, 9)).Should().Be(3);
        birthDate.AgeMonths(new DateTime(1975, 4, 8)).Should().Be(3);

        birthDate.AgeMonths(new DateTime(1975, 4, 9)).Should().Be(4);
        birthDate.AgeMonths(new DateTime(1975, 5, 8)).Should().Be(4);

        birthDate.AgeMonths(new DateTime(1975, 5, 9)).Should().Be(5);
        birthDate.AgeMonths(new DateTime(1975, 6, 8)).Should().Be(5);

        birthDate.AgeMonths(new DateTime(1975, 6, 9)).Should().Be(6);
        birthDate.AgeMonths(new DateTime(1975, 7, 8)).Should().Be(6);

        birthDate.AgeMonths(new DateTime(1975, 7, 9)).Should().Be(7);
        birthDate.AgeMonths(new DateTime(1975, 8, 8)).Should().Be(7);

        birthDate.AgeMonths(new DateTime(1975, 8, 9)).Should().Be(8);
        birthDate.AgeMonths(new DateTime(1975, 9, 8)).Should().Be(8);

        birthDate.AgeMonths(new DateTime(1975, 9, 9)).Should().Be(9);
        birthDate.AgeMonths(new DateTime(1975, 10, 8)).Should().Be(9);

        birthDate.AgeMonths(new DateTime(1975, 10, 9)).Should().Be(10);
        birthDate.AgeMonths(new DateTime(1975, 11, 8)).Should().Be(10);

        birthDate.AgeMonths(new DateTime(1975, 11, 9)).Should().Be(11);
        birthDate.AgeMonths(new DateTime(1975, 12, 8)).Should().Be(11);

        birthDate.AgeMonths(new DateTime(1975, 12, 9)).Should().Be(12);
        birthDate.AgeMonths(new DateTime(1976, 1, 8)).Should().Be(12);

        // Note: 1976 is a leap year
        birthDate.AgeMonths(new DateTime(1976, 12, 9)).Should().Be(24);

        birthDate.AgeMonths(new DateTime(2022, 12, 1)).Should().Be(575);
        birthDate.AgeMonths(new DateTime(2022, 12, 9)).Should().Be(576);
    }

    [TestMethod]
    public void AgeDays_Works()
    {
        var birthDate = new DateTime(1974, 12, 9);

        birthDate.AgeDays(new DateTime(1974, 1, 1)).Should().Be(0);
        birthDate.AgeDays(new DateTime(1974, 12, 1)).Should().Be(0);
        birthDate.AgeDays(new DateTime(1974, 12, 8)).Should().Be(0);
        birthDate.AgeDays(new DateTime(1974, 12, 9)).Should().Be(0);
        birthDate.AgeDays(new DateTime(1974, 12, 10)).Should().Be(1);
        birthDate.AgeDays(new DateTime(1974, 12, 11)).Should().Be(2);
        birthDate.AgeDays(new DateTime(1974, 12, 31)).Should().Be(22);
        birthDate.AgeDays(new DateTime(1975, 1, 1)).Should().Be(23);
        birthDate.AgeDays(new DateTime(1975, 12, 31)).Should().Be(387);
        birthDate.AgeDays(new DateTime(2022, 12, 9)).Should().Be(17532);
    }

    [TestMethod]
    public void TestDateTime()
    {
        var dt = CommonDateTimeExtensions.FromUnixSeconds(1469284816);
        Assert.AreEqual("2016-07-23 14:40:16", dt.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss"));
    }

    [TestMethod]
    public void TestDateTimeMillis()
    {
        var dt = CommonDateTimeExtensions.FromUnixMilliseconds(1469284816000L);
        Assert.AreEqual("2016-07-23 14:40:16", dt.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss"));
    }

    [TestMethod]
    public void TestDateTimeOffset()
    {
        var dt = CommonDateTimeExtensions.FromUnixSecondsOffset(1469284816);
        Assert.AreEqual("2016-07-23 14:40:16", dt.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss"));
    }

    [TestMethod]
    public void TestDateTimeOffsetMillis()
    {
        var dt = CommonDateTimeExtensions.FromUnixMillisecondsOffset(1469284816000);
        Assert.AreEqual("2016-07-23 14:40:16", dt.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss"));
    }

    [TestMethod]
    public void TestIsCloseTo()
    {
        var t20230101145556 = new DateTime(2023, 1, 1, 14, 55, 56);
        var t202301011455 = new DateTime(2023, 1, 1, 14, 55, 0);
        var t20230101 = new DateTime(2023, 1, 1);

        t20230101145556.IsCloseTo(t20230101145556, TimeSpan.TicksPerSecond).Should().BeTrue();
        t20230101145556.IsCloseTo(t202301011455, TimeSpan.TicksPerSecond).Should().BeFalse();
        t20230101145556.IsCloseTo(t202301011455, TimeSpan.TicksPerMinute).Should().BeTrue();
        t20230101145556.IsCloseTo(t20230101, TimeSpan.TicksPerMinute).Should().BeFalse();
        t20230101145556.IsCloseTo(t20230101, TimeSpan.TicksPerHour).Should().BeFalse();
        t20230101145556.IsCloseTo(t20230101, TimeSpan.TicksPerDay).Should().BeTrue();
        t202301011455.IsCloseTo(t20230101, TimeSpan.TicksPerDay).Should().BeTrue();
        t20230101.IsCloseTo(t20230101, TimeSpan.TicksPerDay).Should().BeTrue();
    }

    [TestMethod]
    public void TestTimestamp()
    {
        var dt = new DateTime(2016, 7, 23, 14, 40, 16, DateTimeKind.Utc);
        Assert.AreEqual(1469284816, dt.ToUnixSeconds());
    }

    [TestMethod]
    public void TestTimestampMillis()
    {
        var dt = new DateTime(2016, 7, 23, 14, 40, 16, DateTimeKind.Utc);
        Assert.AreEqual(1469284816000L, dt.ToUnixMilliseconds());
    }

    [TestMethod]
    public void TestTimestampOffset()
    {
        var dt = new DateTimeOffset(2016, 7, 23, 15, 40, 16, TimeSpan.FromHours(1));
        Assert.AreEqual(1469284816, dt.ToUnixSeconds());
    }

    [TestMethod]
    public void TestTimestampOffsetMillis()
    {
        var dt = new DateTimeOffset(2016, 7, 23, 15, 40, 16, TimeSpan.FromHours(1));
        Assert.AreEqual(1469284816000L, dt.ToUnixMilliseconds());
    }
}