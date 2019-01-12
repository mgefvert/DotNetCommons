using System;
using System.Collections.Generic;
using System.Linq;

// Written by Mats Gefvert
// Distributed under MIT License: https://opensource.org/licenses/MIT
// ReSharper disable UnusedMember.Global

namespace DotNetCommons.Collections
{
    public static class CollectionExtensions
    {
        public class Intersection<T>
        {
            public readonly List<T> Left;
            public readonly List<T> Both;
            public readonly List<T> Right;

            public Intersection()
            {
                Left = new List<T>();
                Both = new List<T>();
                Right = new List<T>();
            }
        }

        private static int MinMax(int value, int min, int max)
        {
            return value < min ? min : (value > max ? max : value);
        }

        public static void AddIfNotNull<T>(this ICollection<T> collection, T item)
        {
            if (collection != null && item != null)
                collection.Add(item);
        }

        public static void AddRange<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, IEnumerable<KeyValuePair<TKey, TValue>> items)
        {
            foreach (var item in items)
                dictionary.Add(item);
        }

        public static void AddRangeIfNotNull<T>(this ICollection<T> collection, IEnumerable<T> items)
        {
            if (collection == null || items == null)
                return;

            foreach (var item in items)
                if (item != null)
                    collection.Add(item);
        }

        public static List<TResult> DistinctValues<T, TResult>(this IEnumerable<T> list, Func<T, TResult> func) where TResult : struct
        {
            return list.Select(func).Distinct().ToList();
        }

        public static List<TResult> DistinctValues<T, TResult>(this IEnumerable<T> list, Func<T, TResult?> func) where TResult : struct
        {
            return list.Select(func).Where(x => x != null).Select(x => x.Value).Distinct().ToList();
        }

        public static void Each<T>(this IEnumerable<T> items, Action<T> action)
        {
            foreach (var item in items)
                action(item);
        }

        public static T ExtractAt<T>(this IList<T> list, int position)
        {
            var result = list[position];
            list.RemoveAt(position);

            return result;
        }

        public static T ExtractAtOrDefault<T>(this IList<T> list, int position)
        {
            return position < 0 || position >= list.Count ? default : ExtractAt(list, position);
        }

        public static List<T> ExtractAll<T>(this IList<T> list, Predicate<T> match)
        {
            var result = list.Where(x => match(x)).ToList();
            foreach (var item in result)
                list.Remove(item);

            return result;
        }

        public static T ExtractFirst<T>(this IList<T> list)
        {
            return ExtractAt(list, 0);
        }

        public static T ExtractFirstOrDefault<T>(this IList<T> list)
        {
            return list.Any() ? ExtractAt(list, 0) : default;
        }

        public static T ExtractLast<T>(this IList<T> list)
        {
            return ExtractAt(list, list.Count - 1);
        }

        public static T ExtractLastOrDefault<T>(this IList<T> list)
        {
            return list.Any() ? ExtractAt(list, list.Count - 1) : default;
        }

        public static List<T> ExtractRange<T>(this List<T> list, int offset, int count)
        {
            offset = MinMax(offset, 0, list.Count);
            count = MinMax(count, 0, list.Count - offset);

            var result = list.GetRange(offset, count);
            list.RemoveRange(offset, count);

            return result;
        }

        public static TValue GetOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
        {
            return dictionary.TryGetValue(key, out var result) ? result : default;
        }

        public static decimal Increase<TKey>(this IDictionary<TKey, decimal> dictionary, TKey key, decimal value = 1)
        {
            value += GetOrDefault(dictionary, key);
            dictionary[key] = value;
            return value;
        }

        public static int Increase<TKey>(this IDictionary<TKey, int> dictionary, TKey key, int value = 1)
        {
            value += GetOrDefault(dictionary, key);
            dictionary[key] = value;
            return value;
        }

