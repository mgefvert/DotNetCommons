using System;
using System.Collections.Generic;
using System.Linq;

// Written by Mats Gefvert
// Distributed under MIT License: https://opensource.org/licenses/MIT

namespace DotNetCommons
{
    public class TreeCollection<T>
    {
        private readonly List<TreeNode<T>> _roots;
        public IReadOnlyList<TreeNode<T>> Roots { get; }

        public TreeCollection()
        {
            _roots = new List<TreeNode<T>>();
            Roots = _roots.AsReadOnly();
        } 

        public TreeNode<T> AddRoot(T item)
        {
            var node = new TreeNode<T>(this, item, null);
            NotifyAdd(node);
            _roots.Add(node);
            return node;
        }

        internal virtual void NotifyAdd(TreeNode<T> node)
        {
        }

        internal virtual void NotifyRemove(TreeNode<T> node)
        {
        }

        public void RemoveRoot(T item)
        {
            var nodes = _roots.ExtractAll(c => c.Item.Equals(item));
            foreach (var node in nodes)
                NotifyRemove(node);
        }

        public IEnumerable<T> Recurse()
        {
            return _roots.SelectMany(c => c.InternalRecurse()).Select(c => c.Item);
        }

        public IEnumerable<TResult> Recurse<TResult>(Func<int, T, TResult> selector)
        {
            return _roots.SelectMany(x => x.InternalRecurse(0, selector));
        }
    }
}
