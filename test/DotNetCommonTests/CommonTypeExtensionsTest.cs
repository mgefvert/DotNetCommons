using DotNetCommons;

namespace DotNetCommonTests;

[TestClass]
public class CommonTypeExtensionsTest
{
    [TestMethod]
    public void TestDescendantOfOrEqual()
    {
        Assert.IsFalse(typeof(string).DescendantOfOrEqual(typeof(int)));
        Assert.IsTrue(typeof(string).DescendantOfOrEqual(typeof(object)));
        Assert.IsTrue(typeof(string).DescendantOfOrEqual(typeof(string)));
        Assert.IsTrue(typeof(StreamReader).DescendantOfOrEqual(typeof(StreamReader)));
        Assert.IsTrue(typeof(StreamReader).DescendantOfOrEqual(typeof(TextReader)));
        Assert.IsTrue(typeof(StreamReader).DescendantOfOrEqual(typeof(object)));
        Assert.IsFalse(typeof(StreamReader).DescendantOfOrEqual(typeof(StreamWriter)));
    }

    [TestMethod]
    public void IsNullable()
    {
        Assert.IsFalse(typeof(string).IsNullable());
        Assert.IsFalse(typeof(int).IsNullable());
        Assert.IsTrue(typeof(int?).IsNullable());
        Assert.IsFalse(typeof(DateTime).IsNullable());
        Assert.IsTrue(typeof(DateTime?).IsNullable());
    }

    [TestMethod]
    public void IsNumeric()
    {
        Assert.IsTrue(typeof(int).IsNumeric());
        Assert.IsTrue(typeof(byte).IsNumeric());
        Assert.IsTrue(typeof(sbyte).IsNumeric());
        Assert.IsTrue(typeof(short).IsNumeric());
        Assert.IsTrue(typeof(ushort).IsNumeric());
        Assert.IsTrue(typeof(int).IsNumeric());
        Assert.IsTrue(typeof(uint).IsNumeric());
        Assert.IsTrue(typeof(long).IsNumeric());
        Assert.IsTrue(typeof(ulong).IsNumeric());
        Assert.IsTrue(typeof(decimal).IsNumeric());
        Assert.IsTrue(typeof(float).IsNumeric());
        Assert.IsTrue(typeof(double).IsNumeric());
        Assert.IsFalse(typeof(DateTime).IsNumeric());
        Assert.IsFalse(typeof(string).IsNumeric());
        Assert.IsFalse(typeof(object).IsNumeric());
    }
}