using DotNetCommons.Text;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotNetCommons.Test.Text;

[TestClass]
public class RomanNumeralsTests
{
    [TestMethod] public void Parse_1() => RomanNumerals.Parse("I").Should().Be(1);
    [TestMethod] public void Parse_2() => RomanNumerals.Parse("II").Should().Be(2);
    [TestMethod] public void Parse_3() => RomanNumerals.Parse("III").Should().Be(3);
    [TestMethod] public void Parse_4() => RomanNumerals.Parse("IV").Should().Be(4);
    [TestMethod] public void Parse_5() => RomanNumerals.Parse("V").Should().Be(5);
    [TestMethod] public void Parse_6() => RomanNumerals.Parse("VI").Should().Be(6);
    [TestMethod] public void Parse_7() => RomanNumerals.Parse("VII").Should().Be(7);
    [TestMethod] public void Parse_8() => RomanNumerals.Parse("VIII").Should().Be(8);
    [TestMethod] public void Parse_9() => RomanNumerals.Parse("IX").Should().Be(9);
    [TestMethod] public void Parse_10() => RomanNumerals.Parse("X").Should().Be(10);
    [TestMethod] public void Parse_49() => RomanNumerals.Parse("XLIX").Should().Be(49);
    [TestMethod] public void Parse_50() => RomanNumerals.Parse("L").Should().Be(50);
    [TestMethod] public void Parse_90() => RomanNumerals.Parse("XC").Should().Be(90);
    [TestMethod] public void Parse_100() => RomanNumerals.Parse("C").Should().Be(100);
    [TestMethod] public void Parse_400() => RomanNumerals.Parse("CD").Should().Be(400);
    [TestMethod] public void Parse_500() => RomanNumerals.Parse("D").Should().Be(500);
    [TestMethod] public void Parse_900() => RomanNumerals.Parse("CM").Should().Be(900);
    [TestMethod] public void Parse_1000() => RomanNumerals.Parse("M").Should().Be(1000);
    [TestMethod] public void Parse_11984() => RomanNumerals.Parse("MMMMMMMMMMMCMLXXXIV").Should().Be(11984);

    [TestMethod] public void Render1() => RomanNumerals.Render(1).Should().Be("I");
    [TestMethod] public void Render2() => RomanNumerals.Render(2).Should().Be("II");
    [TestMethod] public void Render3() => RomanNumerals.Render(3).Should().Be("III");
    [TestMethod] public void Render4() => RomanNumerals.Render(4).Should().Be("IV");
    [TestMethod] public void Render5() => RomanNumerals.Render(5).Should().Be("V");
    [TestMethod] public void Render9() => RomanNumerals.Render(9).Should().Be("IX");
    [TestMethod] public void Render10() => RomanNumerals.Render(10).Should().Be("X");
    [TestMethod] public void Render49() => RomanNumerals.Render(49).Should().Be("XLIX");
    [TestMethod] public void Render50() => RomanNumerals.Render(50).Should().Be("L");
    [TestMethod] public void Render100() => RomanNumerals.Render(100).Should().Be("C");
    [TestMethod] public void Render400() => RomanNumerals.Render(400).Should().Be("CD");
    [TestMethod] public void Render500() => RomanNumerals.Render(500).Should().Be("D");
    [TestMethod] public void Render900() => RomanNumerals.Render(900).Should().Be("CM");
    [TestMethod] public void Render1000() => RomanNumerals.Render(1000).Should().Be("M");
    [TestMethod] public void Render11984() => RomanNumerals.Render(11984).Should().Be("MMMMMMMMMMMCMLXXXIV");
}
