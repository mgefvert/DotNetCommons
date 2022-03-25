using System;
using System.Collections.Generic;
using System.Linq;
using DotNetCommons.Collections;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CollectionExtensions = DotNetCommons.Collections.CollectionExtensions;

namespace DotNetCommons.Test.Collections;

[TestClass]
public class CollectionExtensionsTest
{
    private List<string> _list = null!;
    private Dictionary<string, int> _dictI = null!;
    private Dictionary<string, decimal> _dictD = null!;

    [TestInitialize]
    public void Setup()
    {
        _list = new List<string> { "A", "AB", "C", "D" };
        _dictI = new Dictionary<string, int> { ["A"] = 1, ["B"] = 0 };
        _dictD = new Dictionary<string, decimal> { ["A"] = 1, ["B"] = 0 };
    }

    [TestMethod]
    public void TestAddIfNotNull()
    {
        var list = new List<string>();

        ((List<string>?)null).AddIfNotNull("hello");

        list.AddIfNotNull("hello");
        list.AddIfNotNull("");
        list.AddIfNotNull(null);

        Assert.AreEqual(2, list.Count);
    }

    [TestMethod]
    public void TestAddRangeIfNotNull()
    {
        var list = new List<string>();

        ((List<string>?)null).AddRangeIfNotNull(null);
        ((List<string>?)null).AddRangeIfNotNull(list);

        list.AddRangeIfNotNull(null);
        list.AddRangeIfNotNull(new[] { "hello", "", null });

        Assert.AreEqual(2, list.Count);
    }

