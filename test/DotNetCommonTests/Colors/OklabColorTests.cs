using DotNetCommons.Colors;
using FluentAssertions;

namespace DotNetCommonTests.Colors;

[TestClass]
public class OklabColorTests
{
    private const double Precision = 0.000001;

    [TestMethod]
    public void SetLightness_Works()
    {
        var color = new OklabColor();

        color.Lightness = 0.5;
        color.Lightness.Should().BeApproximately(0.5, Precision);
        color.Lightness = 0;
        color.Lightness.Should().BeApproximately(0, Precision);
        color.Lightness = -0.5;
        color.Lightness.Should().BeApproximately(0, Precision);
        color.Lightness = 1;
        color.Lightness.Should().BeApproximately(1, Precision);
        color.Lightness = 1.5;
        color.Lightness.Should().BeApproximately(1, Precision);
        color.Lightness = double.NaN;
        color.Lightness.Should().BeApproximately(0, Precision);
    }

    [TestMethod]
    public void SetA_Works()
    {
        var color = new OklabColor();

        color.A = 0.25;
        color.A.Should().BeApproximately(0.25, Precision);
        color.A = -0.25;
        color.A.Should().BeApproximately(-0.25, Precision);
        color.A = double.NaN;
        color.A.Should().BeApproximately(0, Precision);
        color.A = double.PositiveInfinity;
        color.A.Should().BeApproximately(0, Precision);
    }

    [TestMethod]
    public void SetB_Works()
    {
        var color = new OklabColor();

        color.B = 0.25;
        color.B.Should().BeApproximately(0.25, Precision);
        color.B = -0.25;
        color.B.Should().BeApproximately(-0.25, Precision);
        color.B = double.NaN;
        color.B.Should().BeApproximately(0, Precision);
        color.B = double.NegativeInfinity;
        color.B.Should().BeApproximately(0, Precision);
    }

    [TestMethod]
    public void SetAlpha_Works()
    {
        var color = new OklabColor();

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
