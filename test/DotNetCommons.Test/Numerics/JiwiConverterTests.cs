using System.Text;
using DotNetCommons.Numerics;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotNetCommons.Test.Numerics;

[TestClass]
public class JiwiConverterTests
{
    [TestMethod]
    public void ToJiwi_Works()
    {
        var s = JiwiConverter.ToJiwi(4711);
        s.Should().Be("mafy8");
    }

    [TestMethod]
    public void FromJiwi_Works()
    {
        var n = JiwiConverter.FromJiwi("mafy8");
        n.Should().Be(4711);
    }

    [TestMethod]
    public void ConvertBackAndForth_Works()
    {
        for (var i = 0; i < 1_000; i++)
        {
            var s = JiwiConverter.ToJiwi(i);
            var n = JiwiConverter.FromJiwi(s);
            n.Should().Be(i);
        }

        for (var i = 1L; i < 1_000_000_000_000L; i *= 7)
        {
            var s = JiwiConverter.ToJiwi(i);
            var n = JiwiConverter.FromJiwi(s);
            n.Should().Be(i);
        }
    }

    [TestMethod]
    public void MakeNumbers()
    {
        var buf = new StringBuilder();
        buf.AppendLine($"{JiwiConverter.SyllableCount} syllables");

        for (var i = 0L; i < 100_000; i++)
        {
            var num = i * 139147;
            var jiwi = JiwiConverter.ToJiwi(num);
            buf.AppendLine($"{num} = {jiwi}");
        }

        var file = new FileInfo("jiwi.txt");

        File.WriteAllText(file.FullName, buf.ToString());
        Console.WriteLine(file.FullName);
    }
}