using System.Numerics;
using System.Runtime.InteropServices;
using System.Text.Json.Serialization;

namespace DotNetCommons.AI;

[JsonConverter(typeof(FloatVectorConverter))]
public class FloatVector
{
    private readonly float[] _values;

    public int Length => _values.Length;
    public ReadOnlySpan<float> Values => _values;
    public float this[int index] => _values[index];

    public FloatVector(ReadOnlySpan<byte> data)
    {
        if (data.Length % sizeof(float) != 0)
            throw new ArgumentException("Invalid binary data length", nameof(data));

        _values = MemoryMarshal.Cast<byte, float>(data).ToArray();
    }

    public FloatVector(params float[] values)
    {
        ArgumentNullException.ThrowIfNull(values);
        _values = values;
    }

    public double CosineSimilarity(FloatVector other)
    {
        return Dot(other) / (Magnitude() * other.Magnitude());
    }

    public double Dot(FloatVector other)
    {
        CheckDimensions(this, other);

        var sum         = 0.0;
        var simd        = Vector<float>.Count;
        var accumulator = Vector<float>.Zero;

        var i = 0;
        for (; i <= Length - simd; i += simd)
        {
            var a = new Vector<float>(_values, i);
            var b = new Vector<float>(other._values, i);
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
        var simd        = Vector<float>.Count;
        var accumulator = Vector<float>.Zero;

        var i = 0;
        for (; i <= Length - simd; i += simd)
        {
            var v = new Vector<float>(_values, i);
            accumulator += v * v;
        }

        for (var j = 0; j < simd; j++)
            sum += accumulator[j];

        for (; i < Length; i++)
            sum += _values[i] * _values[i];

        return Math.Sqrt(sum);
    }

    public FloatVector Normalize()
    {
        var magnitude = Magnitude();
        if (magnitude == 0)
            return this;

        var result       = new float[Length];
        var scalar       = (float)(1.0 / magnitude);
        var simd         = Vector<float>.Count;
        var scalarVector = new Vector<float>(scalar);

        var i = 0;
        for (; i <= Length - simd; i += simd)
        {
            var v = new Vector<float>(_values, i);
            (v * scalarVector).CopyTo(result, i);
        }

        for (; i < Length; i++)
            result[i] = _values[i] * scalar;

        return new FloatVector(result);
    }

    public static FloatVector operator +(FloatVector a, FloatVector b)
    {
        CheckDimensions(a, b);
        var result = new float[a.Length];
        var simd   = Vector<float>.Count;

        var i = 0;
        for (; i <= a.Length - simd; i += simd)
        {
            var va = new Vector<float>(a._values, i);
            var vb = new Vector<float>(b._values, i);
            (va + vb).CopyTo(result, i);
        }

        for (; i < result.Length; i++)
            result[i] = a[i] + b[i];

        return new FloatVector(result);
    }

    public static FloatVector operator -(FloatVector a, FloatVector b)
    {
        CheckDimensions(a, b);
        var result = new float[a.Length];
        var simd   = Vector<float>.Count;

        var i = 0;
        for (; i <= a.Length - simd; i += simd)
        {
            var va = new Vector<float>(a._values, i);
            var vb = new Vector<float>(b._values, i);
            (va - vb).CopyTo(result, i);
        }

        for (; i < result.Length; i++)
            result[i] = a[i] - b[i];

        return new FloatVector(result);
    }

    public static FloatVector operator *(FloatVector v, float scalar)
    {
        var result       = new float[v.Length];
        var simd         = Vector<float>.Count;
        var scalarVector = new Vector<float>(scalar);

        var i = 0;
        for (; i <= v.Length - simd; i += simd)
        {
            var vv = new Vector<float>(v._values, i);
            (vv * scalarVector).CopyTo(result, i);
        }

        for (; i < result.Length; i++)
            result[i] = v[i] * scalar;

        return new FloatVector(result);
    }

    public static FloatVector operator /(FloatVector v, float scalar)
    {
        return v * (1f / scalar);
    }

    public byte[]? ToByteArray()
    {
        if (_values.Length == 0)
            return null;

        var bytes = new byte[_values.Length * sizeof(float)];
        MemoryMarshal.AsBytes(_values.AsSpan()).CopyTo(bytes);
        return bytes;
    }

    public DoubleVector ToDouble()
    {
        var result = new double[_values.Length];
        var simd   = Vector<float>.Count;

        var i = 0;
        for (; i <= _values.Length - simd; i += simd)
        {
            var v = new Vector<float>(_values, i);
            for (var j = 0; j < simd && i + j < _values.Length; j++)
                result[i + j] = v[j];
        }

        for (; i < result.Length; i++)
            result[i] = _values[i];

        return new DoubleVector(result);
    }

    public override string ToString()
    {
        return $"[{string.Join(", ", _values)}]";
    }

    private static void CheckDimensions(FloatVector a, FloatVector b)
    {
        if (a.Length != b.Length)
            throw new ArgumentException("Vector dimensions must match.");
    }

    public static implicit operator float[](FloatVector vector) => vector._values;
}
