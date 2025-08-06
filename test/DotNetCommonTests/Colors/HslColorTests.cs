using System.Drawing;
using DotNetCommons.Colors;
using FluentAssertions;

namespace DotNetCommonTests.Colors;

[TestClass]
public class HslColorTests
{
    private const double Precision = 0.000001;
    
    [TestMethod]
    public void SetHue_Works()
    {
        var color = new HslColor();

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
    public void SetSaturation_Works()
    {
        var color = new HslColor();
        
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
    }

    [TestMethod]
    public void SetLightness_Works()
    {
        var color = new HslColor();
        
        color.Lightness = 50;
        color.Lightness.Should().BeApproximately(50, Precision);
        color.Lightness = 0;
        color.Lightness.Should().BeApproximately(0, Precision);
        color.Lightness = -50;
        color.Lightness.Should().BeApproximately(0, Precision);
        color.Lightness = 100;
        color.Lightness.Should().BeApproximately(100, Precision);
        color.Lightness = 150;
        color.Lightness.Should().BeApproximately(100, Precision);
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
    public void ToGrayscale_Works()
    {
        var color = new RgbColor(Color.Red).ToGrayscale();
        color.Value.Should().BeApproximately(0.299 * 255, Precision);

        color = new RgbColor(Color.Blue).ToGrayscale();
        color.Value.Should().BeApproximately(0.114 * 255, Precision);

        color = new RgbColor(Color.White).ToGrayscale();
        color.Value.Should().BeApproximately(255, Precision);
    }

    [TestMethod]
    public void ToHSL_Works()
    {
        var color = new RgbColor(Color.Red);
        var hsl = color.ToHsl();
        hsl.Hue.Should().BeApproximately(0, Precision);
        hsl.Saturation.Should().BeApproximately(100, Precision);
        hsl.Lightness.Should().BeApproximately(50, Precision);
        var rgb = hsl.ToRgb();
        rgb.Red.Should().BeApproximately(255, Precision);
        rgb.Green.Should().BeApproximately(0, Precision);
        rgb.Blue.Should().BeApproximately(0, Precision);

        color = new RgbColor(Color.Blue);
        hsl = color.ToHsl();
        hsl.Hue.Should().BeApproximately(240, Precision);
        hsl.Saturation.Should().BeApproximately(100, Precision);
        hsl.Lightness.Should().BeApproximately(50, Precision);
        rgb = hsl.ToRgb();
        rgb.Red.Should().BeApproximately(0, Precision);
        rgb.Green.Should().BeApproximately(0, Precision);
        rgb.Blue.Should().BeApproximately(255, Precision);

        color = new RgbColor(128, 192, 255);
        hsl = color.ToHsl();
        hsl.Hue.Should().BeApproximately(210, 0.5);
        hsl.Saturation.Should().BeApproximately(100, Precision);
        hsl.Lightness.Should().BeApproximately(75.1, 0.1);
        rgb = hsl.ToRgb();
        rgb.Red.Should().BeApproximately(128, Precision);
        rgb.Green.Should().BeApproximately(192, Precision);
        rgb.Blue.Should().BeApproximately(255, Precision);
    }
}