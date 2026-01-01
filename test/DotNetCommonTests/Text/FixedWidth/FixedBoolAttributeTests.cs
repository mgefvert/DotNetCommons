using System.Globalization;
using DotNetCommons.Text.FixedWidth;

namespace DotNetCommonTests.Text.FixedWidth;

[TestClass]
public class FixedBoolAttributeTests
{
    private readonly CultureInfo _culture = CultureInfo.InvariantCulture;

    [TestMethod]
    public void FormatValue_Bool()
    {
        var attr = new FixedBoolAttribute(1, 1);
        Assert.AreEqual("Y", attr.FormatValue(true, _culture));
        Assert.AreEqual("N", attr.FormatValue(false, _culture));
        Assert.AreEqual("N", attr.FormatValue(null, _culture));
    }

    [TestMethod]
    public void FormatValue_CustomStrings()
    {
        var attr = new FixedBoolAttribute(1, 3) { True = "YES", False = "NO " };
        Assert.AreEqual("YES", attr.FormatValue(true, _culture));
        Assert.AreEqual("NO ", attr.FormatValue(false, _culture));
    }

    [TestMethod]
    public void FormatValue_Padding()
    {
        var attr = new FixedBoolAttribute(1, 3) { Pad = '.' };
        Assert.AreEqual("Y..", attr.FormatValue(true, _culture));
        Assert.AreEqual("N..", attr.FormatValue(false, _culture));
    }

    [TestMethod]
    public void Parse_DefaultStrings()
    {
        var attr = new FixedBoolAttribute(1, 1);
        Assert.IsTrue((bool?)attr.Parse("Y", _culture));
        Assert.IsFalse((bool?)attr.Parse("N", _culture));
    }

    [TestMethod]
    public void Parse_CustomStrings()
    {
        var attr = new FixedBoolAttribute(1, 3) { True = "YES", False = "NO " };
        Assert.IsTrue((bool?)attr.Parse("YES", _culture));
        Assert.IsFalse((bool?)attr.Parse("NO ", _culture));
    }

    [TestMethod]
    public void Parse_GenericStrings()
    {
        var attr = new FixedBoolAttribute(1, 4);
        Assert.IsTrue((bool?)attr.Parse("true", _culture));
        Assert.IsFalse((bool?)attr.Parse("false", _culture));
        Assert.IsTrue((bool?)attr.Parse("1", _culture));
        Assert.IsFalse((bool?)attr.Parse("0", _culture));
    }

    [TestMethod]
    public void Parse_WithPadding()
    {
        var attr = new FixedBoolAttribute(1, 3) { Pad = '.' };
        Assert.IsTrue((bool?)attr.Parse("Y..", _culture));
        Assert.IsFalse((bool?)attr.Parse("N..", _culture));
    }
}