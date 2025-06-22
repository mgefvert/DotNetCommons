using DotNetCommons.Colors;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotNetCommons.Test.Colors;

[TestClass]
public class ColorConversionTests
{
    private readonly RgbColor _black = new(0, 0, 0);
    private readonly RgbColor _red = new(255, 0, 0);
    private readonly RgbColor _green = new(0, 255, 0);
    private readonly RgbColor _blue = new(0, 0, 255);
    private readonly RgbColor _yellow = new(255, 255, 0);
    private readonly RgbColor _cyan = new(0, 255, 255);
    private readonly RgbColor _white = new(255, 255, 255);
    private readonly RgbColor _lime = new(48, 205, 49);

    [TestMethod]
    public void GrayscaleToHsb()
    {
        Check(new GrayscaleColor(0).ToHsb()).Should().BeEquivalentTo([0, 0, 0]);
        Check(new GrayscaleColor(128).ToHsb()).Should().BeEquivalentTo([0, 0, 50]);
        Check(new GrayscaleColor(192).ToHsb()).Should().BeEquivalentTo([0, 0, 75]);
        Check(new GrayscaleColor(255).ToHsb()).Should().BeEquivalentTo([0, 0, 100]);
    }

    [TestMethod]
    public void GrayscaleToHsl()
    {
        Check(new GrayscaleColor(0).ToHsl()).Should().BeEquivalentTo([0, 0, 0]);
        Check(new GrayscaleColor(128).ToHsl()).Should().BeEquivalentTo([0, 0, 50]);
        Check(new GrayscaleColor(192).ToHsl()).Should().BeEquivalentTo([0, 0, 75]);
        Check(new GrayscaleColor(255).ToHsl()).Should().BeEquivalentTo([0, 0, 100]);
    }

    [TestMethod]
    public void GrayscaleToRgb()
    {
        Check(new GrayscaleColor(0).ToRgb()).Should().BeEquivalentTo([0, 0, 0]);
        Check(new GrayscaleColor(128).ToRgb()).Should().BeEquivalentTo([128, 128, 128]);
        Check(new GrayscaleColor(192).ToRgb()).Should().BeEquivalentTo([192, 192, 192]);
        Check(new GrayscaleColor(255).ToRgb()).Should().BeEquivalentTo([255, 255, 255]);
    }

    [TestMethod]
    public void HexToRgb()
    {
        RgbColor.FromHex("").Should().BeNull();
        RgbColor.FromHex(null!).Should().BeNull();

        Check(RgbColor.FromHex("123")!).Should().BeEquivalentTo([0x11, 0x22, 0x33]);
        Check(RgbColor.FromHex("9CF")!).Should().BeEquivalentTo([0x99, 0xCC, 0xFF]);
        Check(RgbColor.FromHex("#123")!).Should().BeEquivalentTo([0x11, 0x22, 0x33]);
        Check(RgbColor.FromHex("#9CF")!).Should().BeEquivalentTo([0x99, 0xCC, 0xFF]);
        Check(RgbColor.FromHex("102030")!).Should().BeEquivalentTo([0x10, 0x20, 0x30]);
        Check(RgbColor.FromHex("90C0F0")!).Should().BeEquivalentTo([0x90, 0xC0, 0xF0]);
        Check(RgbColor.FromHex("#102030")!).Should().BeEquivalentTo([0x10, 0x20, 0x30]);
        Check(RgbColor.FromHex("#90C0F0")!).Should().BeEquivalentTo([0x90, 0xC0, 0xF0]);
    }

    [TestMethod]
    public void HexToRgb_WithAlpha()
    {
        CheckA(RgbColor.FromHex("123A")!).Should().BeEquivalentTo([0x11, 0x22, 0x33, 0xAA]);
        CheckA(RgbColor.FromHex("9CFA")!).Should().BeEquivalentTo([0x99, 0xCC, 0xFF, 0xAA]);
        CheckA(RgbColor.FromHex("#123A")!).Should().BeEquivalentTo([0x11, 0x22, 0x33, 0xAA]);
        CheckA(RgbColor.FromHex("#9CFA")!).Should().BeEquivalentTo([0x99, 0xCC, 0xFF, 0xAA]);
        CheckA(RgbColor.FromHex("102030A0")!).Should().BeEquivalentTo([0x10, 0x20, 0x30, 0xA0]);
        CheckA(RgbColor.FromHex("90C0F0A0")!).Should().BeEquivalentTo([0x90, 0xC0, 0xF0, 0xA0]);
        CheckA(RgbColor.FromHex("#102030A0")!).Should().BeEquivalentTo([0x10, 0x20, 0x30, 0xA0]);
        CheckA(RgbColor.FromHex("#90C0F0A0")!).Should().BeEquivalentTo([0x90, 0xC0, 0xF0, 0xA0]);
    }

