using Ext.Net;
using SoundInTheory.DynamicImage.Filters;
using System;
using System.Collections.Generic;
using System.EnterpriseServices;
using System.Linq;
using System.Runtime.InteropServices;
using System.Web;
using Zeus.BaseLibrary.Web.UI;
using Zeus.Collections;
using Zeus.ContentTypes;
using Zeus.Globalization.ContentTypes;

namespace Zeus.Admin.Plugins.Tree
{
    public class FilteredSiteTree : SiteTree
    {
        private readonly ContentItem _rootItem;

        private PartialHierarchyBuilder Builder => (PartialHierarchyBuilder)_treeBuilder;

        public FilteredSiteTree(ContentItem rootItem) 
            : base(null)
        {
            _rootItem = rootItem;
            _treeBuilder = new PartialHierarchyBuilder(_rootItem);
        }

        public FilteredSiteTree Query(string query)
        {
            Builder.Clear();

            if (string.IsNullOrWhiteSpace(query))
            {
                return this;
            }

            var items = Context.Current.Finder.QueryItems().Where(i => i.Title.Contains(query)).ToArray();

            Builder.Add(items);

            return this;
        }

        public override SiteTree OpenTo(ContentItem item)
        {
            Builder.Add(item);

            return this;
        }

        public override TreeNodeBase ToTreeNode(bool rootOnly, bool withLinks)
        {
            IHierarchyNavigator<ContentItem> navigator = new ItemHierarchyNavigator(_treeBuilder, _filter);
            TreeNodeBase rootNode = BuildNodesRecursive(navigator, rootOnly, withLinks, _filter);
            return rootNode;
        }

        private TreeNodeBase BuildNodesRecursive(IHierarchyNavigator<ContentItem> navigator, bool rootOnly, bool withLinks,
            Func<IEnumerable<ContentItem>, IEnumerable<ContentItem>> filter)
        {
            ContentItem item = navigator.Current;

            var node = CreateNode(item, asyncNode: false, withLinks);
            node.Expanded = true;
            node.Draggable = false;
            node.AllowDrag = false;
            node.AllowDrop = false;
            
            if (_treeBuilder is PartialHierarchyBuilder partialBuilder)
            {
                if (!partialBuilder.ContainsItem(item.ID))
                {
                    node.Disabled = true;
                    node.Cls = "disable-context";
                }
            }

            foreach (var childNavigator in navigator.Children)
            {
                var childNode = BuildNodesRecursive(childNavigator, rootOnly, withLinks, filter);
                ((TreeNode)node).Nodes.Add(childNode);
            }

            if (!navigator.Children.Any())
            {
                node.CustomAttributes.Add(new ConfigItem("children", "[]", ParameterMode.Raw));
            }

            return node;
        }
    }
}