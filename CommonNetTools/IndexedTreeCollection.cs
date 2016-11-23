using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

// Written by Mats Gefvert
// Distributed under MIT License: https://opensource.org/licenses/MIT

namespace CommonNetTools
{
    public class IndexedTreeCollection<T, TKey> : TreeCollection<T>
    {
        private readonly Func<T, TKey> _keySelector;
        private readonly Dictionary<TKey, TreeNode<T>> _index;
        public IReadOnlyDictionary<TKey, TreeNode<T>> Index { get; }

        public IndexedTreeCollection(Func<T, TKey> keySelector)
        {
            _keySelector = keySelector;
            _index = new Dictionary<TKey, TreeNode<T>>();
            Index = new ReadOnlyDictionary<TKey, TreeNode<T>>(_index);
        }

        public IndexedTreeCollection(Func<T, TKey> keySelector, IEqualityComparer<TKey> keyComparer)
        {
            _keySelector = keySelector;
            _index = new Dictionary<TKey, TreeNode<T>>(keyComparer);
            Index = new ReadOnlyDictionary<TKey, TreeNode<T>>(_index);
        }

        public TreeNode<T> Find(TKey key)
        {
            TreeNode<T> node;
            return _index.TryGetValue(key, out node) ? node : null;
        }

        internal override void NotifyAdd(TreeNode<T> node)
        {
            _index.Add(_keySelector(node.Item), node);
        }

        internal override void NotifyRemove(TreeNode<T> node)
        {
            _index.Remove(_keySelector(node.Item));
        }
    }
}
