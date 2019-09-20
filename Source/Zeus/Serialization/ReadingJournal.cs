using System;
using System.Collections.Generic;

namespace Zeus.Serialization
{
	public class ReadingJournal : IImportRecord
	{
		public event EventHandler<ItemEventArgs> ItemAdded;

		public IList<ContentItem> ReadItems { get; } = new List<ContentItem>();

		public ContentItem LastItem
		{
			get
			{
				if (ReadItems.Count == 0)
				{
					return null;
				}

				return ReadItems[ReadItems.Count - 1];
			}
		}

		public ContentItem RootItem
		{
			get
			{
				if (ReadItems.Count == 0)
				{
					return null;
				}

				return ReadItems[0];
			}
		}

		public IList<Exception> Errors { get; } = new List<Exception>();

		public void Report(ContentItem item)
		{
			ReadItems.Add(item);
			ItemAdded?.Invoke(this, new ItemEventArgs(item));
		}

		public ContentItem Find(int itemiD)
		{
			foreach (var previousItem in ReadItems)
			{
				if (previousItem.ID == itemiD)
				{
					return previousItem;
				}
			}

			return null;
		}

		public void Error(Exception ex)
		{
			Errors.Add(ex);
		}
	}
}