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

        /// <summary>
        /// Add a value to a collection if it is not null. If the collection itself is null, nothing is done.
        /// </summary>
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

        /// <summary>
        /// Add a range of values to a collection, filtering for null values. If the collection itself is null,
        /// nothing is done.
        /// </summary>
        public static void AddRangeIfNotNull<T>(this ICollection<T> collection, IEnumerable<T> items)
        {
            if (collection == null || items == null)
                return;

            foreach (var item in items)
                if (item != null)
                    collection.Add(item);
        }

        /// <summary>
        /// Separate a list of values into batches.
        /// </summary>
        public static IEnumerable<T[]> Batch<T>(this IEnumerable<T> list, int size)
        {
            if (size <= 0)
                throw new ArgumentOutOfRangeException(nameof(size));

            var batch = new List<T>(size);
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

        /// <summary>
        /// Generate a list of distinct values from a collection of objects using a select function.
        /// </summary>
        /// <param name="list">List of objects.</param>
        /// <param name="func">Selector function that extracts a value from the object.</param>
        /// <returns>A distinct list of the values returned from the selector function.</returns>
        public static List<TResult> DistinctValues<T, TResult>(this IEnumerable<T> list, Func<T, TResult> func) where TResult : struct
        {
            var result = new HashSet<TResult>();
            foreach (var item in list)
                result.Add(func(item));

            return result.ToList();
        }

        /// <summary>
        /// Generate a list of distinct values from a collection of objects using two select functions.
        /// </summary>
        /// <param name="list">List of objects.</param>
        /// <param name="func1">First selector function</param>
        /// <param name="func2">Second selector function</param>
        /// <returns>A distinct combined list of the values returned from the two selector functions.</returns>
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

        /// <summary>
        /// Generate a list of distinct values from a collection of objects using a select function, and checking for null values.
        /// </summary>
        /// <param name="list">List of objects.</param>
        /// <param name="func">Selector function that extracts a value from the object.</param>
        /// <returns>A distinct list of the values returned from the selector function.</returns>
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

        /// <summary>
        /// Generate a list of distinct values from a collection of objects using two select functions, and checking for null values.
        /// </summary>
        /// <param name="list">List of objects.</param>
        /// <param name="func1">First selector function</param>
        /// <param name="func2">Second selector function</param>
        /// <returns>A distinct combined list of the values returned from the two selector functions.</returns>
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

        /// <summary>
        /// Extract an item from a list at a given position.
        /// </summary>
        public static T ExtractAt<T>(this IList<T> list, int position)
        {
            var result = list[position];
            list.RemoveAt(position);

            return result;
        }

        /// <summary>
        /// Extract an item from a list at a given position, or return a default (null) value if the position is out of bounds.
        /// </summary>
        public static T ExtractAtOrDefault<T>(this IList<T> list, int position, T defaultValue = default)
        {
            return position < 0 || position >= list.Count ? defaultValue : ExtractAt(list, position);
        }

        /// <summary>
        /// Extract all values from a list, effectively clearing it.
        /// </summary>
        public static List<T> ExtractAll<T>(this IList<T> list)
        {
            var result = list.ToList();
            list.Clear();
            return result;
        }

        /// <summary>
        /// Extract all values from a list that matches a selector function.
        /// </summary>
        public static List<T> ExtractAll<T>(this IList<T> list, Predicate<T> match)
        {
            var result = list.Where(x => match(x)).ToList();
            foreach (var item in result)
                list.Remove(item);

            return result;
        }

        /// <summary>
        /// Extract the first item from a list, throwing an exception if there are no items.
        /// </summary>
        public static T ExtractFirst<T>(this IList<T> list)
        {
            return ExtractAt(list, 0);
        }

        /// <summary>
        /// Extract the first item from a list, or return a default (null) value if there are no items.
        /// </summary>
        public static T ExtractFirstOrDefault<T>(this IList<T> list, T defaultValue = default)
        {
            return list.Any() ? ExtractAt(list, 0) : defaultValue;
        }

        /// <summary>
        /// Extract the last item from a list, throwing an exception if there are no items.
        /// </summary>
        public static T ExtractLast<T>(this IList<T> list)
        {
            return ExtractAt(list, list.Count - 1);
        }

        /// <summary>
        /// Extract the last item from a list, or return a default (null) value if there are no items.
        /// </summary>
        public static T ExtractLastOrDefault<T>(this IList<T> list, T defaultValue = default)
        {
            return list.Any() ? ExtractAt(list, list.Count - 1) : defaultValue;
        }

        /// <summary>
        /// Extract a range of items from a list.
        /// </summary>
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

        /// <summary>
        /// Get a value from a dictionary if it exists, otherwise return the default value.
        /// </summary>
        public static TValue GetOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue = default)
        {
            return dictionary.TryGetValue(key, out var result) ? result : defaultValue;
        }

        /// <summary>
        /// Increment the value of a given dictionary key, optionally creating the key with a default value of 0
        /// if it doesn't exist, and then incrementing it. Returns the new value.
        /// </summary>
        public static decimal Increment<TKey>(this IDictionary<TKey, decimal> dictionary, TKey key, decimal value = 1)
        {
            value += GetOrDefault(dictionary, key);
            dictionary[key] = value;
            return value;
        }

        /// <summary>
        /// Increment the value of a given dictionary key, optionally creating the key with a default value of 0
        /// if it doesn't exist, and then incrementing it. Returns the new value.
        /// </summary>
        public static int Increment<TKey>(this IDictionary<TKey, int> dictionary, TKey key, int value = 1)
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
        /// Find minimum and maximum values for a sequence of values.
        /// </summary>
        public static (T Min, T Max) MinMax<T>(this IEnumerable<T> collection)
            where T : IComparable<T>
        {
            var min = default(T);
            var max = default(T);
            var first = true;

            foreach (var item in collection)
            {
                if (first)
                {
                    min = item;
                    max = item;
                    first = false;
                }
                else
                {
                    if (min!.CompareTo(item) > 0)
                        min = item;
                    if (max!.CompareTo(item) < 0)
                        max = item;
                }
            }

            if (first)
                throw new InvalidOperationException("Cannot apply MinMax to an empty collection.");

            return (min!, max!);
        }

        /// <summary>
        /// Find minimum and maximum values for a sequence of values using a selector for each value.
        /// </summary>
        public static (TValue Min, TValue Max) MinMax<TObject, TValue>(this IEnumerable<TObject> collection, Func<TObject, TValue> selector)
            where TValue : IComparable<TValue>
        {
            var min = default(TValue);
            var max = default(TValue);
            var first = true;

            foreach (var item in collection.Select(selector))
            {
                if (first)
                {
                    min = item;
                    max = item;
                    first = false;
                }
                else
                {
                    if (min!.CompareTo(item) > 0)
                        min = item;
                    if (max!.CompareTo(item) < 0)
                        max = item;
                }
            }

            if (first)
                throw new InvalidOperationException("Cannot apply MinMax to an empty collection.");

            return (min!, max!);
        }

        /// <summary>
        /// Repeat a collection a number of times
        /// </summary>
        public static IEnumerable<T> Repeat<T>(this ICollection<T> collection, int times = 2)
        {
            for (var i = 0; i < times; i++)
                foreach (var item in collection)
                    yield return item;
        }

        /// <summary>
        /// Swap position for two items in a list
        /// </summary>
        public static bool Swap<T>(this IList<T> list, int pos1, int pos2)
        {
            if (pos1 < 0 || pos2 < 0 || pos1 >= list.Count || pos2 >= list.Count)
                return false;

            var item = list[pos1];
            list[pos1] = list[pos2];
            list[pos2] = item;

            return true;
        }

        /// <summary>
        /// Iterate through an enumerable and bring along an index counter
        /// </summary>
        public static IEnumerable<(T Item, int Index)> WithIndex<T>(this IEnumerable<T> source)
        {
            return source.Select((item, index) => (item, index));
        }
    }
}
