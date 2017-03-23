using System;
using System.Collections.Generic;
using System.Linq;

// Written by Mats Gefvert
// Distributed under MIT License: https://opensource.org/licenses/MIT

namespace DotNetCommons
{
    public static class CollectionExtensions
    {
        private static int MinMax(int value, int min, int max)
        {
            return value < min ? min : (value > max ? max : value);
        }

        public static void AddIfNotNull<T>(this ICollection<T> collection, T item)
        {
            if (collection != null && item != null)
                collection.Add(item);
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

        public static T ExtractAt<T>(this IList<T> list, int position)
        {
            var result = list[position];
            list.RemoveAt(position);

            return result;
        }

        public static T ExtractAtOrDefault<T>(this IList<T> list, int position)
        {
            return position < 0 || position >= list.Count ? default(T) : ExtractAt(list, position);
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
            return list.Any() ? ExtractAt(list, 0) : default(T);
        }

        public static T ExtractLast<T>(this IList<T> list)
        {
            return ExtractAt(list, list.Count - 1);
        }

        public static T ExtractLastOrDefault<T>(this IList<T> list)
        {
            return list.Any() ? ExtractAt(list, list.Count - 1) : default(T);
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
            TValue result;
            return dictionary.TryGetValue(key, out result) ? result : default(TValue);
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
