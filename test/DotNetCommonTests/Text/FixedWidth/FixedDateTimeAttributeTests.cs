using System.Globalization;
using DotNetCommons.Text.FixedWidth;

namespace DotNetCommonTests.Text.FixedWidth;

[TestClass]
public class FixedDateTimeAttributeTests
{
    private readonly CultureInfo _culture = CultureInfo.InvariantCulture;

    [TestMethod]
    public void FormatValue_DateTime()
    {
        var attr = new FixedDateTimeAttribute(1, 14) { Format = "yyyyMMddHHmmss" };
        var date = new DateTime(2023, 12, 31, 23, 59, 58);
        Assert.AreEqual("20231231235958", attr.FormatValue(date, _culture));
    }

    [TestMethod]
    public void FormatValue_Null_ReturnsEmptyPadded()
    {
        var attr = new FixedDateTimeAttribute(1, 8);
        Assert.AreEqual("        ", attr.FormatValue(null, _culture));
    }

    [TestMethod]
    public void Parse_DateTime()
    {
        var attr = new FixedDateTimeAttribute(1, 14) { Format = "yyyyMMddHHmmss" };
        var expected = new DateTime(2023, 12, 31, 23, 59, 58);
        Assert.AreEqual(expected, attr.Parse("20231231235958", _culture));
    }
}