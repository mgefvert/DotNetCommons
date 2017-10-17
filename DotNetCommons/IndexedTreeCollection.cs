using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

// Written by Mats Gefvert
// Distributed under MIT License: https://opensource.org/licenses/MIT

namespace DotNetCommons
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

        public TreeNode<T> FindParentAndAdd(TKey parent, T item)
        {
            var node = Find(parent);
            return node == null ? AddRoot(item) : node.AddChild(item);
        }

        public TreeNode<T> Find(TKey key)
        {
            return _index.TryGetValue(key, out var node) ? node : null;
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