        /// <summary>
        /// Compare two lists against each other and return an Intersection result from the comparison, 
        /// listing the objects found in only list1, only list2, or both lists.
        /// </summary>
        /// <typeparam name="T">Type of list</typeparam>
        /// <param name="list1">The first list (left)</param>
        /// <param name="list2">The second list (right)</param>
        /// <returns>An Intersection object with the results of the comparison</returns>
        public static Intersection<T> Intersect<T>(IList<T> list1, IList<T> list2)
        {
            return Intersect(list1, list2, (Comparison<T>)null);
        }

        /// <summary>
        /// Compare two lists against each other and return an Intersection result from the comparison, 
        /// listing the objects found in only list1, only list2, or both lists.
        /// </summary>
        /// <typeparam name="T">Type of list</typeparam>
        /// <param name="list1">The first list (left)</param>
        /// <param name="list2">The second list (right)</param>
        /// <param name="comparer">A specific comparer to use for comparing the objects</param>
        /// <returns>An Intersection object with the results of the comparison</returns>
        public static Intersection<T> Intersect<T>(IList<T> list1, IList<T> list2, IComparer<T> comparer)
        {
            return Intersect(list1, list2, comparer.Compare);
        }

        /// <summary>
        /// Compare two lists against each other and return an Intersection result from the comparison, 
        /// listing the objects found in only list1, only list2, or both lists.
        /// </summary>
        /// <typeparam name="T">Type of list</typeparam>
        /// <param name="list1">The first list (left)</param>
        /// <param name="list2">The second list (right)</param>
        /// <param name="comparison">A comparison method for comparing the objects</param>
        /// <returns>An Intersection object with the results of the comparison</returns>
        public static Intersection<T> Intersect<T>(IList<T> list1, IList<T> list2, Comparison<T> comparison)
        {
            bool DoCompare(T item1, T item2)
            {
                if (comparison != null)
                    return comparison(item1, item2) == 0;
                if (item1 is IComparable<T> comparable)
                    return comparable.CompareTo(item2) == 0;
                if (item1 is IEquatable<T> equatable)
                    return equatable.Equals(item2);

                return Comparer<T>.Default.Compare(item1, item2) == 0;
            }

            var result = new Intersection<T>();

            bool empty1 = list1 == null || list1.Count == 0;
            bool empty2 = list2 == null || list2.Count == 0;

            if (empty1 && empty2)
                return result;

            if (empty1)
            {
                result.Right.AddRange(list2);
                return result;
            }

            if (empty2)
            {
                result.Left.AddRange(list1);
                return result;
            }

            var search2 = new List<T>(list2);

            // Divide array1 into Left and Both
            foreach (var item1 in list1)
            {
                var n = search2.FindIndex(x => DoCompare(item1, x));
                if (n == -1)
                {
                    result.Left.Add(item1);
                }
                else
                {
                    result.Both.Add(item1);
                    search2.RemoveAt(n);
                }
            }

            // Any remaining items in array2 (=search2) must now fall to right.
            result.Right.AddRange(search2);

            return result;
        }

        public static bool IsSet<TKey>(this IDictionary<TKey, string> dictionary, TKey key)
        {
            return !string.IsNullOrWhiteSpace(dictionary.GetOrDefault(key));
        }

        public static DateTime Min<T>(this IEnumerable<T> list, Func<T, DateTime> selector)
        {
            var ticks = list.Select(selector).Select(x => x.Ticks).Min();
            return new DateTime(ticks);
        }

        public static DateTime Max<T>(this IEnumerable<T> list, Func<T, DateTime> selector)
        {
            var ticks = list.Select(selector).Select(x => x.Ticks).Max();
            return new DateTime(ticks);
        }

        public static IEnumerable<T> Repeat<T>(this ICollection<T> collection, int times = 2)
        {
            for (var i = 0; i < times; i++)
                foreach (var item in collection)
                    yield return item;
        }

        public static bool Swap<T>(this IList<T> list, int pos1, int pos2)
        {
            if (pos1 < 0 || pos2 < 0 || pos1 >= list.Count || pos2 >= list.Count)
                return false;

            var item = list[pos1];
            list[pos1] = list[pos2];
            list[pos2] = item;

            return true;
        }
    }
}
