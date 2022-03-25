using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable UnusedMember.Global

namespace DotNetCommons.Test;

[TestClass]
public class StructExtensionsTest
{
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
    public void BitCount()
    {
        Assert.AreEqual(1, 1u.BitCount());
        Assert.AreEqual(1, 2u.BitCount());
        Assert.AreEqual(2, 3u.BitCount());
        Assert.AreEqual(7, 4711u.BitCount());
        Assert.AreEqual(16, 0xFFFFu.BitCount());
    }
}