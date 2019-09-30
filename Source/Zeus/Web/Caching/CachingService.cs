using System;
using System.Web.Caching;
using Zeus.Persistence;

namespace Zeus.Web.Caching
{
	public class CachingService : ICachingService, IDisposable
	{
		private readonly IWebContext _webContext;
		private readonly IPersister _persister;

		public CachingService(IWebContext webContext, IPersister persister)
		{
			_webContext = webContext;
			_persister = persister;
			_persister.ItemSaving += OnPersisterItemSaving;
		}

		public bool IsPageCached(ContentItem contentItem)
		{
			return _webContext.HttpContext.Cache[GetCacheKey(contentItem)] != null;
		}

		public void InsertCachedPage(ContentItem contentItem, string html)
		{
			_webContext.HttpContext.Cache.Insert(GetCacheKey(contentItem),
				html, null, DateTime.Now.Add(contentItem.GetPageCachingDuration()),
				Cache.NoSlidingExpiration);
		}

		public string GetCachedPage(ContentItem contentItem)
		{
			var cachedPage = _webContext.HttpContext.Cache[GetCacheKey(contentItem)];
			if (cachedPage == null)
			{
				throw new InvalidOperationException("Page is not cached.");
			}

			return (string) cachedPage;
		}

		public void DeleteCachedPage(ContentItem contentItem)
		{
			_webContext.HttpContext.Cache.Remove(GetCacheKey(contentItem));
		}

		private string GetCacheKey(ContentItem contentItem)
		{
			//return "ZeusPageCache_" + contentItem.ID;
			//changed to make sure that the querystring is considered
			if (_webContext.HttpContext.Request.QueryString == null)
			{
				return "ZeusPageCache_" + contentItem.ID;
			}
			else
			{
				return "ZeusPageCache_" + contentItem.ID + "_" + _webContext.HttpContext.Request.QueryString;
			}
		}

		private void OnPersisterItemSaving(object sender, CancelItemEventArgs e)
		{
			if (IsPageCached(e.AffectedItem))
			{
				DeleteCachedPage(e.AffectedItem);
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
					_persister.ItemSaving -= OnPersisterItemSaving;
				}

				// TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
				// TODO: set large fields to null.

				disposedValue = true;
			}
		}

		// TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
		// ~CachingService()
		// {
		//   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
		//   Dispose(false);
		// }

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