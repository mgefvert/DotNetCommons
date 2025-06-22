using DotNetCommons.Numerics;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotNetCommons.Test.Numerics;

[TestClass]
public class LcgRandomizerTest
{
    [TestMethod]
    public void Next_Works()
    {
        var lcg = new LcgRandomizer(4711);
        var series = Enumerable.Range(1, 20).Select(_ => lcg.Next(0, 1000)).ToArray();
        series.Should().BeEquivalentTo(
            [44, 989, 26, 685, 328, 449, 422, 641, 572, 845, 934, 155, 336, 697, 70, 255, 316, 709, 910, 149]
        );
    }
}