using System.Globalization;
using DotNetCommons.Text.FixedWidth;

namespace DotNetCommonTests.Text.FixedWidth;

[TestClass]
public class FixedTimeAttributeTests
{
    private readonly CultureInfo _culture = CultureInfo.InvariantCulture;

    [TestMethod]
    public void FormatValue_DateTime()
    {
        var attr = new FixedTimeAttribute(1, 6);
        var date = new DateTime(2023, 12, 31, 23, 59, 58);
        Assert.AreEqual("235958", attr.FormatValue(date, _culture));
    }

    [TestMethod]
    public void FormatValue_Null_ReturnsEmptyPadded()
    {
        var attr = new FixedTimeAttribute(1, 6);
        Assert.AreEqual("      ", attr.FormatValue(null, _culture));
    }

    [TestMethod]
    public void Parse_DateTime()
    {
        var attr = new FixedTimeAttribute(1, 6);
        var result = (DateTime)attr.Parse("235958", _culture);
        Assert.AreEqual(23, result.Hour);
        Assert.AreEqual(59, result.Minute);
        Assert.AreEqual(58, result.Second);
    }
}