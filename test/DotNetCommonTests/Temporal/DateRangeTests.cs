using DotNetCommons.Temporal;

namespace DotNetCommonTests.Temporal;

[TestClass]
public class DateRangeTests
{
    [TestMethod]
    public void ConstructionAndMinMax_UseInclusiveDates()
    {
        var dates = new[] { new DateTime(2024, 2, 29, 23, 0, 0), new DateTime(2024, 2, 27, 8, 0, 0), new DateTime(2024, 2, 28) };
        var range = DateRange.FromMinMax(dates, d => d);
        Assert.AreEqual(new DateTime(2024, 2, 27), range.Start);
        Assert.AreEqual(new DateTime(2024, 2, 29), range.End);
        Assert.AreEqual(TimeSpan.FromDays(3), range.Span);
        Assert.IsTrue(range.InRange(new DateTime(2024, 2, 29, 23, 59, 0)));
        Assert.IsFalse(range.InRange(new DateTime(2024, 3, 1)));
        Assert.ThrowsExactly<InvalidOperationException>(() => DateRange.FromMinMax(Array.Empty<DateTime>(), d => d));
    }

    [TestMethod]
    public void BoundRange_ClampsWithoutChangingOriginal()
    {
        var original = new DateRange(new DateTime(2024, 1, 1), new DateTime(2024, 1, 31)) { Type = DateRangeType.Monthly };
        var bounded = original.BoundRange(new DateTime(2024, 1, 10), new DateTime(2024, 1, 20));
        Assert.AreEqual(new DateTime(2024, 1, 10), bounded.Start);
        Assert.AreEqual(new DateTime(2024, 1, 20), bounded.End);
        Assert.AreEqual(DateRangeType.Monthly, bounded.Type);
        Assert.AreEqual(new DateTime(2024, 1, 1), original.Start);
        Assert.AreEqual(new DateTime(2024, 1, 31), original.End);
    }

    [TestMethod]
    public void InRange_RespectsBoundingModesAndOverlap()
    {
        var outer = new DateRange(new DateTime(2024, 1, 10), new DateTime(2024, 1, 20));
        var inner = new DateRange(new DateTime(2024, 1, 12), new DateTime(2024, 1, 18));
        var crossing = new DateRange(new DateTime(2024, 1, 18), new DateTime(2024, 1, 25));
        var disjoint = new DateRange(new DateTime(2024, 1, 21), new DateTime(2024, 1, 25));
        Assert.IsTrue(inner.InRange(outer, DateRangeBound.CompletelyInside));
        Assert.IsFalse(crossing.InRange(outer, DateRangeBound.CompletelyInside));
        Assert.IsTrue(crossing.InRange(outer, DateRangeBound.StartInside));
        Assert.IsFalse(crossing.InRange(outer, DateRangeBound.EndInside));
        Assert.IsTrue(crossing.InRange(outer, DateRangeBound.PartiallyInside));
        Assert.IsFalse(disjoint.InRange(outer, DateRangeBound.PartiallyInside));
        Assert.IsTrue(outer.InRange(inner, DateRangeBound.PartiallyInside));
    }

    [TestMethod]
    public void RangeBasedOnDate_CalculatesAllPeriodTypesAndNeighbors()
    {
        var date = new DateTime(2024, 5, 15, 14, 0, 0);
        var cases = new (DateRangeType Type, DateTime Start, DateTime End)[]
        {
            (DateRangeType.Daily, new(2024, 5, 15), new(2024, 5, 15)),
            (DateRangeType.Weekly, new(2024, 5, 12), new(2024, 5, 18)),
            (DateRangeType.WeeklyIso, new(2024, 5, 13), new(2024, 5, 19)),
            (DateRangeType.Biweekly, new(2024, 5, 12), new(2024, 5, 25)),
            (DateRangeType.BiweeklyIso, new(2024, 5, 13), new(2024, 5, 26)),
            (DateRangeType.Monthly, new(2024, 5, 1), new(2024, 5, 31)),
            (DateRangeType.Bimonthly, new(2024, 5, 1), new(2024, 6, 30)),
            (DateRangeType.Quarterly, new(2024, 4, 1), new(2024, 6, 30)),
            (DateRangeType.Tertile, new(2024, 5, 1), new(2024, 8, 31)),
            (DateRangeType.Semiannually, new(2024, 1, 1), new(2024, 6, 30)),
            (DateRangeType.Annually, new(2024, 1, 1), new(2024, 12, 31)),
            (DateRangeType.Biannually, new(2024, 1, 1), new(2025, 12, 31)),
            (DateRangeType.Decade, new(2020, 1, 1), new(2029, 12, 31)),
            (DateRangeType.Century, new(2000, 1, 1), new(2099, 12, 31)),
            (DateRangeType.Millenium, new(2000, 1, 1), new(2999, 12, 31))
        };
        foreach (var (type, start, end) in cases)
        {
            var range = DateRange.RangeBasedOnDate(type, date);
            Assert.AreEqual(start, range.Start, type.ToString());
            Assert.AreEqual(end, range.End, type.ToString());
            Assert.AreEqual(end.AddDays(1), range.Next().Start, type.ToString());
            Assert.AreEqual(start.AddDays(-1), range.Previous().End, type.ToString());
            var partial = DateRange.RangeStartingOnDate(type, date);
            Assert.AreEqual(date.Date, partial.Start, type.ToString());
            Assert.AreEqual(end, partial.End, type.ToString());
        }
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => DateRange.RangeBasedOnDate(DateRangeType.Undefined, date));
        Assert.ThrowsExactly<InvalidOperationException>(() => new DateRange().Next());
        Assert.ThrowsExactly<InvalidOperationException>(() => new DateRange().Previous());
    }

    [TestMethod]
    public void Previous_FromPartialRange_ReturnsPreviousFullPeriod()
    {
        var partial = DateRange.RangeStartingOnDate(DateRangeType.Monthly, new DateTime(2024, 3, 15));
        var previous = partial.Previous();
        Assert.AreEqual(new DateTime(2024, 2, 1), previous.Start);
        Assert.AreEqual(new DateTime(2024, 2, 29), previous.End);
    }
}
