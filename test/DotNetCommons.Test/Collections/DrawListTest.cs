using System;
using System.Collections.Generic;
using System.Linq;
using DotNetCommons.Collections;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotNetCommons.Test.Collections;

[TestClass]
public class DrawListTest
{
    [TestMethod]
    public void TestRepeating()
    {
        var draw = new List<int>();

        var list = new DrawList<int>();
        Assert.AreEqual(0, list.Count());
        Assert.AreEqual(0, list.Left());

        list.Seed(new[] { 1, 2 });
        Assert.AreEqual(2, list.Count());
        Assert.AreEqual(2, list.Left());

        draw.Add(list.Draw());
        Assert.AreEqual(2, list.Count());
        Assert.AreEqual(1, list.Left());

        draw.Add(list.Draw());
        Assert.AreEqual(2, list.Count());
        Assert.AreEqual(2, list.Left());

        draw.Add(list.Draw());
        Assert.AreEqual(2, list.Count());
        Assert.AreEqual(1, list.Left());

        draw.Add(list.Draw());
        Assert.AreEqual(2, list.Count());
        Assert.AreEqual(2, list.Left());

        Assert.AreEqual("1122", string.Join("", draw.OrderBy(x => x)));
    }

    [TestMethod]
    public void TestSingle()
    {
        var draw = new List<int>();

        var list = new DrawList<int> { Repeat = false };
        Assert.AreEqual(0, list.Count());
        Assert.AreEqual(0, list.Left());
            
        list.Seed(new [] { 1, 2 });
        Assert.AreEqual(2, list.Count());
        Assert.AreEqual(2, list.Left());

        draw.Add(list.Draw());
        Assert.AreEqual(2, list.Count());
        Assert.AreEqual(1, list.Left());

        draw.Add(list.Draw());
        Assert.AreEqual(2, list.Count());
        Assert.AreEqual(0, list.Left());

        draw.Add(list.Draw());
        Assert.AreEqual(2, list.Count());
        Assert.AreEqual(0, list.Left());

        draw.Add(list.Draw());
        Assert.AreEqual(2, list.Count());
        Assert.AreEqual(0, list.Left());

        Assert.AreEqual("0012", string.Join("", draw.OrderBy(x => x)));
    }
}