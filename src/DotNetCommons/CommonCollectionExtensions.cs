// Written by Mats Gefvert
// Distributed under MIT License: https://opensource.org/licenses/MIT
// ReSharper disable UnusedMember.Global

namespace DotNetCommons;

public enum WalkTreeMode
{
    ShallowFirst,
    DepthFirst
}

public static class CommonCollectionExtensions
{
    public class Intersection<T1, T2>
    {
        /// <summary>
        /// Records that were found in the left list.
        /// </summary>
        public List<T1> Left { get; } = [];
        /// <summary>
        /// Records that were found to be identical in both lists (a tuple is used to contain
        /// a reference to both items, from left and right, in case there are internal differences).
        /// </summary>
        public List<(T1, T2)> Both { get; } = [];
        /// <summary>
        /// Records that were found in the right list.
        /// </summary>
        public List<T2> Right { get; } = [];

        public void Deconstruct(out List<T1> left, out List<(T1, T2)> both, out List<T2> right)
        {
            left = Left;
            both = Both;
            right = Right;
        }
    }

    private static int MinMax(int value, int min, int max)
    {
        return value < min ? min
            : value > max ? max
            : value;
    }

    /// <summary>
    /// Add a value to a collection if it is not null. If the collection itself is null, nothing is done.
    /// </summary>
    public static void AddIfNotNull<T>(this ICollection<T>? collection, T? item)
    {
        if (collection != null && item != null)
            collection.Add(item);
    }


