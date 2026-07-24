using System.Numerics;
using DotNetCommons.AI;
using FluentAssertions;

namespace DotNetCommonTests.AI;

[TestClass]
public class DoubleVectorTests
{
    [TestMethod]
    public void Dot_UsesSimdChunksAndScalarTailOnce()
    {
        var leftValues = CreateValues(Vector<double>.Count * 2 + 3, 1.25);
        var rightValues = CreateValues(Vector<double>.Count * 2 + 3, -2.5);
        var left = new DoubleVector(leftValues);
        var right = new DoubleVector(rightValues);
        var expected = leftValues.Select((t, i) => t * rightValues[i]).Sum();

        left.Dot(right).Should().BeApproximately(expected, 0.000000000001);
    }

    [TestMethod]
    public void Magnitude_UsesSimdChunksAndScalarTailOnce()
    {
        var values = CreateValues(Vector<double>.Count * 2 + 3, -3.75);
        var vector = new DoubleVector(values);
        var expected = Math.Sqrt(values.Select(x => x * x).Sum());

        vector.Magnitude().Should().BeApproximately(expected, 0.000000000001);
    }

    [TestMethod]
    public void CosineSimilarity_WorksForSimdSizedVectors()
    {
        var leftValues = CreateValues(Vector<double>.Count + 5, 0.5);
        var rightValues = CreateValues(Vector<double>.Count + 5, 3.25);
        var left = new DoubleVector(leftValues);
        var right = new DoubleVector(rightValues);
        var dot = leftValues.Select((t, i) => t * rightValues[i]).Sum();
        var leftMagnitude = Math.Sqrt(leftValues.Select(x => x * x).Sum());
        var rightMagnitude = Math.Sqrt(rightValues.Select(x => x * x).Sum());

        left.CosineSimilarity(right).Should().BeApproximately(dot / (leftMagnitude * rightMagnitude), 0.000000000001);
    }

    [TestMethod]
    public void Normalize_WorksForSimdChunksAndScalarTail()
    {
        var values = CreateValues(Vector<double>.Count + 3, 2.5);
        var vector = new DoubleVector(values);
        var magnitude = Math.Sqrt(values.Select(x => x * x).Sum());
        var expected = values.Select(x => x / magnitude).ToArray();

        vector.Normalize().Values.ToArray().Should().Equal(expected, (actual, expectedValue) => Math.Abs(actual - expectedValue) < 0.000000000001);
    }

    [TestMethod]
    public void Normalize_ZeroVectorReturnsSameInstance()
    {
        var vector = new DoubleVector(0.0, 0.0, 0.0);

        vector.Normalize().Should().BeSameAs(vector);
    }

    [TestMethod]
    public void Operators_WorkForSimdChunksAndScalarTail()
    {
        var leftValues = CreateValues(Vector<double>.Count + 3, 1.5);
        var rightValues = CreateValues(Vector<double>.Count + 3, -4.25);
        var left = new DoubleVector(leftValues);
        var right = new DoubleVector(rightValues);

        (left + right).Values.ToArray().Should().Equal(leftValues.Select((t, i) => t + rightValues[i]));
        (left - right).Values.ToArray().Should().Equal(leftValues.Select((t, i) => t - rightValues[i]));
        (left * 2.5).Values.ToArray().Should().Equal(leftValues.Select(x => x * 2.5));
        (left / 3.0).Values.ToArray().Should().Equal(leftValues.Select(x => x / 3.0));
    }

    [TestMethod]
    public void ToFloat_WorksForSimdChunksAndScalarTail()
    {
        var values = CreateValues(Vector<double>.Count + 3, -1.5);
        var vector = new DoubleVector(values);

        vector.ToFloat().Values.ToArray().Should().Equal(values.Select(x => (float)x));
    }

    [TestMethod]
    public void DimensionMismatch_Throws()
    {
        var left = new DoubleVector(1.0, 2.0, 3.0);
        var right = new DoubleVector(1.0, 2.0);

        left.Invoking(x => x.Dot(right)).Should().Throw<ArgumentException>();
        left.Invoking(x => x.CosineSimilarity(right)).Should().Throw<ArgumentException>();
        new Action(() => _ = left + right).Should().Throw<ArgumentException>();
        new Action(() => _ = left - right).Should().Throw<ArgumentException>();
    }

    [TestMethod]
    public void IndexLengthAndToString_Work()
    {
        var vector = new DoubleVector(1.0, 2.5, -3.0);

        vector.Length.Should().Be(3);
        vector[1].Should().Be(2.5);
        vector.ToString().Should().Be("[1, 2.5, -3]");
    }

    private static double[] CreateValues(int length, double offset)
    {
        return Enumerable.Range(0, length)
            .Select(i => offset + ((i % 7) - 3) * 0.75 + i * 0.125)
            .ToArray();
    }
}
