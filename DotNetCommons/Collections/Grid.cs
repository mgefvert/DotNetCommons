using System;
using System.Collections.Generic;
using System.Linq;

// Written by Mats Gefvert
// Distributed under MIT License: https://opensource.org/licenses/MIT

namespace DotNetCommons.Collections
{
    public class Grid<TRow, TCol, TData> 
    {
        private readonly Dictionary<Tuple<TRow, TCol>, TData> _data = new Dictionary<Tuple<TRow, TCol>, TData>();

        protected Tuple<TRow, TCol> Key(TRow row, TCol column) => new Tuple<TRow, TCol>(row, column);

        public IEnumerable<TCol> AllColumns()
        {
            return _data.Keys.Select(x => x.Item2).Distinct();
        }

        public IEnumerable<TRow> AllRows()
        {
            return _data.Keys.Select(x => x.Item1).Distinct();
        }

        public TData Extract(TRow row, TCol column, TData defaultValue = default(TData))
        {
            var key = Key(row, column);
            var result = Get(key, defaultValue);
            _data.Remove(key);
            return result;
        }

        protected TData Get(Tuple<TRow, TCol> key, TData defaultValue)
        {
            return _data.TryGetValue(key, out var result)
                ? result
                : defaultValue;
        }

        public TData Get(TRow row, TCol column, TData defaultValue = default(TData))
        {
            return Get(Key(row, column), defaultValue);
        }

        public TData Manipulate(TRow row, TCol column, Func<TData, TData> func, TData defaultValue = default(TData))
        {
            var key = Key(row, column);
            var value = Get(key, defaultValue);
            return _data[key] = func(value);
        }

        public void Set(TRow row, TCol column, TData value)
        {
            _data[Key(row, column)] = value;
        }
    }
}
