using DotNetCommons.Text;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotNetCommons.Test.Text;

[TestClass]
public class NumericToolsTests
{
    [TestMethod]
    public void ToDelimitedSequence_Works()
    {
        NumericTools.ToDelimitedSequence(new[] { 1 }).Should().Be("1");
        NumericTools.ToDelimitedSequence(Array.Empty<int>()).Should().Be("");
        NumericTools.ToDelimitedSequence(new[] { 1, 3 }).Should().Be("1 and 3");
        NumericTools.ToDelimitedSequence(new[] { 10, 11, 12 }).Should().Be("10-12");
        NumericTools.ToDelimitedSequence(new[] { 10, 11, 12, 13, 16, 17 }).Should().Be("10-13 and 16-17");
        NumericTools.ToDelimitedSequence(new[] { 3, 9, 4, 5, 10, 15, 19, 23, 22, 21 })
            .Should().Be("3-5, 9-10, 15, 19 and 21-23");
    }

    [TestMethod]
    public void ToOrdinal_Works()
    {
        NumericTools.ToOrdinal(null).Should().Be("");
        NumericTools.ToOrdinal(-1).Should().Be("-1");
        NumericTools.ToOrdinal(0).Should().Be("0");
        NumericTools.ToOrdinal(1).Should().Be("1st");
        NumericTools.ToOrdinal(2).Should().Be("2nd");
        NumericTools.ToOrdinal(3).Should().Be("3rd");
        NumericTools.ToOrdinal(4).Should().Be("4th");
        NumericTools.ToOrdinal(5).Should().Be("5th");
        NumericTools.ToOrdinal(6).Should().Be("6th");
        NumericTools.ToOrdinal(7).Should().Be("7th");
        NumericTools.ToOrdinal(8).Should().Be("8th");
        NumericTools.ToOrdinal(9).Should().Be("9th");

        NumericTools.ToOrdinal(10).Should().Be("10th");
        NumericTools.ToOrdinal(11).Should().Be("11th");
        NumericTools.ToOrdinal(12).Should().Be("12th");
        NumericTools.ToOrdinal(13).Should().Be("13th");
        NumericTools.ToOrdinal(14).Should().Be("14th");
        NumericTools.ToOrdinal(15).Should().Be("15th");
        
        NumericTools.ToOrdinal(20).Should().Be("20th");
        NumericTools.ToOrdinal(21).Should().Be("21st");
        NumericTools.ToOrdinal(22).Should().Be("22nd");
        NumericTools.ToOrdinal(23).Should().Be("23rd");
        NumericTools.ToOrdinal(24).Should().Be("24th");
        NumericTools.ToOrdinal(25).Should().Be("25th");
        
        NumericTools.ToOrdinal(30).Should().Be("30th");
        NumericTools.ToOrdinal(31).Should().Be("31st");
        NumericTools.ToOrdinal(32).Should().Be("32nd");
        NumericTools.ToOrdinal(33).Should().Be("33rd");
        NumericTools.ToOrdinal(34).Should().Be("34th");
        NumericTools.ToOrdinal(35).Should().Be("35th");

        NumericTools.ToOrdinal(100).Should().Be("100th");
        NumericTools.ToOrdinal(101).Should().Be("101st");
        NumericTools.ToOrdinal(102).Should().Be("102nd");
        NumericTools.ToOrdinal(103).Should().Be("103rd");
        NumericTools.ToOrdinal(104).Should().Be("104th");
        NumericTools.ToOrdinal(105).Should().Be("105th");
    }
}
