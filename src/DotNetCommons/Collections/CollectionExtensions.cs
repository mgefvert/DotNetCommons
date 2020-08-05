using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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
            public readonly List<Tuple<T, T>> Both;
            public readonly List<T> Right;

            public Intersection()
            {
                Left = new List<T>();
                Both = new List<Tuple<T, T>>();
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

        public static IEnumerable<T[]> Batch<T>(this IEnumerable<T> list, int size)
        {
            var batch = new List<T>();
            foreach (var item in list)
            {
                batch.Add(item);
                if (batch.Count >= size)
                {
                    yield return batch.ToArray();
                    batch.Clear();
                }
            }

            if (batch.Any())
                yield return batch.ToArray();
        }

        public static List<TResult> DistinctValues<T, TResult>(this IEnumerable<T> list, Func<T, TResult> func) where TResult : struct
        {
            var result = new HashSet<TResult>();
            foreach (var item in list)
                result.Add(func(item));

            return result.ToList();
        }

        public static List<TResult> DistinctValues<T, TResult>(this IEnumerable<T> list, Func<T, TResult> func1, Func<T, TResult> func2) where TResult : struct
        {
            var result = new HashSet<TResult>();
            foreach (var item in list)
            {
                result.Add(func1(item));
                result.Add(func2(item));
            }

            return result.ToList();
        }

        public static List<TResult> DistinctValues<T, TResult>(this IEnumerable<T> list, Func<T, TResult?> func) where TResult : struct
        {
            var result = new HashSet<TResult>();
            foreach (var item in list)
            {
                var x = func(item);
                if (x != null)
                    result.Add(x.Value);
            }

            return result.ToList();
        }

        public static List<TResult> DistinctValues<T, TResult>(this IEnumerable<T> list, Func<T, TResult?> func1, Func<T, TResult?> func2) where TResult : struct
        {
            var result = new HashSet<TResult>();
            foreach (var item in list)
            {
                var x = func1(item);
                if (x != null)
                    result.Add(x.Value);
                x = func2(item);
                if (x != null)
                    result.Add(x.Value);
            }

            return result.ToList();
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

        public static List<T> ExtractAll<T>(this IList<T> list)
        {
            var result = list.ToList();
            list.Clear();
            return result;
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

        public static async Task ForEachAsync<TSource>(this IEnumerable<TSource> list,
            int maxDegreeOfParallelism, Func<TSource, Task> func)
        {
            var throttler = new SemaphoreSlim(initialCount: maxDegreeOfParallelism);
            var tasks = list.Select(async item =>
            {
                try
                {
                    await throttler.WaitAsync();
                    await func(item);
                }
                finally
                {
                    throttler.Release();
                }
            });

            await Task.WhenAll(tasks);
        }

        public static async Task ForEachAsync<TSource>(this IEnumerable<TSource> list,
            int maxDegreeOfParallelism, Func<TSource, int, Task> func)
        {
            var throttler = new SemaphoreSlim(initialCount: maxDegreeOfParallelism);
            var tasks = list.Select(async (item, index) =>
            {
                try
                {
                    await throttler.WaitAsync();
                    await func(item, index);
                }
                finally
                {
                    throttler.Release();
                }
            });

            await Task.WhenAll(tasks);
        }

        public static async Task<ConcurrentDictionary<TSource, TResult>> ForEachAsync<TSource, TResult>(this IEnumerable<TSource> list, 
            int maxDegreeOfParallelism, Func<TSource, Task<TResult>> func)
        {
            var result = new ConcurrentDictionary<TSource, TResult>();
            var throttler = new SemaphoreSlim(initialCount: maxDegreeOfParallelism);
            var tasks = list.Select(async item =>
            {
                try
                {
                    await throttler.WaitAsync();
                    var response = await func(item);
                    result.TryAdd(item, response);
                }
                finally
                {
                    throttler.Release();
                }
            });

            await Task.WhenAll(tasks);
            return result;
        }

        public static async Task<ConcurrentDictionary<TSource, TResult>> ForEachAsync<TSource, TResult>(this IEnumerable<TSource> list,
            int maxDegreeOfParallelism, Func<TSource, int, Task<TResult>> func)
        {
            var result = new ConcurrentDictionary<TSource, TResult>();
            var throttler = new SemaphoreSlim(initialCount: maxDegreeOfParallelism);
            var tasks = list.Select(async (item, index) =>
            {
                try
                {
                    await throttler.WaitAsync();
                    var response = await func(item, index);
                    result.TryAdd(item, response);
                }
                finally
                {
                    throttler.Release();
                }
            });

            await Task.WhenAll(tasks);
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
        public static Intersection<T> Intersect<T>(IReadOnlyCollection<T> list1, IReadOnlyCollection<T> list2)
        {
            return Intersect(list1, list2, null, x => x);
        }

        /// <summary>
        /// Compare two lists against each other and return an Intersection result from the comparison, 
        /// listing the objects found in only list1, only list2, or both lists.
        /// </summary>
        /// <typeparam name="T">Type of list</typeparam>
        /// <typeparam name="TKey">Type of key</typeparam>
        /// <param name="list1">The first list (left)</param>
        /// <param name="list2">The second list (right)</param>
        /// <param name="selector">A selector for the key to compare the objects by</param>
        /// <returns>An Intersection object with the results of the comparison</returns>
        public static Intersection<T> Intersect<T, TKey>(IReadOnlyCollection<T> list1, IReadOnlyCollection<T> list2, Func<T, TKey> selector)
        {
            return Intersect(list1, list2, null, selector);
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
        public static Intersection<T> Intersect<T>(IReadOnlyCollection<T> list1, IReadOnlyCollection<T> list2, IComparer<T> comparer)
        {
            return Intersect(list1, list2, comparer.Compare, x => x);
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
        public static Intersection<T> Intersect<T>(IReadOnlyCollection<T> list1, IReadOnlyCollection<T> list2, Comparison<T> comparison)
        {
            return Intersect(list1, list2, comparison, x => x);
        }

        public static Intersection<T> Intersect<T, TKey>(IReadOnlyCollection<T> list1, IReadOnlyCollection<T> list2, Comparison<TKey> comparison, Func<T, TKey> selector)
        {
            bool DoCompare(T item1, T item2)
            {
                var value1 = selector(item1);
                var value2 = selector(item2);

                if (comparison != null)
                    return comparison(value1, value2) == 0;
                if (value1 is IComparable<TKey> comparable)
                    return comparable.CompareTo(value2) == 0;
                if (value1 is IEquatable<TKey> equatable)
                    return equatable.Equals(value2);

                return Comparer<TKey>.Default.Compare(value1, value2) == 0;
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
                    result.Both.Add(new Tuple<T, T>(item1, search2[n]));
                    search2.RemoveAt(n);
                }
            }

            // Any remaining items in array2 (=search2) must now fall to right.
            result.Right.AddRange(search2);

            return result;
        }

        /// <summary>
        /// Check whether a value exists and is not null and not empty.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="dictionary"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool IsSet<TKey>(this IDictionary<TKey, string> dictionary, TKey key)
        {
            return !string.IsNullOrWhiteSpace(dictionary.GetOrDefault(key));
        }

        /**
         * Return the minimum of a list of DateTimes
         */
        public static DateTime Min<T>(this IEnumerable<T> list, Func<T, DateTime> selector)
        {
            var ticks = list.Select(selector).Select(x => x.Ticks).Min();
            return new DateTime(ticks);
        }

        /**
         * Return the maximum of a list of DateTimes
         */
        public static DateTime Max<T>(this IEnumerable<T> list, Func<T, DateTime> selector)
        {
            var ticks = list.Select(selector).Select(x => x.Ticks).Max();
            return new DateTime(ticks);
        }

        /**
         * Repeat a collection a number of times
         */
        public static IEnumerable<T> Repeat<T>(this ICollection<T> collection, int times = 2)
        {
            for (var i = 0; i < times; i++)
                foreach (var item in collection)
                    yield return item;
        }

        /**
         * Swap position for two items in a list
         */
        public static bool Swap<T>(this IList<T> list, int pos1, int pos2)
        {
            if (pos1 < 0 || pos2 < 0 || pos1 >= list.Count || pos2 >= list.Count)
                return false;

            var item = list[pos1];
            list[pos1] = list[pos2];
            list[pos2] = item;

            return true;
        }

        /**
         * Iterate through an enumerable and bring along an index counter
         */
        public static IEnumerable<(T item, int index)> WithIndex<T>(this IEnumerable<T> source)
        {
            return source.Select((item, index) => (item, index));
        }
    }
}