    [TestMethod]
    public void HsbToRgb()
    {
        Check(_red.ToHsb().ToRgb()).Should().BeEquivalentTo(Check(_red));
        Check(_green.ToHsb().ToRgb()).Should().BeEquivalentTo(Check(_green));
        Check(_blue.ToHsb().ToRgb()).Should().BeEquivalentTo(Check(_blue));
        Check(_yellow.ToHsb().ToRgb()).Should().BeEquivalentTo(Check(_yellow));
        Check(_cyan.ToHsb().ToRgb()).Should().BeEquivalentTo(Check(_cyan));
        Check(_black.ToHsb().ToRgb()).Should().BeEquivalentTo(Check(_black));
        Check(_white.ToHsb().ToRgb()).Should().BeEquivalentTo(Check(_white));
        Check(_lime.ToHsb().ToRgb()).Should().BeEquivalentTo(Check(_lime));
    }

    [TestMethod]
    public void HslToRgb()
    {
        Check(_red.ToHsl().ToRgb()).Should().BeEquivalentTo(Check(_red));
        Check(_green.ToHsl().ToRgb()).Should().BeEquivalentTo(Check(_green));
        Check(_blue.ToHsl().ToRgb()).Should().BeEquivalentTo(Check(_blue));
        Check(_yellow.ToHsl().ToRgb()).Should().BeEquivalentTo(Check(_yellow));
        Check(_cyan.ToHsl().ToRgb()).Should().BeEquivalentTo(Check(_cyan));
        Check(_black.ToHsl().ToRgb()).Should().BeEquivalentTo(Check(_black));
        Check(_white.ToHsl().ToRgb()).Should().BeEquivalentTo(Check(_white));
        Check(_lime.ToHsl().ToRgb()).Should().BeEquivalentTo(Check(_lime));
    }

    [TestMethod]
    public void RgbToHex()
    {
        _red.ToHex().Should().Be("#FF0000");
        _green.ToHex().Should().Be("#00FF00");
        _blue.ToHex().Should().Be("#0000FF");
        _yellow.ToHex().Should().Be("#FFFF00");
        _cyan.ToHex().Should().Be("#00FFFF");
        _black.ToHex().Should().Be("#000000");
        _white.ToHex().Should().Be("#FFFFFF");
        _lime.ToHex().Should().Be("#30CD31");
    }

    [TestMethod]
    public void RgbToGrayscale()
    {
        Check(_red.ToGrayscale()).Should().Be(76);
        Check(_green.ToGrayscale()).Should().Be(150);
        Check(_blue.ToGrayscale()).Should().Be(29);
        Check(_yellow.ToGrayscale()).Should().Be(226);
        Check(_cyan.ToGrayscale()).Should().Be(179);
        Check(_black.ToGrayscale()).Should().Be(0);
        Check(_white.ToGrayscale()).Should().Be(255);
        Check(_lime.ToGrayscale()).Should().Be(140);
    }

    [TestMethod]
    public void RgbToHsb()
    {
        Check(_red.ToHsb()).Should().BeEquivalentTo([0, 100, 100]);
        Check(_green.ToHsb()).Should().BeEquivalentTo([120, 100, 100]);
        Check(_blue.ToHsb()).Should().BeEquivalentTo([240, 100, 100]);
        Check(_yellow.ToHsb()).Should().BeEquivalentTo([60, 100, 100]);
        Check(_cyan.ToHsb()).Should().BeEquivalentTo([180, 100, 100]);
        Check(_black.ToHsb()).Should().BeEquivalentTo([0, 0, 0]);
        Check(_white.ToHsb()).Should().BeEquivalentTo([0, 0, 100]);
        Check(_lime.ToHsb()).Should().BeEquivalentTo([120, 77, 80]);
    }

    [TestMethod]
    public void RgbToHsl()
    {
        Check(_red.ToHsl()).Should().BeEquivalentTo([0, 100, 50]);
        Check(_green.ToHsl()).Should().BeEquivalentTo([120, 100, 50]);
        Check(_blue.ToHsl()).Should().BeEquivalentTo([240, 100, 50]);
        Check(_yellow.ToHsl()).Should().BeEquivalentTo([60, 100, 50]);
        Check(_cyan.ToHsl()).Should().BeEquivalentTo([180, 100, 50]);
        Check(_black.ToHsl()).Should().BeEquivalentTo([0, 0, 0]);
        Check(_white.ToHsl()).Should().BeEquivalentTo([0, 0, 100]);
        Check(_lime.ToHsl()).Should().BeEquivalentTo([120, 62, 50]);
    }

    private int Check(GrayscaleColor color) => (int)Math.Round(color.Value);
    private int[] Check(HsbColor color) => [(int)Math.Round(color.Hue), (int)Math.Round(color.Saturation), (int)Math.Round(color.Brightness)];
    private int[] Check(HslColor color) => [(int)Math.Round(color.Hue), (int)Math.Round(color.Saturation), (int)Math.Round(color.Lightness)];
    private int[] Check(RgbColor color) => [(int)Math.Round(color.Red), (int)Math.Round(color.Green), (int)Math.Round(color.Blue)];
    private int[] CheckA(RgbColor color) => [(int)Math.Round(color.Red), (int)Math.Round(color.Green), (int)Math.Round(color.Blue), (int)Math.Round(color.Alpha)];
}