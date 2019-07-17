using System;

// Written by Mats Gefvert
// Distributed under MIT License: https://opensource.org/licenses/MIT
// ReSharper disable UnusedMember.Global

namespace DotNetCommons.Core
{
    public static class TypeExtensions
    {
        public static bool DescendantOfOrEqual(this Type type, Type testAgainst)
        {
            return type == testAgainst || type.IsSubclassOf(testAgainst);
        }

        public static bool IsNullable(this Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>) && type != typeof(string);
        }

        // From https://stackoverflow.com/questions/1749966/c-sharp-how-to-determine-whether-a-type-is-a-number
        public static bool IsNumeric(this Type type)
        {
            var tc = Type.GetTypeCode(type);
            return tc == TypeCode.Byte || tc == TypeCode.SByte
                   || tc == TypeCode.UInt16 || tc == TypeCode.UInt32 || tc == TypeCode.UInt64
                   || tc == TypeCode.Int16 || tc == TypeCode.Int32 || tc == TypeCode.Int64
                   || tc == TypeCode.Decimal || tc == TypeCode.Double || tc == TypeCode.Single;
        }
    }
}
