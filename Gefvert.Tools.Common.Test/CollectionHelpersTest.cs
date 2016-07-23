using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Gefvert.Tools.Common.Test
{
  [TestClass]
  public class CollectionHelpersTest
  {
    private List<string> _list;
    private Dictionary<string, int> _dictI;
    private Dictionary<string, decimal> _dictD;

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

      ((List<string>)null).AddIfNotNull("hello");

      list.AddIfNotNull("hello");
      list.AddIfNotNull("");
      list.AddIfNotNull((string)null);

      Assert.AreEqual(2, list.Count);
    }

    [TestMethod]
    public void TestAddIfNotNullRange()
    {
      var list = new List<string>();

      ((List<string>)null).AddIfNotNull((List<string>)null);
      ((List<string>)null).AddIfNotNull(list);

      list.AddIfNotNull((List<string>)null);
      list.AddIfNotNull(new[] { "hello", "", null });

      Assert.AreEqual(2, list.Count);
    }

    [TestMethod]
    public void TestExtractAt()
    {
      var item = _list.ExtractAt(1);

      Assert.AreEqual(item, "AB");
      Assert.AreEqual("A,C,D", string.Join(",", _list));
    }

    [TestMethod]
    public void TestExtractAll()
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
    public void TestExtractLast()
    {
      Assert.AreEqual("D", _list.ExtractLast());
    }

    [TestMethod]
    public void TestGetOrDefault()
    {
      Assert.AreEqual(1, _dictI.GetOrDefault("A"));
      Assert.AreEqual(0, _dictI.GetOrDefault("B"));
      Assert.AreEqual(0, _dictI.GetOrDefault("C"));
    }

    [TestMethod]
    public void TestIncreaseInteger()
    {
      Assert.AreEqual(2, _dictI.Increase("A"));
      Assert.AreEqual(2, _dictI.Increase("Z", 2));
    }

    [TestMethod]
    public void TestIncreaseDecimal()
    {
      Assert.AreEqual(2.99M, _dictD.Increase("A", 1.99M));
      Assert.AreEqual(2.45M, _dictD.Increase("Z", 2.45M));
    }
  }
}
