﻿using System;
using System.Linq;
using Coolite.Ext.Web;
using Zeus.BaseLibrary.Web;
using Zeus.Collections;
using System.Collections.Generic;

namespace Zeus.Admin.Plugins.Tree
{
	public class SiteTree
	{
		private readonly HierarchyBuilder _treeBuilder;
		private Func<IEnumerable<ContentItem>, IEnumerable<ContentItem>> _filter;
		private bool _excludeRoot, _forceSync;

		public SiteTree(HierarchyBuilder treeBuilder)
		{
			_treeBuilder = treeBuilder;
		}

		public static SiteTree From(ContentItem rootItem)
		{
			return new SiteTree(new TreeHierarchyBuilder(rootItem));
		}

		public static SiteTree From(ContentItem rootItem, int maxDepth)
		{
			return new SiteTree(new TreeHierarchyBuilder(rootItem, maxDepth));
		}

		public static SiteTree Between(ContentItem initialItem, ContentItem lastAncestor, bool appendAdditionalLevel)
		{
			return new SiteTree(new BranchHierarchyBuilder(initialItem, lastAncestor, appendAdditionalLevel));
		}

		public SiteTree ExcludeRoot(bool exclude)
		{
			_excludeRoot = exclude;
			return this;
		}

		public SiteTree Filter(Func<IEnumerable<ContentItem>, IEnumerable<ContentItem>> filter)
		{
			_filter = filter;
			return this;
		}

		public SiteTree ForceSync()
		{
			_forceSync = true;
			return this;
		}

		public SiteTree OpenTo(ContentItem item)
		{
			IList<ContentItem> items = Find.ListParents(item);
			//return ClassProvider(c => (items.Contains(c) || c == item) ? "open" : string.Empty);
			return this;
		}

		public TreeNodeBase ToTreeNode(bool rootOnly)
		{
			return ToTreeNode(rootOnly, true);
		}

		public TreeNodeBase ToTreeNode(bool rootOnly, bool withLinks)
		{
			IHierarchyNavigator<ContentItem> navigator = new ItemHierarchyNavigator(_treeBuilder, _filter);
			TreeNodeBase rootNode = BuildNodesRecursive(navigator, rootOnly, withLinks);
			//rootNode.ChildrenOnly = _excludeRoot;
			return rootNode;
		}

		private static TreeNodeBase BuildNodesRecursive(IHierarchyNavigator<ContentItem> navigator, bool rootOnly, bool withLinks)
		{
			ContentItem item = navigator.Current;

			bool hasAsyncChildren = ((!navigator.Children.Any() && item.GetChildren().Any()) || rootOnly);
			TreeNodeBase node = (hasAsyncChildren) ? new AsyncTreeNode() as TreeNodeBase : new TreeNode();
			node.Text = ((INode) item).Contents;
			node.IconFile = item.IconUrl;
			node.IconCls = "zeus-tree-icon";
			node.Cls = "zeus-tree-node";
			node.NodeID = item.ID.ToString();
			if (withLinks)
			{
				// Allow plugin to set the href (it will be based on whatever is the default context menu plugin).
				foreach (ITreePlugin treePlugin in Context.Current.ResolveAll<ITreePlugin>())
					treePlugin.ModifyTreeNode(node, item);
			}

			if (!hasAsyncChildren)
				foreach (IHierarchyNavigator<ContentItem> childNavigator in navigator.Children)
				{
					TreeNodeBase childNode = BuildNodesRecursive(childNavigator, rootOnly, withLinks);
					((TreeNode) node).Nodes.Add(childNode);
				}
			if (!item.GetChildren().Any())
			{
				node.CustomAttributes.Add(new ConfigItem("children", "[]", ParameterMode.Raw));
				node.Expanded = true;
			}
			else if (navigator.Children.Any())
				node.Expanded = true;
			return node;
		}
	}
}