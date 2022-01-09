using System;
using System.Collections.Generic;
using System.Linq;

// Written by Mats Gefvert
// Distributed under MIT License: https://opensource.org/licenses/MIT
// ReSharper disable UnusedMember.Global

namespace DotNetCommons.Collections;

public static class CollectionLinker
{
    public static void LinkToOne<TSource, TTarget, TKey>(ICollection<TSource> source, ICollection<TTarget> target, 
        Func<TSource, TKey> sourceSelector, Func<TTarget, TKey> targetSelector, Action<TSource, TTarget> assign)
    {
        if (source == null || target == null)
            return;

        var lookup = target.ToDictionary(targetSelector);
        foreach (var item in source)
        {
            var key = sourceSelector(item);
            if (key != null && lookup.TryGetValue(key, out var found))
                assign(item, found);
        }
    }

    public static void LinkToMany<TSource, TTarget, TKey>(ICollection<TSource> source, ICollection<TTarget> target, 
        Func<TSource, TKey> sourceSelector, Func<TTarget, TKey> targetSelector, Action<TSource, IEnumerable<TTarget>> assign)
    {
        if (source == null || target == null)
            return;

        var lookup = target.ToLookup(targetSelector);
        foreach (var item in source)
        {
            var key = sourceSelector(item);
            if (key != null)
                assign(item, lookup[key]);
        }
    }
}