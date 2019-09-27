using Zeus.Persistence;
using System.Collections.Generic;
using System;

namespace Zeus.Admin.NavigationFlag
{
	public class NavigationCachingService : INavigationCachingService, IDisposable
	{
		private readonly IPersister _persister;

		public NavigationCachingService(IPersister persister)
		{
			_persister = persister;

			_persister.ItemDeleted += OnPersisterItemDeleted;
			_persister.ItemSaved += OnPersisterItemSaved;
		}

		private void OnPersisterItemDeleted(object sender, ItemEventArgs e)
		{
			DeleteCachedImages(e.AffectedItem);
		}

		private void OnPersisterItemSaved(object sender, ItemEventArgs e)
		{
			DeleteCachedImages(e.AffectedItem);
		}

		public void DeleteCachedImages(ContentItem contentItem)
		{
			//any time anything is saved or changed, delete all the primary nav app cache data
			if (contentItem.IsPage)
			{
				var keysToRemove = new List<string>();
				var enumerator = System.Web.HttpContext.Current.Cache.GetEnumerator();
				while (enumerator.MoveNext())
				{
					var key = (string)enumerator.Key;
					if (key.StartsWith("primaryNav"))
					{
						keysToRemove.Add(key);
					}
				}

				foreach (var key in keysToRemove)
				{
					System.Web.HttpContext.Current.Cache.Remove(key);
				}
			}
		}

		#region IDisposable Support
		private bool disposedValue = false; // To detect redundant calls

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					// TODO: dispose managed state (managed objects).

					_persister.ItemDeleted -= OnPersisterItemDeleted;
					_persister.ItemSaved -= OnPersisterItemSaved;
				}

				// TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
				// TODO: set large fields to null.

				disposedValue = true;
			}
		}

		// This code added to correctly implement the disposable pattern.
		public void Dispose()
		{
			// Do not change this code. Put cleanup code in Dispose(bool disposing) above.
			Dispose(true);
		}
		#endregion
	}
}