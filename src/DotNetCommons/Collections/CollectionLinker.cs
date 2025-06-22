// Written by Mats Gefvert
// Distributed under MIT License: https://opensource.org/licenses/MIT
// ReSharper disable UnusedMember.Global

namespace DotNetCommons.Collections;

public static class CollectionLinker
{
    /// <summary>
    /// Link two collections of objects together, using source and target selectors to select index entries, where
    /// one object in the source list links to one single object in the target list.
    /// </summary>
    /// <param name="source">List of source objects</param>
    /// <param name="target">List of target objects to link to</param>
    /// <param name="sourceSelector">Selector for the index key in the source list</param>
    /// <param name="targetSelector">Selector for the index key in the target list</param>
    /// <param name="assign">Assignment function that lets the caller perform the linking by assigning the target object to the source</param>
    public static void LinkToOne<TSource, TTarget, TKey>(ICollection<TSource> source, ICollection<TTarget> target,
        Func<TSource, TKey> sourceSelector, Func<TTarget, TKey> targetSelector, Action<TSource, TTarget> assign) where TKey : notnull
    {
        var lookup = target.ToDictionary(targetSelector);
        foreach (var item in source)
        {
            var key = sourceSelector(item);
            if (lookup.TryGetValue(key, out var found))
                assign(item, found);
        }
    }

    /// <summary>
    /// Link two collections of objects together, using source and target selectors to select index entries, where
    /// one object in the source list links to several objects in the target list.
    /// </summary>
    /// <param name="source">List of source objects</param>
    /// <param name="target">List of target objects to link to</param>
    /// <param name="sourceSelector">Selector for the index key in the source list</param>
    /// <param name="targetSelector">Selector for the index key in the target list</param>
    /// <param name="assign">Assignment function that lets the caller perform the linking by assigning the target object to the source</param>
    public static void LinkToMany<TSource, TTarget, TKey>(ICollection<TSource> source, ICollection<TTarget> target,
        Func<TSource, TKey> sourceSelector, Func<TTarget, TKey> targetSelector, Action<TSource, IEnumerable<TTarget>> assign)
    {
        var lookup = target.ToLookup(targetSelector);
        foreach (var item in source)
        {
            var key = sourceSelector(item);
            if (key != null)
                assign(item, lookup[key]);
        }
    }
}