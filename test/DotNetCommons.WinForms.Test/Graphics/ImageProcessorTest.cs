using System;
using System.Drawing;
using DotNetCommons.WinForms.Graphics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotNetCommons.WinForms.Test.Graphics
{
    [TestClass]
    public class ImageProcessorTest
    {
        [TestMethod]
        public void TestScaleMax()
        {
            var size = new Size(3344, 2210);
            var result = ImageProcessor.ScaleMax(size, new Size(640, 480));

            Assert.AreEqual(640, result.Width);
            Assert.AreEqual(423, result.Height);
        }

        [TestMethod]
        public void TestScaleMin()
        {
            var size = new Size(3344, 2210);
            var result = ImageProcessor.ScaleMin(size, new Size(640, 480));

            Assert.AreEqual(726, result.Width);
            Assert.AreEqual(480, result.Height);
        }
    }
}
