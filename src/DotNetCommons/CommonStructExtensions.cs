using System;

// ReSharper disable UnusedMember.Global

namespace DotNetCommons;

public static class CommonStructExtensions
{
    /// <summary>
    /// Limit a value inside a guard range.
    /// </summary>
    public static T Limit<T>(this T value, T min, T max) where T : struct, IComparable<T>
    {
        if (value.CompareTo(min) < 0)
            return min;
        if (value.CompareTo(max) > 0)
            return max;

        return value;
    }

    /// <summary>
    /// Determine the number of set bits in a value.
    /// </summary>
    public static int BitCount(this uint value)
    {
        value -= (value >> 1) & 0x55555555;
        value = (value & 0x33333333) + ((value >> 2) & 0x33333333);
        return (int)(((value + (value >> 4)) & 0x0F0F0F0F) * 0x01010101) >> 24;
    }
}