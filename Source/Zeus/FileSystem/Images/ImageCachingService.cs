using SoundInTheory.DynamicImage.Caching;
using System;
using Zeus.Persistence;

namespace Zeus.FileSystem.Images
{
	public class ImageCachingService : IDisposable
	{
		private readonly IPersister _persister;

		public ImageCachingService(IPersister persister)
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
			if (contentItem is Image)
			{
				var source = new ZeusImageSource { ContentID = contentItem.ID };
				DynamicImageCacheManager.Remove(source);
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
			// TODO: uncomment the following line if the finalizer is overridden above.
			// GC.SuppressFinalize(this);
		}
		#endregion
	}
}