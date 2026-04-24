using System.Globalization;
using DotNetCommons.Text.FixedWidth;

namespace DotNetCommonTests.Text.FixedWidth;

[TestClass]
public class FixedNumberAttributeTests
{
    private readonly CultureInfo _culture = CultureInfo.InvariantCulture;

    [TestMethod]
    public void FormatValue_Int()
    {
        var attr = new FixedNumberAttribute(1, 5);
        Assert.AreEqual("00123", attr.FormatValue(123, _culture));
    }

    [TestMethod]
    public void FormatValue_Double()
    {
        var attr = new FixedNumberAttribute(1, 10) { Decimals = 2 };
        Assert.AreEqual("0000123.46", attr.FormatValue(123.456, _culture));
    }

    [TestMethod]
    public void FormatValue_Scale()
    {
        var attr = new FixedNumberAttribute(1, 5) { Scale = 2 };
        Assert.AreEqual("12300", attr.FormatValue(123, _culture));
    }

    [TestMethod]
    public void FormatValue_Null_ReturnsEmptyPadded()
    {
        var attr = new FixedNumberAttribute(1, 5);
        Assert.AreEqual("00000", attr.FormatValue(null, _culture));
    }

    [TestMethod]
    public void Parse_Int()
    {
        var attr = new FixedNumberAttribute(1, 5);
        Assert.AreEqual(123.0, attr.Parse("  123", _culture));
    }

    [TestMethod]
    public void Parse_Double()
    {
        var attr = new FixedNumberAttribute(1, 10) { Decimals = 2 };
        Assert.AreEqual(123.45, attr.Parse("    123.45", _culture));
    }

    [TestMethod]
    public void Parse_Scale()
    {
        var attr = new FixedNumberAttribute(1, 5) { Scale = 2 };
        Assert.AreEqual(1.23, attr.Parse("  123", _culture));
    }
}