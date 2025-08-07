// ReSharper disable UnusedMember.Global

using System.Numerics;
using System.Runtime.CompilerServices;

namespace DotNetCommons;

public static class CommonStructExtensions
{
    /// <summary>
    /// Determines whether a value lies between two specified boundaries.
    /// </summary>
    /// <param name="value">The value to evaluate.</param>
    /// <param name="lower">The lower boundary of the range.</param>
    /// <param name="upper">The upper boundary of the range.</param>
    /// <param name="inclusiveUpper">Indicates whether the upper boundary is inclusive. Default is false.</param>
    /// <typeparam name="T">The type of the value, which must be a value type implementing IComparable.</typeparam>
    /// <returns>True if the value is between the specified range; otherwise, false.</returns>
    public static bool Between<T>(this T value, T lower, T upper, bool inclusiveUpper = false)
        where T : struct, IComparable<T>
    {
        var lowerCheck = value.CompareTo(lower) >= 0;
        var upperCheck = inclusiveUpper ? value.CompareTo(upper) <= 0 : value.CompareTo(upper) < 0;

        return lowerCheck && upperCheck;
    }

    /// <summary>
    /// Limit a value inside a guard range.
    /// </summary>
    public static T Limit<T>(this T value, T min, T max) where T : struct, IComparable
    {
        if (value.CompareTo(min) < 0)
            return min;
        if (value.CompareTo(max) > 0)
            return max;

        return value;
    }

    /// <summary>
    /// Determines whether the number of set bits in a value is odd or even.
    /// </summary>
    /// <param name="value">The unsigned integer value to evaluate.</param>
    /// <returns>true if the number of set bits in the value is even; otherwise, false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsParityEven(this uint value) => (BitOperations.PopCount(value) & 1) == 0;

    /// <summary>
    /// Determines whether the number of set bits in a value is odd or even.
    /// </summary>
    /// <param name="value">The unsigned integer value to evaluate.</param>
    /// <returns>true if the number of set bits in the value is odd; otherwise, false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsParityOdd(this uint value) => (BitOperations.PopCount(value) & 1) != 0;

    /// <summary>
    /// Determines whether the number of set bits in a value is odd or even.
    /// </summary>
    /// <param name="value">The unsigned long value to evaluate.</param>
    /// <returns>true if the number of set bits in the value is odd; otherwise, false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsParityEven(this ulong value) => (BitOperations.PopCount(value) & 1) == 0;

    /// <summary>
    /// Determines whether the number of set bits in a value is odd or even.
    /// </summary>
    /// <param name="value">The unsigned long value to evaluate.</param>
    /// <returns>true if the number of set bits in the value is even; otherwise, false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsParityOdd(this ulong value) => (BitOperations.PopCount(value) & 1) != 0;
}