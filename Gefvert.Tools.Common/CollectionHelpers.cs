using System;
using System.Collections.Generic;
using System.Linq;

namespace Gefvert.Tools.Common
{
  public static class CollectionHelpers
  {
    public static void AddIfNotNull<T>(this ICollection<T> collection, T item)
    {
      if (collection != null && item != null)
        collection.Add(item);
    }

    public static void AddIfNotNull<T>(this ICollection<T> collection, IEnumerable<T> items)
    {
      if (collection == null || items == null)
        return;

      foreach(var item in items)
        if (item != null)
          collection.Add(item);
    }

    public static T ExtractAt<T>(this IList<T> list, int position)
    {
      var result = list[position];
      list.RemoveAt(position);

      return result;
    }

    public static List<T> ExtractAll<T>(this IList<T> list, Predicate<T> match)
    {
      var result = list.Where(x => match(x)).ToList();
      foreach(var item in result)
        list.Remove(item);

      return result;
    }

    public static T ExtractFirst<T>(this IList<T> list)
    {
      return ExtractAt(list, 0);
    }

    public static T ExtractLast<T>(this IList<T> list)
    {
      return ExtractAt(list, list.Count - 1);
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
  }
}
