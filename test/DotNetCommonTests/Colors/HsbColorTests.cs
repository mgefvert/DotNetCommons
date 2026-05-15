using DotNetCommons.Colors;
using FluentAssertions;

namespace DotNetCommonTests.Colors;

[TestClass]
public class HsbColorTests
{
    private const double Precision = 0.000001;

    [TestMethod]
    public void SetHue_Works()
    {
        var color = new HsbColor();

        color.Hue = 180;
        color.Hue.Should().BeApproximately(180, Precision);
        color.Hue = 0;
        color.Hue.Should().BeApproximately(0, Precision);
        color.Hue = -3;
        color.Hue.Should().BeApproximately(357, Precision);
        color.Hue = 720;
        color.Hue.Should().BeApproximately(0, Precision);
        color.Hue = 380;
        color.Hue.Should().BeApproximately(20, Precision);
    }

    [TestMethod]
    public void SetHue_WithNonFiniteValue_ResetsToZero()
    {
        var color = new HsbColor();

        color.Hue = double.NaN;
        color.Hue.Should().BeApproximately(0, Precision);
        color.Hue = double.PositiveInfinity;
        color.Hue.Should().BeApproximately(0, Precision);
        color.Hue = double.NegativeInfinity;
        color.Hue.Should().BeApproximately(0, Precision);
    }

    [TestMethod]
    public void SetSaturation_Works()
    {
        var color = new HsbColor();

        color.Saturation = 50;
        color.Saturation.Should().BeApproximately(50, Precision);
        color.Saturation = 0;
        color.Saturation.Should().BeApproximately(0, Precision);
        color.Saturation = -50;
        color.Saturation.Should().BeApproximately(0, Precision);
        color.Saturation = 100;
        color.Saturation.Should().BeApproximately(100, Precision);
        color.Saturation = 150;
        color.Saturation.Should().BeApproximately(100, Precision);
        color.Saturation = double.NaN;
        color.Saturation.Should().BeApproximately(0, Precision);
    }

    [TestMethod]
    public void SetBrightness_Works()
    {
        var color = new HsbColor();

        color.Brightness = 50;
        color.Brightness.Should().BeApproximately(50, Precision);
        color.Brightness = 0;
        color.Brightness.Should().BeApproximately(0, Precision);
        color.Brightness = -50;
        color.Brightness.Should().BeApproximately(0, Precision);
        color.Brightness = 100;
        color.Brightness.Should().BeApproximately(100, Precision);
        color.Brightness = 150;
        color.Brightness.Should().BeApproximately(100, Precision);
        color.Brightness = double.NaN;
        color.Brightness.Should().BeApproximately(0, Precision);
    }

    [TestMethod]
    public void SetAlpha_Works()
    {
        var color = new HsbColor();

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
        color.Alpha = double.NaN;
        color.Alpha.Should().BeApproximately(0, Precision);
    }
}
