using System.Drawing.Imaging;
using DotNetCommons.WinForms.Graphics;
using FluentAssertions;

namespace DotNetCommonTests.WinForms.Graphics;

[TestClass]
public class BitmapBufferTests
{
    [TestMethod]
    public void TestCreateAndDispose()
    {
        using var bitmap = new Bitmap(100, 100);

        var buffer = bitmap.LockBuffer(ImageLockMode.ReadWrite);

        Assert.IsNotNull(buffer);
        Assert.IsFalse(buffer.IsDisposed);

        buffer.Dispose();
        buffer.IsDisposed.Should().BeTrue();
    }

    [TestMethod]
    public void TestReadPixelData()
    {
        using var bitmap = new Bitmap(1, 1);
        bitmap.SetPixel(0, 0, Color.Red);

        using var buffer = bitmap.LockBuffer(ImageLockMode.ReadOnly);

        buffer.GetColor(0, 0).ToArgb().Should().Be(Color.Red.ToArgb());
    }
}