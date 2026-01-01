using System.Globalization;
using DotNetCommons.Text.FixedWidth;

namespace DotNetCommonTests.Text.FixedWidth;

[TestClass]
public class FixedCharAttributeTests
{
    private readonly CultureInfo _culture = CultureInfo.InvariantCulture;

    [TestMethod]
    public void FormatValue_Char()
    {
        var attr = new FixedCharAttribute(1);
        Assert.AreEqual("A", attr.FormatValue('A', _culture));
    }

    [TestMethod]
    public void FormatValue_String()
    {
        var attr = new FixedCharAttribute(1);
        Assert.AreEqual("B", attr.FormatValue("B", _culture));
    }

    [TestMethod]
    public void FormatValue_LongString_Throws()
    {
        var attr = new FixedCharAttribute(1);
        Assert.ThrowsExactly<InvalidDataException>(() => attr.FormatValue("ABC", _culture));
    }

    [TestMethod]
    public void FormatValue_Null_ReturnsPad()
    {
        var attr = new FixedCharAttribute(1) { Pad = '.' };
        Assert.AreEqual(".", attr.FormatValue(null, _culture));
    }

    [TestMethod]
    public void Parse_Char()
    {
        var attr = new FixedCharAttribute(1);
        Assert.AreEqual('A', attr.Parse("A", _culture));
    }

    [TestMethod]
    public void Parse_Empty_ReturnsPad()
    {
        var attr = new FixedCharAttribute(1) { Pad = '.' };
        Assert.AreEqual('.', attr.Parse(".", _culture));
    }
}