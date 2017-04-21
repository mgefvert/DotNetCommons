using System;

// Written by Mats Gefvert
// Distributed under MIT License: https://opensource.org/licenses/MIT

namespace DotNetCommons
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
    }
}
