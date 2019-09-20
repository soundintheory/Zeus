using System;
using System.Collections.Generic;

namespace Zeus.Collections
{
	/// <summary>
	/// Navigates a graph of content item nodes.
	/// </summary>
	public class ItemHierarchyNavigator : IHierarchyNavigator<ContentItem>
	{
		#region Fields


		#endregion

		#region Constructors

		public ItemHierarchyNavigator(HierarchyNode<ContentItem> currentNode)
		{
			CurrentNode = currentNode;
		}

		public ItemHierarchyNavigator(HierarchyBuilder builder, Func<IEnumerable<ContentItem>, IEnumerable<ContentItem>> filter)
		{
			CurrentNode = builder.Build(filter);
		}

		public ItemHierarchyNavigator(HierarchyBuilder builder)
		{
			CurrentNode = builder.Build();
		}

		#endregion

		#region Properties

		public HierarchyNode<ContentItem> CurrentNode { get; }

		public IHierarchyNavigator<ContentItem> Parent
		{
			get
			{
				if (CurrentNode.Parent != null)
				{
					return new ItemHierarchyNavigator(CurrentNode.Parent);
				}

				return null;
			}
		}

		public IEnumerable<IHierarchyNavigator<ContentItem>> Children
		{
			get
			{
				foreach (var childNode in CurrentNode.Children)
				{
					yield return new ItemHierarchyNavigator(childNode);
				}
			}
		}

		public ContentItem Current
		{
			get { return CurrentNode.Current; }
		}

		public bool HasChildren
		{
			get { return CurrentNode.Children.Count > 0; }
		}

		#endregion

		#region Methods

		public ItemHierarchyNavigator GetRootHierarchy()
		{
			return new ItemHierarchyNavigator(GetRootNode());
		}

		public HierarchyNode<ContentItem> GetRootNode()
		{
			var last = CurrentNode;
			while (last.Parent != null)
			{
				last = last.Parent;
			}

			return last;
		}

		public IEnumerable<ContentItem> EnumerateAllItems()
		{
			var rootNode = GetRootNode();
			return EnumerateItemsRecursive(rootNode);
		}

		public IEnumerable<ContentItem> EnumerateChildItems()
		{
			return EnumerateItemsRecursive(CurrentNode);
		}

		protected virtual IEnumerable<ContentItem> EnumerateItemsRecursive(HierarchyNode<ContentItem> node)
		{
			yield return node.Current;
			foreach (var childNode in node.Children)
			{
				foreach (var childItem in EnumerateItemsRecursive(childNode))
				{
					yield return childItem;
				}
			}
		}

		#endregion
	}
}
