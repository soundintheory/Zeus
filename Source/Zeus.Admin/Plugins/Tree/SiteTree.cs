using System;
using System.Linq;
using Ext.Net;
using Zeus.BaseLibrary.Web.UI;
using Zeus.Collections;
using System.Collections.Generic;
using Zeus.Globalization.ContentTypes;
using System.EnterpriseServices;
using System.Xml.Linq;

namespace Zeus.Admin.Plugins.Tree
{
	public class SiteTree
	{
		protected HierarchyBuilder _treeBuilder;
		protected Func<IEnumerable<ContentItem>, IEnumerable<ContentItem>> _filter;

		public SiteTree(HierarchyBuilder treeBuilder)
		{
			_treeBuilder = treeBuilder;
		}

        public static FilteredSiteTree Filtered(string query)
        {
            var tree = new FilteredSiteTree(Find.RootItem);
			tree.Query(query);	

			return tree;
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

		public SiteTree Filter(Func<IEnumerable<ContentItem>, IEnumerable<ContentItem>> filter)
		{
			_filter = filter;
			return this;
		}

		public SiteTree ForceSync()
		{
			return this;
		}

		public virtual SiteTree OpenTo(ContentItem item)
		{
			return this;
		}

		public virtual TreeNodeBase ToTreeNode(bool rootOnly)
		{
			return ToTreeNode(rootOnly, true);
		}

		public virtual TreeNodeBase ToTreeNode(bool rootOnly, bool withLinks)
		{
			IHierarchyNavigator<ContentItem> navigator = new ItemHierarchyNavigator(_treeBuilder, _filter);
			TreeNodeBase rootNode = BuildNodesRecursive(navigator, rootOnly, withLinks, _filter);
			return rootNode;
		}

		private static TreeNodeBase BuildNodesRecursive(IHierarchyNavigator<ContentItem> navigator, bool rootOnly, bool withLinks,
			Func<IEnumerable<ContentItem>, IEnumerable<ContentItem>> filter)
		{
			ContentItem item = navigator.Current;

			ContentItem translatedItem;
			TranslationStatus translationStatus = GetTranslationStatus(item, out translatedItem);

			var itemChildren = item.GetChildren();
			if (filter != null)
				itemChildren = filter(itemChildren);
			bool hasAsyncChildren = ((!navigator.Children.Any() && itemChildren.Any()) || rootOnly);

			var node = CreateNode(item, hasAsyncChildren, withLinks);
			node.Text = ((INode) translatedItem).Contents;

            if (translationStatus == TranslationStatus.NotAvailableInSelectedLanguage)
                node.Cls += " notAvailableInSelectedLanguage";

            if (translationStatus == TranslationStatus.NotAvailableInSelectedLanguage || translationStatus == TranslationStatus.DisplayedInAnotherLanguage)
			{
				node.Text += "&nbsp;";
				switch (translationStatus)
				{
					case TranslationStatus.NotAvailableInSelectedLanguage:
						node.Text += "<img src='" + WebResourceUtility.GetUrl(typeof(SiteTree), "Zeus.Admin.Assets.Images.Icons.LanguageMissing.gif") + "' title='Page is missing for the current language and will not be displayed.' />";
						break;
					case TranslationStatus.DisplayedInAnotherLanguage:
						Language language = Context.Current.LanguageManager.GetLanguage(translatedItem.Language);
						node.Text += "<img src='" + language.IconUrl + "' title='Page is displayed in another language (" + language.Title + ").' />";
						break;
				}
			}

			if (!hasAsyncChildren)
			{
				// Allow for grouping of child items into folders.
				var folderGroups = navigator.Children
					.Select(ci => ci.Current.FolderPlacementGroup)
					.Where(s => s != null)
					.Distinct();

                int uniqueCount = 0;
				foreach (string folderGroup in folderGroups)
				{
                    uniqueCount += 100000;
                    TreeNode folderNode = new TreeNode
                    {
                        Text = folderGroup,
                        IconFile = Utility.GetCooliteIconUrl(Icon.FolderGo),
                        Cls = "zeus-tree-node disable-context",
                        Expanded = false,
                        // TODO: Check why this was necessary
                        //NodeID = (-1 * (Int32.Parse(node.NodeID) + uniqueCount)).ToString()
                        NodeID = node.NodeID + uniqueCount
					};

					((TreeNode) node).Nodes.Add(folderNode);
					foreach (IHierarchyNavigator<ContentItem> childNavigator in navigator.Children.Where(n => n.Current.FolderPlacementGroup == folderGroup))
					{
						TreeNodeBase childNode = BuildNodesRecursive(childNavigator, rootOnly, withLinks, filter);
						folderNode.Nodes.Add(childNode);
					}
				}
				foreach (IHierarchyNavigator<ContentItem> childNavigator in navigator.Children.Where(n => n.Current.FolderPlacementGroup == null))
				{
					TreeNodeBase childNode = BuildNodesRecursive(childNavigator, rootOnly, withLinks, filter);
					((TreeNode) node).Nodes.Add(childNode);
				}
			}
			if (!itemChildren.Any())
			{
				node.CustomAttributes.Add(new ConfigItem("children", "[]", ParameterMode.Raw));
				node.Expanded = true;
			}
			else if (navigator.Children.Any())
				node.Expanded = true;
			return node;
		}

		public static TreeNodeBase CreateNode(ContentItem item, bool asyncNode = false, bool withLinks = false)
		{
            TreeNodeBase node = asyncNode ? new AsyncTreeNode() as TreeNodeBase : new TreeNode();

            node.Text = ((INode)item).Contents;
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

			return node;
        }

		private static TranslationStatus GetTranslationStatus(ContentItem contentItem, out ContentItem translatedItem)
		{
			translatedItem = contentItem;

			if (contentItem == null)
				return TranslationStatus.Available;

			if (!Zeus.Context.Current.LanguageManager.CanBeTranslated(contentItem))
				return TranslationStatus.Available;

			if (string.IsNullOrEmpty(contentItem.Language))
				return TranslationStatus.Available;

			string languageCode = Zeus.Context.AdminManager.CurrentAdminLanguageBranch;
			ContentItem testTranslatedItem = Zeus.Context.Current.LanguageManager.GetTranslation(contentItem, languageCode);
			if (testTranslatedItem != null)
				translatedItem = testTranslatedItem;

			if (Zeus.Context.Current.LanguageManager.TranslationExists(contentItem, languageCode))
				return TranslationStatus.Available;

			if (testTranslatedItem == null)
				return TranslationStatus.NotAvailableInSelectedLanguage;

			if (translatedItem.Language != languageCode)
				return TranslationStatus.DisplayedInAnotherLanguage;

			return TranslationStatus.Available;
		}

		private enum TranslationStatus
		{
			Available,

			/// <summary>
			/// Page is missing for the current language and will not be displayed.
			/// </summary>
			NotAvailableInSelectedLanguage,

			/// <summary>
			/// Page is displayed in another language (English).
			/// </summary>
			DisplayedInAnotherLanguage
		}
	}
}