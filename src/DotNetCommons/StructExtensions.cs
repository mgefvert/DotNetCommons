using System;

namespace DotNetCommons
{
    public static class StructExtensions
    {
        public static T Limit<T>(this T value, T min, T max) where T : struct, IComparable<T>
        {
            if (value.CompareTo(min) < 0)
                return min;
            if (value.CompareTo(max) > 0)
                return max;

            return value;
        }
    }
}
