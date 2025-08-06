using DotNetCommons.Colors;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotNetCommonTests.Colors;

[TestClass]
public class GrayscaleColorTests
{
    private const double Precision = 0.000001;
    
    [TestMethod]
    public void SetValue_Works()
    {
        var color = new GrayscaleColor();

        color.Value = 128;
        color.Value.Should().BeApproximately(128, Precision);
        color.Value = 0;
        color.Value.Should().BeApproximately(0, Precision);
        color.Value = -128;
        color.Value.Should().BeApproximately(0, Precision);
        color.Value = 255;
        color.Value.Should().BeApproximately(255, Precision);
        color.Value = 500;
        color.Value.Should().BeApproximately(255, Precision);
    }

    [TestMethod]
    public void SetAlpha_Works()
    {
        var color = new HslColor();
        
        color.Alpha = 50;
        color.Alpha.Should().BeApproximately(50, Precision);
        color.Alpha = 0;
        color.Alpha.Should().BeApproximately(0, Precision);
        color.Alpha = -50;
        color.Alpha.Should().BeApproximately(0, Precision);
        color.Alpha = 255;
        color.Alpha.Should().BeApproximately(255, Precision);
        color.Alpha = 500;
        color.Alpha.Should().BeApproximately(255, Precision);
    }

    [TestMethod]
    public void ToHSL_Works()
    {
        var hsl = new GrayscaleColor(0).ToHsl();
        hsl.Hue.Should().BeApproximately(0, Precision);
        hsl.Saturation.Should().BeApproximately(0, Precision);
        hsl.Lightness.Should().BeApproximately(0, Precision);

        hsl = new GrayscaleColor(128).ToHsl();
        hsl.Hue.Should().BeApproximately(0, Precision);
        hsl.Saturation.Should().BeApproximately(0, Precision);
        hsl.Lightness.Should().BeApproximately(50, 0.5);

        hsl = new GrayscaleColor(255).ToHsl();
        hsl.Hue.Should().BeApproximately(0, Precision);
        hsl.Saturation.Should().BeApproximately(0, Precision);
        hsl.Lightness.Should().BeApproximately(100, Precision);
    }
}