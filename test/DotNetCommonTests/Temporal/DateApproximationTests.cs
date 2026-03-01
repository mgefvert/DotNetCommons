using DotNetCommons.Temporal;

namespace DotNetCommonTests.Temporal;

[TestClass]
public class DateApproximationTests
{
    [TestMethod]
    public void Constructor_DateOnly_Works()
    {
        var appx = new DateApproximation(new DateOnly(2023, 6, 15));
        Assert.AreEqual("2023-06-15", appx.ToYMDString());
    }

    [TestMethod]
    public void Constructor_DateTime_Works()
    {
        var appx = new DateApproximation(new DateOnly(2023, 6, 15));
        Assert.AreEqual("2023-06-15", appx.ToYMDString());
    }

    [TestMethod]
    public void Constructor_Works()
    {
        var appx = new DateApproximation(2023, 6, 15);
        Assert.AreEqual("2023-06-15", appx.ToYMDString());

        appx = new DateApproximation(2023, 6);
        Assert.AreEqual("2023-06-00", appx.ToYMDString());

        appx = new DateApproximation(2023);
        Assert.AreEqual("2023-00-00", appx.ToYMDString());

        appx = new DateApproximation();
        Assert.AreEqual("0000-00-00", appx.ToYMDString());
    }

    [TestMethod]
    public void ToOnly_Works()
    {
        var appx = new DateApproximation(2023, 6, 15);

        Assert.AreEqual("2023-06-15", appx.ToYMDString());
        Assert.AreEqual("2023-06-00", appx.ToYMOnly().ToYMDString());
        Assert.AreEqual("2023-00-00", appx.ToYOnly().ToYMDString());
    }

    [TestMethod]
    public void FormatYearMonth_Works()
    {
        var appx = new DateApproximation(2023, 6, 15);

        Assert.AreEqual("June 2023", appx.FormatYearMonth());
        Assert.AreEqual("June 2023", appx.ToYMOnly().FormatYearMonth());
        Assert.AreEqual("Unknown 2023", appx.ToYOnly().FormatYearMonth());
        Assert.AreEqual("Unknown", new DateApproximation().FormatYearMonth());
    }

    [TestMethod]
    public void FormatYear_Works()
    {
        var appx = new DateApproximation(2023, 6, 15);

        Assert.AreEqual("2023", appx.FormatYear());
        Assert.AreEqual("2023", appx.ToYMOnly().FormatYear());
        Assert.AreEqual("2023", appx.ToYOnly().FormatYear());
        Assert.AreEqual("Unknown", new DateApproximation().FormatYear());
    }

    [TestMethod]
    public void Equality_GroupsCorrectly()
    {
        var appx1 = new DateApproximation(2023, 6, 15);
        var appx2 = new DateApproximation(2023, 6, 20);
        var appx3 = new DateApproximation(2023, 7, 15);

        var ym1 = appx1.ToYMOnly();
        var ym2 = appx2.ToYMOnly();
        var ym3 = appx3.ToYMOnly();

        Assert.AreEqual(ym1, ym2);
        Assert.AreNotEqual(ym1, ym3);
    }
}