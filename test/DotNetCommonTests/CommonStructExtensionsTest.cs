using DotNetCommons;

// ReSharper disable UnusedMember.Global

namespace DotNetCommonTests;

[TestClass]
public class CommonStructExtensionsTest
{
    [TestMethod]
    public void Between_Int()
    {
        var value = 5;
        Assert.IsTrue(value.Between(1, 10, true));  // 1 ≤ 5 ≤ 10
        Assert.IsTrue(value.Between(5, 10, true));  // 5 ≤ 5 ≤ 10
        Assert.IsTrue(value.Between(1, 5, true));   // 1 ≤ 5 ≤ 5
        Assert.IsFalse(value.Between(6, 10, true)); // 6 ≤ 5 ≤ 10 (False)

        Assert.IsTrue(value.Between(1, 10, false));  // 1 ≤ 5 < 10
        Assert.IsTrue(value.Between(5, 10, false)); // 5 ≤ 5 < 10 (True)
        Assert.IsFalse(value.Between(1, 5, false));  // 1 ≤ 5 < 5 (False)
    }

    [TestMethod]
    public void Between_Double()
    {
        var value = 7.5;
        Assert.IsTrue(value.Between(5.0, 10.0, true));  // 5.0 ≤ 7.5 ≤ 10.0
        Assert.IsFalse(value.Between(8.0, 10.0, true)); // 8.0 ≤ 7.5 ≤ 10.0 (False)
        Assert.IsTrue(value.Between(5.0, 7.5, true));   // 5.0 ≤ 7.5 ≤ 7.5

        Assert.IsTrue(value.Between(5.0, 10.0, false)); // 5.0 ≤ 7.5 < 10.0
        Assert.IsFalse(value.Between(5.0, 7.5, false)); // 5.0 ≤ 7.5 < 7.5 (False)
    }

    [TestMethod]
    public void Between_DateTime()
    {
        var value = new DateTime(2025, 1, 15);

        var jan1  = new DateTime(2025, 1, 1);
        var jan16 = new DateTime(2025, 1, 16);
        var feb1  = new DateTime(2025, 2, 1);

        Assert.IsTrue(value.Between(jan1, feb1, true));                      // Jan 1 ≤ Jan 15 ≤ Feb 1
        Assert.IsTrue(value.Between(jan1, value, true));                    // Jan 1 ≤ Jan 15 ≤ Jan 15
        Assert.IsFalse(value.Between(jan16, feb1, true)); // Jan 16 ≤ Jan 15 ≤ Feb 1 (False)

        Assert.IsTrue(value.Between(jan1, feb1, false));    // Jan 1 ≤ Jan 15 < Feb 1
        Assert.IsFalse(value.Between(jan1, value, false)); // Jan 1 ≤ Jan 15 < Jan 15 (False)
    }

    [TestMethod]
    public void Between_TimeSpan()
    {
        var value = TimeSpan.FromHours(5);

        var hr1  = TimeSpan.FromHours(1);
        var hr5  = TimeSpan.FromHours(5);
        var hr6  = TimeSpan.FromHours(6);
        var hr10 = TimeSpan.FromHours(10);

        Assert.IsTrue(value.Between(hr1, hr10, true));  // 1h ≤ 5h ≤ 10h
        Assert.IsFalse(value.Between(hr6, hr10, true)); // 6h ≤ 5h ≤ 10h (False)

        Assert.IsTrue(value.Between(hr1, hr10, false)); // 1h ≤ 5h < 10h
        Assert.IsFalse(value.Between(hr1, hr5, false)); // 1h ≤ 5h < 5h (False)
    }

    [TestMethod]
    public void Limit()
    {
        Assert.AreEqual(5, (-1).Limit(5, 10));
        Assert.AreEqual(5, 4.Limit(5, 10));
        Assert.AreEqual(5, 5.Limit(5, 10));
        Assert.AreEqual(6, 6.Limit(5, 10));
        Assert.AreEqual(7, 7.Limit(5, 10));
        Assert.AreEqual(8, 8.Limit(5, 10));
        Assert.AreEqual(9, 9.Limit(5, 10));
        Assert.AreEqual(10, 10.Limit(5, 10));
        Assert.AreEqual(10, 11.Limit(5, 10));
        Assert.AreEqual(10, 12.Limit(5, 10));
    }

    [TestMethod]
    public void ParityEven_Works()
    {
        Assert.IsTrue(0u.IsParityEven());
        Assert.IsFalse(1u.IsParityEven());
        Assert.IsFalse(2u.IsParityEven());
        Assert.IsTrue(3u.IsParityEven());
        Assert.IsFalse(4711u.IsParityEven());
        Assert.IsTrue(0xFFFFu.IsParityEven());
    }

    [TestMethod]
    public void ParityOdd_Works()
    {
        Assert.IsFalse(0u.IsParityOdd());
        Assert.IsTrue(1u.IsParityOdd());
        Assert.IsTrue(2u.IsParityOdd());
        Assert.IsFalse(3u.IsParityOdd());
        Assert.IsTrue(4711u.IsParityOdd());
        Assert.IsFalse(0xFFFFu.IsParityOdd());
    }
}