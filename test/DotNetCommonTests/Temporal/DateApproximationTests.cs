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
        var appx = new DateApproximation(new DateTime(2023, 6, 15, 14, 30, 0));
        Assert.AreEqual("2023-06-15", appx.ToYMDString());
    }

    [TestMethod]
    public void FromString_ParsesPartialAndCompleteDates()
    {
        Assert.AreEqual(new DateApproximation(), DateApproximation.FromString("0000"));
        Assert.AreEqual(new DateApproximation(2024), DateApproximation.FromString("2024"));
        Assert.AreEqual(new DateApproximation(2024, 2), DateApproximation.FromString("2024-02"));
        Assert.AreEqual(new DateApproximation(2024, 2, 29), DateApproximation.FromString("2024-02-29"));
        Assert.ThrowsExactly<ArgumentException>(() => DateApproximation.FromString("2024-02-29-01"));
    }

    [TestMethod]
    public void Constructor_RejectsInvalidComponents()
    {
        Assert.ThrowsExactly<ArgumentException>(() => new DateApproximation(2024, 0, 1));
        Assert.ThrowsExactly<ArgumentException>(() => new DateApproximation(0, 1));
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new DateApproximation(-1));
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new DateApproximation(2024, 13));
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new DateApproximation(2023, 2, 29));
        Assert.AreEqual("2024-02-29", new DateApproximation(2024, 2, 29).ToYMDString());
    }

    [TestMethod]
    public void Comparison_OrdersUnknownAndKnownComponents()
    {
        var dates = new[] { new DateApproximation(2024, 2, 1), new DateApproximation(),
            new DateApproximation(2024, 1), new DateApproximation(2024), new DateApproximation(2023, 12, 31) };
        Array.Sort(dates);
        CollectionAssert.AreEqual(new[] { "0000-00-00", "2023-12-31", "2024-00-00", "2024-01-00", "2024-02-01" },
            dates.Select(d => d.ToYMDString()).ToArray());
        Assert.IsTrue(dates[0].CompareTo(null) > 0);
        Assert.ThrowsExactly<ArgumentException>(() => ((IComparable)dates[0]).CompareTo("2024"));
        Assert.AreEqual("2024", dates[4].ToYString());
        Assert.AreEqual("2024-02", dates[4].ToYMString());
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
