using System.Numerics;
using DotNetCommons.AI;
using FluentAssertions;

namespace DotNetCommonTests.AI;

[TestClass]
public class FloatVectorTests
{
    [TestMethod]
    public void Dot_UsesSimdChunksAndScalarTailOnce()
    {
        var leftValues = CreateValues(Vector<float>.Count * 2 + 3, 1.25f);
        var rightValues = CreateValues(Vector<float>.Count * 2 + 3, -2.5f);
        var left = new FloatVector(leftValues);
        var right = new FloatVector(rightValues);
        var expected = leftValues.Select((t, i) => t * rightValues[i]).Sum(x => (double)x);

        left.Dot(right).Should().BeApproximately(expected, 0.0001);
    }

    [TestMethod]
    public void Magnitude_UsesSimdChunksAndScalarTailOnce()
    {
        var values = CreateValues(Vector<float>.Count * 2 + 3, -3.75f);
        var vector = new FloatVector(values);
        var expected = Math.Sqrt(values.Select(x => x * x).Sum(x => (double)x));

        vector.Magnitude().Should().BeApproximately(expected, 0.0001);
    }

    [TestMethod]
    public void CosineSimilarity_WorksForSimdSizedVectors()
    {
        var leftValues = CreateValues(Vector<float>.Count + 5, 0.5f);
        var rightValues = CreateValues(Vector<float>.Count + 5, 3.25f);
        var left = new FloatVector(leftValues);
        var right = new FloatVector(rightValues);
        var dot = leftValues.Select((t, i) => t * rightValues[i]).Sum(x => (double)x);
        var leftMagnitude = Math.Sqrt(leftValues.Select(x => x * x).Sum(x => (double)x));
        var rightMagnitude = Math.Sqrt(rightValues.Select(x => x * x).Sum(x => (double)x));

        left.CosineSimilarity(right).Should().BeApproximately(dot / (leftMagnitude * rightMagnitude), 0.0001);
    }

    [TestMethod]
    public void Normalize_WorksForSimdChunksAndScalarTail()
    {
        var values = CreateValues(Vector<float>.Count + 3, 2.5f);
        var vector = new FloatVector(values);
        var magnitude = Math.Sqrt(values.Select(x => x * x).Sum(x => (double)x));
        var expected = values.Select(x => (float)(x / magnitude)).ToArray();

        vector.Normalize().Values.ToArray().Should().Equal(expected, (actual, expectedValue) => Math.Abs(actual - expectedValue) < 0.000001f);
    }

    [TestMethod]
    public void Normalize_ZeroVectorReturnsSameInstance()
    {
        var vector = new FloatVector(0f, 0f, 0f);

        vector.Normalize().Should().BeSameAs(vector);
    }

    [TestMethod]
    public void Operators_WorkForSimdChunksAndScalarTail()
    {
        var leftValues = CreateValues(Vector<float>.Count + 3, 1.5f);
        var rightValues = CreateValues(Vector<float>.Count + 3, -4.25f);
        var left = new FloatVector(leftValues);
        var right = new FloatVector(rightValues);

        (left + right).Values.ToArray().Should().Equal(leftValues.Select((t, i) => t + rightValues[i]));
        (left - right).Values.ToArray().Should().Equal(leftValues.Select((t, i) => t - rightValues[i]));
        (left * 2.5f).Values.ToArray().Should().Equal(leftValues.Select(x => x * 2.5f));
        (left / 2f).Values.ToArray().Should().Equal(leftValues.Select(x => x / 2f));
    }

    [TestMethod]
    public void ToDouble_WorksForSimdChunksAndScalarTail()
    {
        var values = CreateValues(Vector<float>.Count + 3, -1.5f);
        var vector = new FloatVector(values);

        vector.ToDouble().Values.ToArray().Should().Equal(values.Select(x => (double)x));
    }

    [TestMethod]
    public void DimensionMismatch_Throws()
    {
        var left = new FloatVector(1f, 2f, 3f);
        var right = new FloatVector(1f, 2f);

        left.Invoking(x => x.Dot(right)).Should().Throw<ArgumentException>();
        left.Invoking(x => x.CosineSimilarity(right)).Should().Throw<ArgumentException>();
        new Action(() => _ = left + right).Should().Throw<ArgumentException>();
        new Action(() => _ = left - right).Should().Throw<ArgumentException>();
    }

    [TestMethod]
    public void IndexLengthAndToString_Work()
    {
        var vector = new FloatVector(1f, 2.5f, -3f);

        vector.Length.Should().Be(3);
        vector[1].Should().Be(2.5f);
        vector.ToString().Should().Be("[1, 2.5, -3]");
    }

    private static float[] CreateValues(int length, float offset)
    {
        return Enumerable.Range(0, length)
            .Select(i => offset + ((i % 7) - 3) * 0.75f + i * 0.125f)
            .ToArray();
    }
}