    /// <summary>
    /// Add a range of items to a collection. (This applies to any collection, not just lists.)
    /// </summary>
    public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> items)
    {
        foreach (var item in items)
            collection.Add(item);
    }

    /// <summary>
    /// Add a range of items to a collection. (This applies to any collection, not just lists.)
    /// </summary>
    public static void AddRange<T>(this ICollection<T> collection, params T[] items) => collection.AddRange((IEnumerable<T>)items);

    /// <summary>
    /// Copy values from one dictionary to another.
    /// </summary>
    public static void AddRange<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, IEnumerable<KeyValuePair<TKey, TValue>> items)
    {
        foreach (var item in items)
            dictionary.Add(item);
    }

    /// <summary>
    /// Add a range of values to a collection, filtering for null values. If the collection itself is null,
    /// nothing is done.
    /// </summary>
    public static void AddRangeIfNotNull<T>(this ICollection<T>? collection, IEnumerable<T?>? items)
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
    public static T? ExtractAtOrDefault<T>(this IList<T> list, int position, T? defaultValue = default)
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
    public static T? ExtractFirstOrDefault<T>(this IList<T> list, T? defaultValue = default)
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
    public static T? ExtractLastOrDefault<T>(this IList<T> list, T? defaultValue = default)
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

    /// <summary>
    /// Iterative over an enumerable list of items, and perform an action for each of them.
    /// </summary>
    public static void ForEach<T>(this IEnumerable<T> items, Action<T> action)
    {
        foreach (var item in items)
            action(item);
    }

    /// <summary>
    /// Increment the value of a given dictionary key, optionally creating the key with a default value of 0
    /// if it doesn't exist, and then incrementing it. Returns the new value.
    /// </summary>
    public static decimal Increment<TKey>(this Dictionary<TKey, decimal> dictionary, TKey key, decimal value = 1) where TKey : notnull
    {
        value += dictionary.GetValueOrDefault(key);
        dictionary[key] = value;
        return value;
    }

    /// <summary>
    /// Increment the value of a given dictionary key, optionally creating the key with a default value of 0
    /// if it doesn't exist, and then incrementing it. Returns the new value.
    /// </summary>
    public static int Increment<TKey>(this Dictionary<TKey, int> dictionary, TKey key, int value = 1) where TKey : notnull
    {
        value += dictionary.GetValueOrDefault(key);
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
    public static Intersection<T, T> Intersect<T>(this IReadOnlyCollection<T> list1, IReadOnlyCollection<T> list2)
    {
        return Intersect(list1, list2, null, x => x, x => x);
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
    public static Intersection<T, T> Intersect<T, TKey>(this IReadOnlyCollection<T> list1, IReadOnlyCollection<T> list2, Func<T, TKey> selector)
    {
        return Intersect(list1, list2, null, selector, selector);
    }

    /// <summary>
    /// Compare two lists against each other and return an Intersection result from the comparison,
    /// listing the objects found in only list1, only list2, or both lists.
    /// </summary>
    /// <param name="list1">The first list (left)</param>
    /// <param name="list2">The second list (right)</param>
    /// <param name="keySelector1">A selector for the key to compare the left objects by</param>
    /// <param name="keySelector2">A selector for the key to compare the right objects by</param>
    /// <returns>An Intersection object with the results of the comparison</returns>
    public static Intersection<T1, T2> Intersect<T1, T2, TKey>(this IReadOnlyCollection<T1> list1, IReadOnlyCollection<T2> list2,
        Func<T1, TKey> keySelector1, Func<T2, TKey> keySelector2)
    {
        return Intersect(list1, list2, null, keySelector1, keySelector2);
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
    public static Intersection<T, T> Intersect<T>(this IReadOnlyCollection<T> list1, IReadOnlyCollection<T> list2, IComparer<T> comparer)
    {
        return Intersect(list1, list2, comparer.Compare, x => x, x => x);
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
    public static Intersection<T, T> Intersect<T>(this IReadOnlyCollection<T> list1, IReadOnlyCollection<T> list2, Comparison<T> comparison)
    {
        return Intersect(list1, list2, comparison, x => x, x => x);
    }

    /// <summary>
    /// Compare two lists against each other and return an Intersection result from the comparison,
    /// listing the objects found in only list1, only list2, or both lists, using a selector function.
    /// </summary>
    /// <param name="list1">The first list (left)</param>
    /// <param name="list2">The second list (right)</param>
    /// <param name="comparison">A comparison method for comparing the objects</param>
    /// <param name="keySelector1">A selector for the key to compare the left objects by</param>
    /// <param name="keySelector2">A selector for the key to compare the right objects by</param>
    /// <returns>An Intersection object with the results of the comparison</returns>
    public static Intersection<T1, T2> Intersect<T1, T2, TKey>(this IReadOnlyCollection<T1> list1, IReadOnlyCollection<T2> list2,
        Comparison<TKey>? comparison, Func<T1, TKey> keySelector1, Func<T2, TKey> keySelector2)
    {
        bool DoCompare(T1 item1, T2 item2)
        {
            var value1 = keySelector1(item1);
            var value2 = keySelector2(item2);

            if (comparison != null)
                return comparison(value1, value2) == 0;
            if (value1 is IComparable<TKey> comparable)
                return comparable.CompareTo(value2) == 0;
            if (value1 is IEquatable<TKey> equatable)
                return equatable.Equals(value2);

            return Comparer<TKey>.Default.Compare(value1, value2) == 0;
        }

        var result = new Intersection<T1, T2>();

        var empty1 = list1.Count == 0;
        var empty2 = list2.Count == 0;

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

        var search2 = new List<T2>(list2);

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
                result.Both.Add((item1, search2[n]));
                search2.RemoveAt(n);
            }
        }

        // Any remaining items in array2 (=search2) must now fall to right.
        result.Right.AddRange(search2);

        return result;
    }

    /// <summary>
    /// Determines whether a collection is null or contains no elements.
    /// </summary>
    public static bool IsEmpty<T>(this ICollection<T>? collection) => collection == null || collection.Count == 0;

    /// <summary>
    /// Determines whether a collection contains exactly one element.
    /// </summary>
    public static bool IsOne<T>(this ICollection<T>? collection) => collection is { Count: 1 };

    /// <summary>
    /// Determines whether a collection contains at least one element.
    /// </summary>
    public static bool IsAtLeastOne<T>(this ICollection<T>? collection) => collection is { Count: >= 1 };

    /// <summary>
    /// Determines whether the collection contains more than one element.
    /// </summary>
    public static bool IsMany<T>(this ICollection<T>? collection) => collection is { Count: > 1 };

    /// <summary>
    /// Javascript-like String.Join.
    /// </summary>
    public static string Join(this IEnumerable<string> enumerable, string separator)
    {
        return string.Join(separator, enumerable);
    }

    /// <summary>
    /// Javascript-like String.Join.
    /// </summary>
    public static string Join<T>(this IEnumerable<T> enumerable, Func<T, string> selector, string separator)
    {
        return enumerable.Select(selector).Join(separator);
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
    /// Exclude all null items
    /// </summary>
    public static IEnumerable<T> NotNulls<T>(this IEnumerable<T?> items) where T : class
    {
        foreach (var item in items)
            if (item != null)
                yield return item;
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

        (list[pos1], list[pos2]) = (list[pos2], list[pos1]);
        return true;
    }

    /// <summary>
    /// Toss items into different lists depending on a condition. Everything satisfying the condition(s) will go into
    /// their respective list, everything not matching will go into a remaining list.
    /// </summary>
    /// <param name="objects">Objects to consider</param>
    /// <param name="condition1">Condition for items going into list 1</param>
    /// <returns>A list of tossed items as well as a remainder</returns>
    public static (List<T>, List<T>) Toss<T>(this IEnumerable<T> objects, Func<T, bool> condition1)
    {
        var result1 = new List<T>();
        var remaining = new List<T>();
        foreach (var item in objects)
        {
            if (condition1(item))
                result1.Add(item);
            else
                remaining.Add(item);
        }

        return (result1, remaining);
    }

    /// <summary>
    /// Toss items into different lists depending on a condition. Everything satisfying the condition(s) will go into
    /// their respective list, everything not matching will go into a remaining list.
    /// </summary>
    /// <param name="objects">Objects to consider</param>
    /// <param name="condition1">Condition for items going into list 1</param>
    /// <param name="condition2">Condition for items going into list 2</param>
    /// <returns>A list of tossed items as well as a remainder</returns>
    public static (List<T>, List<T>, List<T>) Toss<T>(this IEnumerable<T> objects, Func<T, bool> condition1,
        Func<T, bool> condition2)
    {
        var result1 = new List<T>();
        var result2 = new List<T>();
        var remaining = new List<T>();
        foreach (var item in objects)
        {
            if (condition1(item))
                result1.Add(item);
            else if (condition2(item))
                result2.Add(item);
            else
                remaining.Add(item);
        }

        return (result1, result2, remaining);
    }

    /// <summary>
    /// Toss items into different lists depending on a condition. Everything satisfying the condition(s) will go into
    /// their respective list, everything not matching will go into a remaining list.
    /// </summary>
    /// <param name="objects">Objects to consider</param>
    /// <param name="condition1">Condition for items going into list 1</param>
    /// <param name="condition2">Condition for items going into list 2</param>
    /// <param name="condition3">Condition for items going into list 3</param>
    /// <returns>A list of tossed items as well as a remainder</returns>
    public static (List<T>, List<T>, List<T>, List<T>) Toss<T>(this IEnumerable<T> objects, Func<T, bool> condition1,
        Func<T, bool> condition2, Func<T, bool> condition3)
    {
        var result1 = new List<T>();
        var result2 = new List<T>();
        var result3 = new List<T>();
        var remaining = new List<T>();
        foreach (var item in objects)
        {
            if (condition1(item))
                result1.Add(item);
            else if (condition2(item))
                result2.Add(item);
            else if (condition2(item))
                result3.Add(item);
            else
                remaining.Add(item);
        }

        return (result1, result2, result3, remaining);
    }

    /// <summary>
    /// Walk a tree, returning all the nodes either depth-first or shallow-first.
    /// </summary>
    /// <param name="nodes">Node tree to operate on</param>
    /// <param name="childNodes">Selector for retrieving the list of child nodes for a given node</param>
    /// <param name="mode">Mode of operation, depth-first or shallow-first</param>
    /// <returns>An IEnumerable of nodes</returns>
    public static IEnumerable<T> WalkTree<T>(this ICollection<T> nodes, Func<T, ICollection<T>> childNodes, WalkTreeMode mode)
    {
        return mode switch
        {
            WalkTreeMode.DepthFirst => WalkTreeDepthFirst(nodes, childNodes),
            WalkTreeMode.ShallowFirst => WalkTreeShallowFirst(nodes, childNodes),
            _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
        };
    }

    private static IEnumerable<T> WalkTreeDepthFirst<T>(ICollection<T> nodes, Func<T, ICollection<T>> childNodes)
    {
        foreach (var node in nodes)
        {
            yield return node;
            foreach (var subNode in WalkTreeDepthFirst(childNodes(node), childNodes))
                yield return subNode;
        }
    }

    private static IEnumerable<T> WalkTreeShallowFirst<T>(ICollection<T> nodes, Func<T, ICollection<T>> childNodes)
    {
        foreach (var node in nodes)
            yield return node;

        foreach (var subNodes in nodes.Select(childNodes))
        foreach (var subNode in WalkTreeShallowFirst(subNodes, childNodes))
            yield return subNode;
    }

    /// <summary>
    /// Iterate through an enumerable and bring along an index counter
    /// </summary>
    public static IEnumerable<(T Item, int Index)> WithIndex<T>(this IEnumerable<T> source)
    {
        return source.Select((item, index) => (item, index));
    }
}