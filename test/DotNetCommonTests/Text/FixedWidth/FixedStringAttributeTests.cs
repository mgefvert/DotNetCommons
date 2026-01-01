using System.Globalization;
using DotNetCommons.Text.FixedWidth;

namespace DotNetCommonTests.Text.FixedWidth;

[TestClass]
public class FixedStringAttributeTests
{
    private readonly CultureInfo _culture = CultureInfo.InvariantCulture;

    [TestMethod]
    public void FormatValue_String()
    {
        var attr = new FixedStringAttribute(1, 5);
        Assert.AreEqual("Hello", attr.FormatValue("Hello", _culture));
    }

    [TestMethod]
    public void FormatValue_Padding()
    {
        var attr = new FixedStringAttribute(1, 10);
        Assert.AreEqual("Hello     ", attr.FormatValue("Hello", _culture));
    }

    [TestMethod]
    public void FormatValue_UpperCase()
    {
        var attr = new FixedStringAttribute(1, 5) { UpperCase = true };
        Assert.AreEqual("HELLO", attr.FormatValue("Hello", _culture));
    }

    [TestMethod]
    public void FormatValue_AllowedChars()
    {
        var attr = new FixedStringAttribute(1, 5) { AllowedChars = "ABC" };
        Assert.AreEqual("ABC  ", attr.FormatValue("ABCDEF", _culture));
    }

    [TestMethod]
    public void FormatValue_Null_ReturnsEmptyPadded()
    {
        var attr = new FixedStringAttribute(1, 5);
        Assert.AreEqual("     ", attr.FormatValue(null, _culture));
    }

    [TestMethod]
    public void Parse_String()
    {
        var attr = new FixedStringAttribute(1, 10);
        Assert.AreEqual("Hello", attr.Parse("Hello     ", _culture));
    }
}