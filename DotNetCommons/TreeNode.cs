using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

// Written by Mats Gefvert
// Distributed under MIT License: https://opensource.org/licenses/MIT

namespace DotNetCommons
{
    public class TreeNode<T>
    {
        private readonly List<TreeNode<T>> _children;

        public IReadOnlyList<TreeNode<T>> Children { get; }
        public bool HasChildren => Children.Count != 0;
        public T Item { get; set; }
        public TreeNode<T> Parent { get; }
        public TreeCollection<T> Collection { get; }

        public TreeNode()
        {
            _children = new List<TreeNode<T>>();
            Children = new ReadOnlyCollection<TreeNode<T>>(_children);
        } 

        public TreeNode(TreeCollection<T> collection, T item, TreeNode<T> parent) : this()
        {
            Item = item;
            Collection = collection;
            Parent = parent;
        }

        public TreeNode<T> AddChild(T item)
        {
            var node = new TreeNode<T>(Collection, item, this);
            Collection.NotifyAdd(node);
            _children.Add(node);
            return node;
        }

        internal IEnumerable<TreeNode<T>> InternalRecurse()
        {
            yield return this;
            foreach (var item in _children.SelectMany(n => n.InternalRecurse()))
                yield return item;
        }

        public IEnumerable<TResult> InternalRecurse<TResult>(int level, Func<int, T, TResult> selector)
        {
            yield return selector(level, Item);
            foreach (var child in _children.SelectMany(x => x.InternalRecurse(level + 1, selector)))
                yield return child;
        }

        public IEnumerable<T> Recurse()
        {
            return InternalRecurse().Select(n => n.Item);
        }

        public IEnumerable<TResult> Recurse<TResult>(Func<int, T, TResult> selector)
        {
            return InternalRecurse(0, selector);
        }

        public void Remove()
        {
            if (Parent != null)
                Parent.RemoveChild(Item);
            else
                Collection.RemoveRoot(Item);
        }

        public void RemoveChild(T item)
        {
            var nodes = _children.ExtractAll(c => c.Item.Equals(item)).SelectMany(c => c.InternalRecurse());
            foreach(var node in nodes)
                Collection.NotifyRemove(node);
        }
    }
}
