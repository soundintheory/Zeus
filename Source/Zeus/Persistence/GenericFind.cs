﻿using System;
using System.Collections.Generic;
using System.Linq;
using Zeus.Collections;
using Zeus.FileSystem;
using Zeus.Web;
using Zeus.Security;
using Zeus.Web.Security.Items;
using Zeus.Security.ContentTypes;

namespace Zeus.Persistence
{
	public abstract class GenericFind<TRoot, TStart>
		where TRoot : ContentItem
		where TStart : ContentItem
	{
		/// <summary>Gets the currently displayed page (based on the query string).</summary>
		public static ContentItem CurrentPage
		{
			get { return Context.CurrentPage; }
		}

		/// <summary>Gets an enumeration of pages leading to the current page.</summary>
		public static IEnumerable<ContentItem> Parents
		{
			get
			{
				ContentItem startPage = StartPage;
				ContentItem item = CurrentPage;
				return EnumerateParents(item, startPage);
			}
		}

		/// <summary>Gets the site's root items.</summary>
		public static TRoot RootItem
		{
			//get { return (TStart) Context.Current.UrlParser.RootItem; }
			get { return (TRoot) Context.Persister.Load(Context.Current.Host.CurrentSite.RootItemID); }
		}

		/// <summary>Gets the current start page (this may vary depending on host url).</summary>
		public static TStart StartPage
		{
			get { return (TStart) Context.Current.UrlParser.StartPage; }
		}

        public static T FirstOfType<T>() where T : ContentItem
        {
            // Try and get the cached first ID of the type. This will also return the item if it was
            // freshly retrieved from the persister
            var firstId = Context.Cache.GetFirstOfTypeID<T>(out var item);

            // Use item if possible
            if (item != default)
            {
                return item;
            }

            // We have a cached ID. The call to Perister.Get() is a lot faster than the alternative!
            if (firstId > 0)
            {
                return Context.Persister.Get<T>(firstId);
            }

            return default;
        }

        public static T RootItemOfType<T>() => StartPage.GetChildren<T>().FirstOrDefault();

        /// <summary>
        /// Gets the item at the specified level.
        /// </summary>
        /// <param name="level">Level = 1 equals start page, level = 2 a child of the start page, and so on.</param>
        /// <returns>An ancestor at the specified level.</returns>
        public static ContentItem AncestorAtLevel(int level)
		{
			return AncestorAtLevel(level, Parents, CurrentPage);
		}

		/// <summary>
		/// Gets the item at the specified level.
		/// </summary>
		/// <param name="level">Level = 1 equals start page, level = 2 a child of the start page, and so on.</param>
		/// <returns>An ancestor at the specified level.</returns>
		public static ContentItem AncestorAtLevel(int level, IEnumerable<ContentItem> parents, ContentItem currentPage)
		{
			IList<ContentItem> items = parents.ToList();
			if (items.Count >= level)
				return items[items.Count - level];
			else if (items.Count == level - 1)
				return currentPage;
			return null;
		}

		/// <summary>Enumerates child items and their children, and so on.</summary>
		/// <param name="item">The parent item whose child items to enumerate. The item itself is not returned.</param>
		/// <returns>An enumeration of all children of an item.</returns>
		public static IEnumerable<ContentItem> EnumerateChildren(ContentItem item)
		{
			if (item.VersionOf != null) item = item.VersionOf;

			foreach (ContentItem child in item.Children)
			{
				yield return child;
				foreach (ContentItem childItem in EnumerateChildren(child))
					yield return childItem;
			}
		}

		/// <summary>Enumerates child items and their children, and so on.</summary>
		/// <param name="item">The parent item whose child items to enumerate. The item itself is not returned.</param>
		/// <returns>An enumeration of all children of an item.</returns>
		public static IEnumerable<ContentItem> EnumerateAccessibleChildren(ContentItem item)
		{
            int depth = 200;
			if (item.VersionOf != null) item = item.VersionOf;

			foreach (ContentItem child in item.GetChildren())
			{
				yield return child;
				foreach (ContentItem childItem in EnumerateAccessibleChildren(child, depth))
					yield return childItem;
			}
		}

        /// <summary>Enumerates child items and their children, and so on.</summary>
        /// <param name="item">The parent item whose child items to enumerate. The item itself is not returned.</param>
        /// <returns>An enumeration of all children of an item.</returns>
        public static IEnumerable<ContentItem> EnumerateAccessibleChildren(ContentItem item, int depth)
        {
            if (item.VersionOf != null) item = item.VersionOf;

            foreach (ContentItem child in item.GetChildren())
            {
                yield return child;
                if (depth > 1)
                {                    
                    foreach (ContentItem childItem in EnumerateAccessibleChildren(child, depth - 1))
                        yield return childItem;
                }
            }
        }

		public static bool IsAccessibleChildOrSelf(ContentItem item, ContentItem wantedItem)
		{
			return (item == wantedItem) || In(wantedItem, EnumerateAccessibleChildren(item));
		}

		public static IList<ContentItem> ListParents(ContentItem initialItem)
		{
			return new List<ContentItem>(EnumerateParents(initialItem, null, false));
		}

		/// <summary>Enumerates parents of the initial item.</summary>
		/// <param name="initialItem">The page whose parents will be enumerated. The page itself will not appear in the enumeration.</param>
		/// <returns>An enumeration of the parents of the initial page.</returns>
		public static IEnumerable<ContentItem> EnumerateParents(ContentItem initialItem)
		{
			return EnumerateParents(initialItem, null);
		}

		/// <summary>Enumerates parents of the initial item.</summary>
		/// <param name="initialItem">The page whose parents will be enumerated. The page itself will not appear in the enumeration.</param>
		/// <param name="lastAncestor">The last page of the enumeration. The enumeration will contain this page.</param>
		/// <returns>An enumeration of the parents of the initial page. If the last page isn't a parent of the inital page all pages until there are no more parents are returned.</returns>
		public static IEnumerable<ContentItem> EnumerateParents(ContentItem initialItem, ContentItem lastAncestor)
		{
			return EnumerateParents(initialItem, lastAncestor, false);
		}

		/// <summary>Enumerates parents of the initial item.</summary>
		/// <param name="initialItem">The page whose parents will be enumerated. The page itself will appear in the enumeration if includeSelf is applied.</param>
		/// <param name="lastAncestor">The last page of the enumeration. The enumeration will contain this page.</param>
		/// <param name="includeSelf">Include the lanitialItem in the enumeration.</param>
		/// <returns>An enumeration of the parents of the initial page. If the last page isn't a parent of the inital page all pages until there are no more parents are returned.</returns>
		public static IEnumerable<ContentItem> EnumerateParents(ContentItem initialItem, ContentItem lastAncestor, bool includeSelf)
		{
			if (initialItem == null) yield break;

			ContentItem item;
			if (includeSelf)
				item = initialItem;
			else if (initialItem != lastAncestor)
				item = initialItem.GetParent();
			else
				yield break;

			while (item != null)
			{
				yield return item;
				if (item == lastAncestor)
					break;
				item = item.GetParent();
			}
		}

		/// <summary>Determines wether an item is below a certain ancestral item or is the ancestral item.</summary>
		/// <param name="item">The item to check for beeing a child or descendant.</param>
		/// <param name="ancestor">The item to check for beeing parent or ancestor.</param>
		/// <returns>True if the item is descendant the ancestor.</returns>
		public static bool IsDescendantOrSelf(ContentItem item, ContentItem ancestor)
		{
			if (item == null) throw new ArgumentNullException("item");
			if (ancestor == null) throw new ArgumentNullException("ancestor");

			return item == ancestor || In(ancestor, EnumerateParents(item));
		}

		/// <summary>Determines wether an item is in a enumeration of items.</summary>
		/// <param name="wantedItem">The item to look for.</param>
		/// <param name="linedUpItems">The items to look among.</param>
		/// <returns>True if the item is in the enumeration of items.</returns>
		public static bool In(ContentItem wantedItem, IEnumerable<ContentItem> linedUpItems)
		{
			if (wantedItem == null) throw new ArgumentNullException("wantedItem");
			if (linedUpItems == null) throw new ArgumentNullException("linedUpItems");

			foreach (ContentItem enumeratedItem in linedUpItems)
			{
				if (enumeratedItem == wantedItem)
					return true;
			}
			return false;
		}

        public static UserContainer UserContainer()
        {
            return RootItem.GetChildren<SystemNode>().Single().GetChildren<SecurityContainer>().Single().GetChildren<UserContainer>().Single();
        }
	}
}