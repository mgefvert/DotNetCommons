using DotNetCommons;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotNetCommonTests;

[TestClass]
public class CommonCollectionExtensionsTest
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
    public void TestExtractFirst_WithPredicate()
    {
        Assert.AreEqual("AB", _list.ExtractFirst(x => x.Length > 1));
    }

    [TestMethod]
    public void TestExtractFirstOrDefault()
    {
        Assert.AreEqual("A", _list.ExtractFirstOrDefault());
        _list.Clear();
        Assert.IsNull(_list.ExtractFirstOrDefault());
    }

    [TestMethod]
    public void TestExtractFirstOrDefault_WithPredicate()
    {
        Assert.AreEqual("AB", _list.ExtractFirstOrDefault(x => x.Length > 1));
    }

    [TestMethod]
    public void TestExtractLast()
    {
        Assert.AreEqual("D", _list.ExtractLast());
    }

    [TestMethod]
    public void TestExtractLast_WithPredicate()
    {
        Assert.AreEqual("AB", _list.ExtractLast(x => x.Length > 1));
    }

    [TestMethod]
    public void TestExtractLastOrDefault()
    {
        Assert.AreEqual("D", _list.ExtractLastOrDefault());
        _list.Clear();
        Assert.IsNull(_list.ExtractLastOrDefault());
    }

    [TestMethod]
    public void TestExtractLastOrDefault_WithPredicate()
    {
        Assert.AreEqual("AB", _list.ExtractLastOrDefault(x => x.Length > 1));
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

        var intersect = list1.IntersectCollection(list2);
        CollectionAssert.AreEqual(new[] { 1, 2, 3 }, intersect.Left);
        CollectionAssert.AreEqual(new[] { (4, 4), (5, 5) }, intersect.Both);
        CollectionAssert.AreEqual(new[] { 6, 7 }, intersect.Right);

        list1 = new[] { 1, 2, 3, 4, 5 };
        list2 = new[] { 4, 5 };

        intersect = list1.IntersectCollection(list2);
        CollectionAssert.AreEqual(new[] { 1, 2, 3 }, intersect.Left);
        CollectionAssert.AreEqual(new[] { (4, 4), (5, 5) }, intersect.Both);
        CollectionAssert.AreEqual(Array.Empty<int>(), intersect.Right);

        list1 = Array.Empty<int>();
        list2 = new[] { 4, 5 };

        intersect = list1.IntersectCollection(list2);
        CollectionAssert.AreEqual(Array.Empty<int>(), intersect.Left);
        CollectionAssert.AreEqual(Array.Empty<int>(), intersect.Both);
        CollectionAssert.AreEqual(new[] { 4, 5 }, intersect.Right);
    }

    [TestMethod]
    public void TestIntersect_EmptyLeft()
    {
        var list1 = Array.Empty<int>();
        var list2 = new[] { 4, 5, 6, 7 };

        var intersect = list1.IntersectCollection(list2);
        CollectionAssert.AreEqual(Array.Empty<int>(), intersect.Left);
        CollectionAssert.AreEqual(Array.Empty<int>(), intersect.Both);
        CollectionAssert.AreEqual(new[] { 4, 5, 6, 7 }, intersect.Right);
    }

    [TestMethod]
    public void TestIntersect_EmptyRight()
    {
        var list1 = new[] { 1, 2, 3, 4, 5 };
        var list2 = Array.Empty<int>();

        var intersect = list1.IntersectCollection(list2);
        CollectionAssert.AreEqual(new[] { 1, 2, 3, 4, 5 }, intersect.Left);
        CollectionAssert.AreEqual(Array.Empty<int>(), intersect.Both);
        CollectionAssert.AreEqual(Array.Empty<int>(), intersect.Right);
    }

    [TestMethod]
    public void TestIntersect_MassiveSet()
    {
        var list1 = Enumerable.Range(1, 20000).Select(x => 2*x).RandomOrder().ToList();
        var list2 = Enumerable.Range(1, 20000).Select(x => 3*x).RandomOrder().ToList();

        var t0      = DateTime.UtcNow;
        var result  = list1.IntersectCollection(list2);
        var elapsed = (int)(DateTime.UtcNow - t0).TotalMilliseconds;
        Console.WriteLine($"Total time = {elapsed} ms");

        if (elapsed > 100)
            Assert.Fail($"Elapsed time = {elapsed} ms; should be < 100 ms");

        foreach (var (n1, n2) in result.Both)
            Assert.AreEqual(n1, n2);

        var left  = new HashSet<int>(result.Left);
        var right = new HashSet<int>(result.Right);
        var both  = new HashSet<int>(result.Both.Select(x => x.Item1));

        foreach (var n in left)
        {
            Assert.IsTrue(n % 2 == 0);
            Assert.IsFalse(right.Contains(n));
            Assert.IsFalse(both.Contains(n));
        }

        foreach (var n in right)
        {
            Assert.IsTrue(n % 3 == 0);
            Assert.IsFalse(left.Contains(n));
            Assert.IsFalse(both.Contains(n));
        }

        foreach (var n in both)
        {
            Assert.IsTrue(n % 2 == 0 && n % 3 == 0);
            Assert.IsFalse(right.Contains(n));
            Assert.IsFalse(left.Contains(n));
        }
    }

    [TestMethod]
    public void TestIntersect_DifferentTypes()
    {
        // Left list
        var dan1 = new KeyValuePair<string, string>("dan", "dan-password");
        var joe1 = new KeyValuePair<string, string>("joe", "joe-password");
        var lars1 = new KeyValuePair<string, string>("lars", "lars-password");

        // Right list
        var jessie2 = new KeyValuePair<string, int>("jessie", 42);
        var joe2 = new KeyValuePair<string, int>("joe", 19);
        var maggie2 = new KeyValuePair<string, int>("maggie", 34);
        var zoe2 = new KeyValuePair<string, int>("zoe", 28);

        var list1 = new[] { dan1, joe1, lars1 };
        var list2 = new[] { jessie2, joe2, maggie2, zoe2 };

        var intersect = list1.IntersectCollection(list2, x => x.Key, x => x.Key);
        CollectionAssert.AreEqual(new[] { dan1, lars1 }, intersect.Left);
        CollectionAssert.AreEqual(new[] { (joe1, joe2) }, intersect.Both);
        CollectionAssert.AreEqual(new[] { jessie2, maggie2, zoe2 }, intersect.Right);
    }

    [TestMethod]
    public void TestIncrementDecimal()
    {
        Assert.AreEqual(2.99M, _dictD.Increment("A", 1.99M));
        Assert.AreEqual(2.45M, _dictD.Increment("Z", 2.45M));
    }

    [TestMethod]
    public void TestIsEmpty()
    {
        ((int[]?)null).IsEmpty().Should().BeTrue();
        Array.Empty<int>().IsEmpty().Should().BeTrue();
        new[] { 1 }.IsEmpty().Should().BeFalse();
        new[] { 1, 2 }.IsEmpty().Should().BeFalse();
    }
    
    [TestMethod]
    public void TestIsOne()
    {
        ((int[]?)null).IsOne().Should().BeFalse();
        Array.Empty<int>().IsOne().Should().BeFalse();
        new[] { 1 }.IsOne().Should().BeTrue();
        new[] { 1, 2 }.IsOne().Should().BeFalse();
    }
    
    [TestMethod]
    public void TestAtLeastOne()
    {
        ((int[]?)null).IsAtLeastOne().Should().BeFalse();
        Array.Empty<int>().IsAtLeastOne().Should().BeFalse();
        new[] { 1 }.IsAtLeastOne().Should().BeTrue();
        new[] { 1, 2 }.IsAtLeastOne().Should().BeTrue();
    }
    
    [TestMethod]
    public void TestIsMany()
    {
        ((int[]?)null).IsMany().Should().BeFalse();
        Array.Empty<int>().IsMany().Should().BeFalse();
        new[] { 1 }.IsMany().Should().BeFalse();
        new[] { 1, 2 }.IsMany().Should().BeTrue();
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

    [TestMethod]
    public void TestSwap()
    {
        var list = new[] { 1, 2, 3, 4, 5 };
        Assert.IsTrue(list.Swap(0, 1));
        Assert.AreEqual("2,1,3,4,5", string.Join(",", list));

        list = new[] { 1, 2, 3, 4, 5 };
        Assert.IsTrue(list.Swap(1, 2));
        Assert.AreEqual("1,3,2,4,5", string.Join(",", list));

        list = new[] { 1, 2, 3, 4, 5 };
        Assert.IsTrue(list.Swap(0, 4));
        Assert.AreEqual("5,2,3,4,1", string.Join(",", list));

        list = new[] { 1, 2, 3, 4, 5 };
        Assert.IsTrue(list.Swap(3, 4));
        Assert.AreEqual("1,2,3,5,4", string.Join(",", list));

        list = new[] { 1, 2, 3, 4, 5 };
        Assert.IsFalse(list.Swap(-1, 0));
        Assert.AreEqual("1,2,3,4,5", string.Join(",", list));

        list = new[] { 1, 2, 3, 4, 5 };
        Assert.IsFalse(list.Swap(0, 5));
        Assert.AreEqual("1,2,3,4,5", string.Join(",", list));
    }

    public class TreeNode
    {
        public string Name { get; }
        public List<TreeNode> Children { get; } = new List<TreeNode>();

        public TreeNode(string name, params TreeNode[] children)
        {
            Name = name;
            Children.AddRange(children);
        }
    }

    private static List<TreeNode> MakeTreeNodes()
    {
        return new List<TreeNode>
        {
            new TreeNode("Top",
                new TreeNode("TopLeft"),
                new TreeNode("TopRight")
            ),
            new TreeNode("Middle"),
            new TreeNode("Bottom",
                new TreeNode("BottomLeft",
                    new TreeNode("BottomLeftDeep")
                ),
                new TreeNode("BottomRight",
                    new TreeNode("BottomRightDeep")
                )
            )
        };
    }

    [TestMethod]
    public void TestWalkTree_DepthFirst()
    {
        var names = MakeTreeNodes()
            .WalkTree(n => n.Children, WalkTreeMode.DepthFirst)
            .Select(n => n.Name)
            .ToList();

        names.Count.Should().Be(9);

        var text = string.Join(",", names);
        text.Should().Be("Top,TopLeft,TopRight,Middle,Bottom,BottomLeft,BottomLeftDeep,BottomRight,BottomRightDeep");
    }

    [TestMethod]
    public void TestWalkTree_ShallowFirst()
    {
        var names = MakeTreeNodes()
            .WalkTree(n => n.Children, WalkTreeMode.ShallowFirst)
            .Select(n => n.Name)
            .ToList();

        names.Count.Should().Be(9);

        var text = string.Join(",", names);
        text.Should().Be("Top,Middle,Bottom,TopLeft,TopRight,BottomLeft,BottomRight,BottomLeftDeep,BottomRightDeep");
    }

    [TestMethod]
    public void TestWalkTree_BreakInMiddle()
    {
        var nodes = MakeTreeNodes();
        var names = new List<string>();

        foreach (var node in nodes.WalkTree(n => n.Children, WalkTreeMode.ShallowFirst))
        {
            names.Add(node.Name);
            if (names.Count >= 5)
                break;
        }

        names.Count.Should().Be(5);
        var text = string.Join(",", names);
        text.Should().Be("Top,Middle,Bottom,TopLeft,TopRight");
    }
}