using DotNetCommons.Temporal;

namespace DotNetCommonTests.Temporal;

[TestClass]
public class DateTimeToolsTest
{
    [TestMethod]
    public void TestGenerate()
    {
        var strings = DateTimeTools.Generate(new DateTime(2020, 1, 1), new DateTime(2020, 6, 1),
                dt => dt.AddMonths(1))
            .Select(x => x.ToString("yyyyMMdd"));

        Assert.AreEqual("20200101,20200201,20200301,20200401,20200501,20200601", string.Join(",", strings));

        strings = DateTimeTools.Generate(new DateTime(2019, 5, 1), 5, dt => dt.AddDays(2))
            .Select(x => x.ToString("yyyyMMdd"));
        Assert.AreEqual("20190501,20190503,20190505,20190507,20190509", string.Join(",", strings));
    }

    [TestMethod]
    public void Generate_HandlesEmptyAndInclusiveBounds()
    {
        var start = new DateTime(2024, 2, 28);
        CollectionAssert.AreEqual(new[] { start, start.AddDays(1), start.AddDays(2) },
            DateTimeTools.Generate(start, new DateTime(2024, 3, 1), d => d.AddDays(1)).ToArray());
        Assert.AreEqual(0, DateTimeTools.Generate(start, start.AddDays(-1), d => d.AddDays(1)).Count());
        Assert.AreEqual(0, DateTimeTools.Generate(start, 0, d => d.AddDays(1)).Count());
    }

    [TestMethod]
    public void ForceKind_PreservesTicksAndHandlesNull()
    {
        var date = new DateTime(2024, 2, 29, 12, 30, 0, DateTimeKind.Local);
        var converted = date.ForceKind(DateTimeKind.Utc);
        Assert.AreEqual(date.Ticks, converted.Ticks);
        Assert.AreEqual(DateTimeKind.Utc, converted.Kind);
        DateTime? nullable = date;
        Assert.AreEqual(DateTimeKind.Unspecified, nullable.ForceKind(DateTimeKind.Unspecified)!.Value.Kind);
        nullable = null;
        Assert.IsNull(nullable.ForceKind(DateTimeKind.Utc));
    }

    [TestMethod]
    public void MinMax_HandlePairsNullsAndEmptySequences()
    {
        var early = new DateTime(2024, 1, 1);
        var late = new DateTime(2024, 2, 1);
        Assert.AreEqual(early, DateTimeTools.MinDate(late, early));
        Assert.AreEqual(late, DateTimeTools.MaxDate(early, late));
        Assert.AreEqual(early, DateTimeTools.MinDate(new DateTime?[] { null, late, early }));
        Assert.AreEqual(late, DateTimeTools.MaxDate(new DateTime?[] { null, early, late }));
        Assert.IsNull(DateTimeTools.MinDate(Array.Empty<DateTime?>()));
        Assert.IsNull(DateTimeTools.MaxDate(new DateTime?[] { null }));
        var earlyOffset = new DateTimeOffset(early, TimeSpan.Zero);
        var lateOffset = new DateTimeOffset(late, TimeSpan.Zero);
        Assert.AreEqual(earlyOffset, DateTimeTools.MinDate(lateOffset, earlyOffset));
        Assert.AreEqual(lateOffset, DateTimeTools.MaxDate(earlyOffset, lateOffset));
        Assert.AreEqual(earlyOffset, DateTimeTools.MinDate(new DateTimeOffset?[] { null, lateOffset, earlyOffset }));
        Assert.AreEqual(lateOffset, DateTimeTools.MaxDate(new DateTimeOffset?[] { null, earlyOffset, lateOffset }));
        Assert.IsNull(DateTimeTools.MinDate(Array.Empty<DateTimeOffset?>()));
        Assert.IsNull(DateTimeTools.MaxDate(new DateTimeOffset?[] { null }));
    }

    [TestMethod]
    public void FindTimeZone_ReturnsNullForMissingId()
    {
        Assert.IsNotNull(DateTimeTools.FindTimeZone("UTC"));
        Assert.IsNull(DateTimeTools.FindTimeZone(" "));
        Assert.IsNull(DateTimeTools.FindTimeZone("Not/A/Real/TimeZone"));
    }

    [TestMethod]
    public void MinMax_DateTimeOffsetPairsCompareInstants()
    {
        var earlier = new DateTimeOffset(2024, 1, 1, 12, 0, 0, TimeSpan.FromHours(2));
        var later = new DateTimeOffset(2024, 1, 1, 11, 0, 0, TimeSpan.Zero);
        Assert.AreEqual(earlier, DateTimeTools.MinDate(earlier, later));
        Assert.AreEqual(later, DateTimeTools.MaxDate(earlier, later));
    }
}
