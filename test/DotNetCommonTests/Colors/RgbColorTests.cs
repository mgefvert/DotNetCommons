using System.Drawing;
using DotNetCommons.Colors;
using FluentAssertions;

namespace DotNetCommonTests.Colors;

[TestClass]
public class RgbColorTests
{
    [TestMethod]
    public void SetRed_Works()
    {
        var color = new RgbColor();
        
        color.Red = 128;
        color.Red.Should().Be(128);
        color.Red = 0;
        color.Red.Should().Be(0);
        color.Red = -128;
        color.Red.Should().Be(0);
        color.Red = 255;
        color.Red.Should().Be(255);
        color.Red = 500;
        color.Red.Should().Be(255);
    }

    [TestMethod]
    public void SetGreen_Works()
    {
        var color = new RgbColor();

        color.Green = 128;
        color.Green.Should().Be(128);
        color.Green = 0;
        color.Green.Should().Be(0);
        color.Green = -128;
        color.Green.Should().Be(0);
        color.Green = 255;
        color.Green.Should().Be(255);
        color.Green = 500;
        color.Green.Should().Be(255);
    }

    [TestMethod]
    public void SetBlue_Works()
    {
        var color = new RgbColor();
        
        color.Blue = 128;
        color.Blue.Should().Be(128);
        color.Blue = 0;
        color.Blue.Should().Be(0);
        color.Blue = -128;
        color.Blue.Should().Be(0);
        color.Blue = 255;
        color.Blue.Should().Be(255);
        color.Blue = 500;
        color.Blue.Should().Be(255);
    }

    [TestMethod]
    public void SetAlpha_Works()
    {
        var color = new RgbColor();
        
        color.Alpha = 128;
        color.Alpha.Should().Be(128);
        color.Alpha = 0;
        color.Alpha.Should().Be(0);
        color.Alpha = -128;
        color.Alpha.Should().Be(0);
        color.Alpha = 255;
        color.Alpha.Should().Be(255);
        color.Alpha = 500;
        color.Alpha.Should().Be(255);
    }

    [TestMethod]
    public void FromBytes_Works()
    {
        var color = new RgbColor(0xFF, 0xCC, 0x22, 0x77);
        color.Red.Should().Be(0xFF);
        color.Green.Should().Be(0xCC);
        color.Blue.Should().Be(0x22);
        color.Alpha.Should().Be(0x77);
    }

    [TestMethod]
    public void FromHex_Works()
    {
        var color = RgbColor.FromHex(null!);
        color.Should().BeNull();

        color = RgbColor.FromHex("");
        color.Should().BeNull();
        
        color = RgbColor.FromHex("FC2");
        color.Should().NotBeNull();
        color!.Red.Should().Be(0xFF);
        color.Green.Should().Be(0xCC);
        color.Blue.Should().Be(0x22);
        color.Alpha.Should().Be(0xFF);

        color = RgbColor.FromHex("FC27");
        color.Should().NotBeNull();
        color!.Red.Should().Be(0xFF);
        color.Green.Should().Be(0xCC);
        color.Blue.Should().Be(0x22);
        color.Alpha.Should().Be(0x77);

        color = RgbColor.FromHex("FACA2A");
        color.Should().NotBeNull();
        color!.Red.Should().Be(0xFA);
        color.Green.Should().Be(0xCA);
        color.Blue.Should().Be(0x2A);
        color.Alpha.Should().Be(0xFF);

        color = RgbColor.FromHex("FACA2A7A");
        color.Should().NotBeNull();
        color!.Red.Should().Be(0xFA);
        color.Green.Should().Be(0xCA);
        color.Blue.Should().Be(0x2A);
        color.Alpha.Should().Be(0x7A);
    }

    [TestMethod]
    public void FromColor_Works()
    {
        var color = new RgbColor(Color.White);
        color.Red.Should().Be(255);
        color.Green.Should().Be(255);
        color.Blue.Should().Be(255);
        color.Alpha.Should().Be(255);

        color = new RgbColor(Color.Black);
        color.Red.Should().Be(0);
        color.Green.Should().Be(0);
        color.Blue.Should().Be(0);
        color.Alpha.Should().Be(255);

        color = new RgbColor(Color.Transparent);
        color.Alpha.Should().Be(0);

        color = new RgbColor(Color.Blue);
        color.Red.Should().Be(0);
        color.Green.Should().Be(0);
        color.Blue.Should().Be(255);
        color.Alpha.Should().Be(255);

        color = new RgbColor(Color.MediumVioletRed);
        color.Red.Should().Be(0xC7);
        color.Green.Should().Be(0x15);
        color.Blue.Should().Be(0x85);
        color.Alpha.Should().Be(255);
    }

    [TestMethod]
    public void Darken()
    {
        var color = new RgbColor(Color.CadetBlue);
        color.ToHex().Should().Be("#5F9EA0");
        color.Darken(0.2);
        color.ToHex().Should().Be("#4C7E80");
    }

    [TestMethod]
    public void Lighten()
    {
        var color = new RgbColor(Color.CadetBlue);
        color.ToHex().Should().Be("#5F9EA0");
        color.Lighten(0.2);
        color.ToHex().Should().Be("#7FB1B3");
    }

    [TestMethod]
    public void MixIn_Works()
    {
        var color = new RgbColor(Color.Blue);
        var red   = new RgbColor(255, 0, 0);
        var green = new RgbColor(0, 255, 0);
        var blue  = new RgbColor(0, 0, 255);
        var white = new RgbColor(255, 255, 255);
        
        color.Clone().MixIn(red, 0.5).ToHex().Should().Be("#800080");
        color.Clone().MixIn(green, 0.5).ToHex().Should().Be("#008080");
        color.Clone().MixIn(blue, 0.5).ToHex().Should().Be("#0000FF");
        color.Clone().MixIn(white, 0.5).ToHex().Should().Be("#8080FF");

        color.Clone().MixIn(red, 0.75).ToHex().Should().Be("#BF0040");
        color.Clone().MixIn(green, 0.75).ToHex().Should().Be("#00BF40");
        color.Clone().MixIn(blue, 0.75).ToHex().Should().Be("#0000FF");
        color.Clone().MixIn(white, 0.75).ToHex().Should().Be("#BFBFFF");
        
        color.Clone().MixIn(red).ToHex().Should().Be("#FF0000");
        color.Clone().MixIn(green).ToHex().Should().Be("#00FF00");
        color.Clone().MixIn(blue).ToHex().Should().Be("#0000FF");
        color.Clone().MixIn(white).ToHex().Should().Be("#FFFFFF");
    }
    
    [TestMethod]
    public void ToColor_Works()
    {
        var color = new RgbColor(0xFF, 0x80, 0x20, 0x99).ToColor();
        color.R.Should().Be(0xFF);
        color.G.Should().Be(0x80);
        color.B.Should().Be(0x20);
        color.A.Should().Be(0x99);
    }

    [TestMethod]
    public void ToHex_Works()
    {
        new RgbColor(0xFF, 0x80, 0x20, 0x99).ToHex().Should().Be("#FF802099");
        new RgbColor(0xFF, 0x80, 0x20).ToHex().Should().Be("#FF8020");
    }
}