using DotNetCommons.WinForms.Graphics;

namespace DotNetCommonTests.WinForms.Graphics;

[TestClass]
public class ImageProcessorTest
{
    [TestMethod]
    public void TestScaleToFit()
    {
        var size = new Size(3344, 2210);
        var result = BitmapOperations.ScaleToFit(size, new Size(640, 480));

        Assert.AreEqual(640, result.Width);
        Assert.AreEqual(423, result.Height);
    }

    [TestMethod]
    public void TestScaleToCover()
    {
        var size = new Size(3344, 2210);
        var result = BitmapOperations.ScaleToCover(size, new Size(640, 480));

        Assert.AreEqual(726, result.Width);
        Assert.AreEqual(480, result.Height);
    }
}