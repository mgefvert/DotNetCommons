using System;

// Written by Mats Gefvert
// Distributed under MIT License: https://opensource.org/licenses/MIT
// ReSharper disable UnusedMember.Global

namespace DotNetCommons;

public static class CommonTypeExtensions
{
    /// <summary>
    /// True if the given type is equal to or a descendant of another type.
    /// </summary>
    public static bool DescendantOfOrEqual(this Type type, Type testAgainst)
    {
        return type == testAgainst || type.IsSubclassOf(testAgainst);
    }

    /// <summary>
    /// True if a given type is nullable.
    /// </summary>
    public static bool IsNullable(this Type type)
    {
        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>) &&
               type != typeof(string);
    }

    // From https://stackoverflow.com/questions/1749966/c-sharp-how-to-determine-whether-a-type-is-a-number
    /// <summary>
    /// True if a given type is numeric (int32, uint32, byte, decimal, double etc).
    /// </summary>
    public static bool IsNumeric(this Type type)
    {
        var tc = Type.GetTypeCode(type);
        return tc == TypeCode.Byte || tc == TypeCode.SByte
                                   || tc == TypeCode.UInt16 || tc == TypeCode.UInt32 || tc == TypeCode.UInt64
                                   || tc == TypeCode.Int16 || tc == TypeCode.Int32 || tc == TypeCode.Int64
                                   || tc == TypeCode.Decimal || tc == TypeCode.Double || tc == TypeCode.Single;
    }
}