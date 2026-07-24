using System.Numerics;
using System.Text.Json.Serialization;

namespace DotNetCommons.AI;

[JsonConverter(typeof(DoubleVectorConverter))]
public class DoubleVector
{
    private readonly double[] _values;

    public int Length => _values.Length;
    public ReadOnlySpan<double> Values => _values;
    public double this[int index] => _values[index];

    public DoubleVector(params double[] values)
    {
        ArgumentNullException.ThrowIfNull(values);
        _values = values;
    }

    public double CosineSimilarity(DoubleVector other)
    {
        return Dot(other) / (Magnitude() * other.Magnitude());
    }

    public double Dot(DoubleVector other)
    {
        CheckDimensions(this, other);

        var sum         = 0.0;
        var simd        = Vector<double>.Count;
        var accumulator = Vector<double>.Zero;

        var i = 0;
        for (; i <= Length - simd; i += simd)
        {
            var a = new Vector<double>(_values, i);
            var b = new Vector<double>(other._values, i);
            accumulator += a * b;
        }

        for (var j = 0; j < simd; j++)
            sum += accumulator[j];

        for (; i < Length; i++)
            sum += _values[i] * other._values[i];

        return sum;
    }

    public double Magnitude()
    {
        var sum         = 0.0;
        var simd        = Vector<double>.Count;
        var accumulator = Vector<double>.Zero;

        var i = 0;
        for (; i <= Length - simd; i += simd)
        {
            var v = new Vector<double>(_values, i);
            accumulator += v * v;
        }

        for (var j = 0; j < simd; j++)
            sum += accumulator[j];

        for (; i < Length; i++)
            sum += _values[i] * _values[i];

        return Math.Sqrt(sum);
    }

    public DoubleVector Normalize()
    {
        var magnitude = Magnitude();
        if (magnitude == 0)
            return this;

        var result       = new double[Length];
        var scalar       = 1.0 / magnitude;
        var simd         = Vector<double>.Count;
        var scalarVector = new Vector<double>(scalar);

        var i = 0;
        for (; i <= Length - simd; i += simd)
        {
            var v = new Vector<double>(_values, i);
            (v * scalarVector).CopyTo(result, i);
        }

        for (; i < Length; i++)
            result[i] = _values[i] * scalar;

        return new DoubleVector(result);
    }

    public static DoubleVector operator +(DoubleVector a, DoubleVector b)
    {
        CheckDimensions(a, b);
        var result = new double[a.Length];
        var simd   = Vector<double>.Count;

        var i = 0;
        for (; i <= a.Length - simd; i += simd)
        {
            var va = new Vector<double>(a._values, i);
            var vb = new Vector<double>(b._values, i);
            (va + vb).CopyTo(result, i);
        }

        for (; i < result.Length; i++)
            result[i] = a[i] + b[i];

        return new DoubleVector(result);
    }

    public static DoubleVector operator -(DoubleVector a, DoubleVector b)
    {
        CheckDimensions(a, b);
        var result = new double[a.Length];
        var simd   = Vector<double>.Count;

        var i = 0;
        for (; i <= a.Length - simd; i += simd)
        {
            var va = new Vector<double>(a._values, i);
            var vb = new Vector<double>(b._values, i);
            (va - vb).CopyTo(result, i);
        }

        for (; i < result.Length; i++)
            result[i] = a[i] - b[i];

        return new DoubleVector(result);
    }

    public static DoubleVector operator *(DoubleVector v, double scalar)
    {
        var result       = new double[v.Length];
        var simd         = Vector<double>.Count;
        var scalarVector = new Vector<double>(scalar);

        var i = 0;
        for (; i <= v.Length - simd; i += simd)
        {
            var vv = new Vector<double>(v._values, i);
            (vv * scalarVector).CopyTo(result, i);
        }

        for (; i < result.Length; i++)
            result[i] = v[i] * scalar;

        return new DoubleVector(result);
    }

    public static DoubleVector operator /(DoubleVector v, double scalar)
    {
        return v * (1.0 / scalar);
    }

    public FloatVector ToFloat()
    {
        var result = new float[_values.Length];
        var simd   = Vector<double>.Count;

        var i = 0;
        for (; i <= _values.Length - simd; i += simd)
        {
            var v = new Vector<double>(_values, i);
            for (var j = 0; j < simd && i + j < _values.Length; j++)
                result[i + j] = (float)v[j];
        }

        for (; i < result.Length; i++)
            result[i] = (float)_values[i];

        return new FloatVector(result);
    }

    public override string ToString()
    {
        return $"[{string.Join(", ", _values)}]";
    }

    private static void CheckDimensions(DoubleVector a, DoubleVector b)
    {
        if (a.Length != b.Length)
            throw new ArgumentException("Vector dimensions must match.");
    }

    public static implicit operator double[](DoubleVector vector) => vector._values;
}
