using System.Globalization;
using DotNetCommons.Text.FixedWidth;

namespace DotNetCommonTests.Text.FixedWidth;

[TestClass]
public class FixedDateAttributeTests
{
    private readonly CultureInfo _culture = CultureInfo.InvariantCulture;

    [TestMethod]
    public void FormatValue_DateTime()
    {
        var attr = new FixedDateAttribute(1, 8);
        var date = new DateTime(2023, 12, 31);
        Assert.AreEqual("20231231", attr.FormatValue(date, _culture));
    }

    [TestMethod]
    public void FormatValue_CustomFormat()
    {
        var attr = new FixedDateAttribute(1, 10) { Format = "yyyy-MM-dd" };
        var date = new DateTime(2023, 12, 31);
        Assert.AreEqual("2023-12-31", attr.FormatValue(date, _culture));
    }

    [TestMethod]
    public void FormatValue_Null_ReturnsEmptyPadded()
    {
        var attr = new FixedDateAttribute(1, 8);
        Assert.AreEqual("        ", attr.FormatValue(null, _culture));
    }

    [TestMethod]
    public void Parse_DateTime()
    {
        var attr = new FixedDateAttribute(1, 8);
        var expected = new DateTime(2023, 12, 31);
        Assert.AreEqual(expected, attr.Parse("20231231", _culture));
    }

    [TestMethod]
    public void Parse_CustomFormat()
    {
        var attr = new FixedDateAttribute(1, 10) { Format = "yyyy-MM-dd" };
        var expected = new DateTime(2023, 12, 31);
        Assert.AreEqual(expected, attr.Parse("2023-12-31", _culture));
    }
}