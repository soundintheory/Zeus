using NHibernate.Criterion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zeus.Collections
{
    public class PartialHierarchyBuilder : HierarchyBuilder
    {
        private readonly ContentItem _rootItem;

        private readonly Dictionary<int, ContentItem> _items;

        private HierarchyNode<ContentItem> _rootNode;

        private readonly Dictionary<int, HierarchyNode<ContentItem>> _nodes;

        public PartialHierarchyBuilder(ContentItem rootItem)
        {
            _rootItem = rootItem;
            _items = new Dictionary<int, ContentItem>();
            _nodes = new Dictionary<int, HierarchyNode<ContentItem>>();
        }

        public void Add(params ContentItem[] items)
        {
            foreach (var item in items)
            {
                _items[item.ID] = item;
            }
        }

        public bool ContainsItem(int id) => _items.ContainsKey(id);

        public void Clear()
        {
            _items.Clear();
        }

        public override HierarchyNode<ContentItem> Build()
        {
            ResetNodes();

            foreach (ContentItem item in FilterItems(_items.Values.ToList()).OrderBy(x => x.SortOrder))
            {
                GetOrAddNode(item);
            }

            return _rootNode;
        }

        protected virtual HierarchyNode<ContentItem> GetOrAddNode(ContentItem item)
        {
            if (_nodes.TryGetValue(item.ID, out var node))
            {
                return node;
            }

            var hierarchy = GetHierarchy(item);
            var currentNode = _rootNode;

            if (hierarchy.Count == 0)
            {
                return null;
            }

            foreach (var currentItem in hierarchy.Skip(1))
            {
                if (!_nodes.TryGetValue(currentItem.ID, out node))
                {
                    node = new HierarchyNode<ContentItem>(currentItem);
                    _nodes[currentItem.ID] = node;
                    currentNode.Children.Add(node);
                }

                currentNode = node;
            }

            return currentNode;
        }

        protected virtual List<ContentItem> GetHierarchy(ContentItem item)
        {
            var hierarchy = new List<ContentItem> { item };

            while (item != null && item.ID != _rootItem.ID && item.Parent != null)
            {
                hierarchy.Insert(0, item.Parent);
                item = item.Parent;
            }

            if (hierarchy[0].ID != _rootItem.ID || FilterItems(hierarchy).Count != hierarchy.Count)
            {
                return new List<ContentItem>();
            }

            return hierarchy;
        }

        protected void ResetNodes()
        {
            _nodes.Clear();
            _rootNode = new HierarchyNode<ContentItem>(_rootItem);
            _nodes[_rootItem.ID] = _rootNode;
        }

        protected List<ContentItem> FilterItems(List<ContentItem> items)
        {
            if (Filter != null)
            {
                return Filter(items).ToList();
            }

            return items.ToList();
        }
    }
}