    [TestMethod]
    public void TestBatch()
    {
        var batches = new[] { 'a', 'b', 'c', 'd', 'e', 'f' }.Batch(3)
            .Select(x => string.Join("", x))
            .ToList();

        Assert.AreEqual(2, batches.Count);
        Assert.AreEqual("abc", batches[0]);
        Assert.AreEqual("def", batches[1]);

        batches = new[] { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h' }.Batch(3)
            .Select(x => string.Join("", x))
            .ToList();

        Assert.AreEqual(3, batches.Count);
        Assert.AreEqual("abc", batches[0]);
        Assert.AreEqual("def", batches[1]);
        Assert.AreEqual("gh", batches[2]);

        batches = new[] { 'a' }.Batch(3)
            .Select(x => string.Join("", x))
            .ToList();

        Assert.AreEqual(1, batches.Count);
        Assert.AreEqual("a", batches[0]);

        batches = Array.Empty<char>().Batch(3)
            .Select(x => string.Join("", x))
            .ToList();

        Assert.AreEqual(0, batches.Count);
    }

    [TestMethod]
    public void TestExtractAt()
    {
        var item = _list.ExtractAt(1);

        Assert.AreEqual(item, "AB");
        Assert.AreEqual("A,C,D", string.Join(",", _list));
    }

    [TestMethod]
    public void TestExtractAtOrDefault()
    {
        var item = _list.ExtractAtOrDefault(1);

        Assert.AreEqual(item, "AB");
        Assert.AreEqual("A,C,D", string.Join(",", _list));

        item = _list.ExtractAtOrDefault(99);

        Assert.IsNull(item);
        Assert.AreEqual("A,C,D", string.Join(",", _list));
    }

    [TestMethod]
    public void TestExtractAll()
    {
        var items = _list.ExtractAll();

        Assert.AreEqual(4, items.Count);
        Assert.AreEqual(0, _list.Count);
    }

    [TestMethod]
    public void TestExtractAll_WithPredicate()
    {
        var items = _list.ExtractAll(x => x.StartsWith("A"));

        Assert.AreEqual("A,AB", string.Join(",", items));
        Assert.AreEqual("C,D", string.Join(",", _list));
    }

    [TestMethod]
    public void TestExtractFirst()
    {
        Assert.AreEqual("A", _list.ExtractFirst());
    }

    [TestMethod]
    public void TestExtractFirstOrDefault()
    {
        Assert.AreEqual("A", _list.ExtractFirstOrDefault());
        _list.Clear();
        Assert.IsNull(_list.ExtractFirstOrDefault());
    }

    [TestMethod]
    public void TestExtractLast()
    {
        Assert.AreEqual("D", _list.ExtractLast());
    }

    [TestMethod]
    public void TestExtractLastOrDefault()
    {
        Assert.AreEqual("D", _list.ExtractLastOrDefault());
        _list.Clear();
        Assert.IsNull(_list.ExtractLastOrDefault());
    }

    [TestMethod]
    public void TestExtractRange()
    {
        var list = Enumerable.Range(1, 10).ToList();

        var extract = list.ExtractRange(3, 2);

        Assert.AreEqual("4,5", string.Join(",", extract));
        Assert.AreEqual("1,2,3,6,7,8,9,10", string.Join(",", list));
    }

    [TestMethod]
    public void TestIncrementInteger()
    {
        Assert.AreEqual(2, _dictI.Increment("A"));
        Assert.AreEqual(2, _dictI.Increment("Z", 2));
    }

    [TestMethod]
    public void TestIntersect()
    {
        var list1 = new[] { 1, 2, 3, 4, 5 };
        var list2 = new[] { 4, 5, 6, 7 };

        var intersect = CollectionExtensions.Intersect(list1, list2);
        CollectionAssert.AreEqual(new[] { 1, 2, 3 }, intersect.Left);
        CollectionAssert.AreEqual(new[] { (4, 4), (5, 5) }, intersect.Both);
        CollectionAssert.AreEqual(new[] { 6, 7 }, intersect.Right);

        list1 = new[] { 1, 2, 3, 4, 5 };
        list2 = new[] { 4, 5 };

        intersect = CollectionExtensions.Intersect(list1, list2);
        CollectionAssert.AreEqual(new[] { 1, 2, 3 }, intersect.Left);
        CollectionAssert.AreEqual(new[] { (4, 4), (5, 5) }, intersect.Both);
        CollectionAssert.AreEqual(Array.Empty<int>(), intersect.Right);

        list1 = Array.Empty<int>();
        list2 = new[] { 4, 5 };

        intersect = CollectionExtensions.Intersect(list1, list2);
        CollectionAssert.AreEqual(Array.Empty<int>(), intersect.Left);
        CollectionAssert.AreEqual(Array.Empty<int>(), intersect.Both);
        CollectionAssert.AreEqual(new[] { 4, 5 }, intersect.Right);
    }

    [TestMethod]
    public void TestIncrementDecimal()
    {
        Assert.AreEqual(2.99M, _dictD.Increment("A", 1.99M));
        Assert.AreEqual(2.45M, _dictD.Increment("Z", 2.45M));
    }

    [TestMethod]
    public void TestMinMax()
    {
        var list = new[] { 4, 7, 9, 3, 2, 10, 99, 64, 5 };

        var (min, max) = list.MinMax();
        Assert.AreEqual(2, min);
        Assert.AreEqual(99, max);

        list = new[] { 4 };

        (min, max) = list.MinMax();
        Assert.AreEqual(4, min);
        Assert.AreEqual(4, max);
    }

    [TestMethod]
    public void TestMinMax_WithSelector()
    {
        var list = new int?[] { 4, 7, 9, 3, 2, 10, 99, 64, 5 };

        var (min, max) = list.MinMax(x => x!.Value);
        Assert.AreEqual(2, min);
        Assert.AreEqual(99, max);
    }

    [TestMethod]
    public void TestRepeat()
    {
        var list = new[] { 1, 2, 3 };

        Assert.AreEqual("123123123", string.Join("", list.Repeat(3)));
        Assert.AreEqual("123", string.Join("", list.Repeat(1)));
        Assert.AreEqual("", string.Join("", list.Repeat(0)));

        list = new[] { 1 };

        Assert.AreEqual("111", string.Join("", list.Repeat(3)));
        Assert.AreEqual("1", string.Join("", list.Repeat(1)));
        Assert.AreEqual("", string.Join("", list.Repeat(0)));

        list = Array.Empty<int>();

        Assert.AreEqual("", string.Join("", list.Repeat(3)));
        Assert.AreEqual("", string.Join("", list.Repeat(1)));
        Assert.AreEqual("", string.Join("", list.Repeat(0)));
    }
}