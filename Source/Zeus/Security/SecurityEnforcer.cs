﻿using System;
using Zeus.Persistence;

namespace Zeus.Security
{
	/// <summary>
	/// Checks against unauthorized requests, and updates of content items.
	/// </summary>
	public class SecurityEnforcer : ISecurityEnforcer, IDisposable
	{
		#region Fields

		private readonly ISecurityManager _security;
		private readonly Web.IWebContext _webContext;
		private readonly IPersister _persister;

		#endregion

		#region Constructor

		public SecurityEnforcer(ISecurityManager security, Web.IWebContext webContext, IPersister persister)
		{
			_webContext = webContext;
			_security = security;
			_persister = persister;

			_persister.ItemSaving += ItemSavingEventHandler;
			_persister.ItemCopying += ItemCopyingEvenHandler;
			_persister.ItemDeleting += ItemDeletingEvenHandler;
			_persister.ItemMoving += ItemMovingEvenHandler;
		}

		#endregion

		#region Events

		/// <summary>
		/// Is invoked when a security violation is encountered. The security 
		/// exception can be cancelled by setting the cancel property on the event 
		/// arguments.
		/// </summary>
		public event EventHandler<CancelItemEventArgs> AuthorizationFailed;

		#endregion

		#region Methods

		#region Event Handlers

		private void ItemSavingEventHandler(object sender, CancelItemEventArgs e)
		{
			OnItemSaving(e.AffectedItem);
		}

		private void ItemMovingEvenHandler(object sender, CancelDestinationEventArgs e)
		{
			OnItemMoving(e.AffectedItem, e.Destination);
		}

		private void ItemDeletingEvenHandler(object sender, CancelItemEventArgs e)
		{
			OnItemDeleting(e.AffectedItem);
		}

		private void ItemCopyingEvenHandler(object sender, CancelDestinationEventArgs e)
		{
			OnItemCopying(e.AffectedItem, e.Destination);
		}

		#endregion

		/// <summary>Checks that the current user is authorized to access the current item.</summary>
		public virtual void AuthoriseRequest()
		{
			var item = _webContext.CurrentPage;

			if (item == null)
			{
				return;
			}

			if (_security.IsAuthorized(item, _webContext.User, Operations.Read))
			{
				return;
			}

			var args = new CancelItemEventArgs(item);
			AuthorizationFailed?.Invoke(this, args);

			if (!args.Cancel)
			{
				//throw new PermissionDeniedException(item, webContext.User);
				throw new UnauthorizedAccessException();
			}
		}

		/// <summary>Is invoked when an item is saved.</summary>
		/// <param name="item">The item that is to be saved.</param>
		protected virtual void OnItemSaving(ContentItem item)
		{
			if (!_security.IsAuthorized(item, _webContext.User, Operations.Change))
			{
				throw new PermissionDeniedException(item, _webContext.User, Operations.Change);
			}

			var user = _webContext.User;
			item.SavedBy = user != null ? user.Identity.Name : null;
		}

		/// <summary>Is Invoked when an item is moved.</summary>
		/// <param name="source">The item that is to be moved.</param>
		/// <param name="destination">The destination for the item.</param>
		protected virtual void OnItemMoving(ContentItem source, ContentItem destination)
		{
			if (!_security.IsAuthorized(source, _webContext.User, Operations.Read) || !_security.IsAuthorized(destination, _webContext.User, Operations.Create))
			{
				throw new PermissionDeniedException(source, _webContext.User, Operations.Create);
			}
		}

		/// <summary>Is invoked when an item is to be deleted.</summary>
		/// <param name="item">The item to delete.</param>
		protected virtual void OnItemDeleting(ContentItem item)
		{
			var user = _webContext.User;
			if (!_security.IsAuthorized(item, user, Operations.Delete))
			{
				throw new PermissionDeniedException(item, user, Operations.Delete);
			}
		}

		/// <summary>Is invoked when an item is to be copied.</summary>
		/// <param name="source">The item that is to be copied.</param>
		/// <param name="destination">The destination for the copied item.</param>
		protected virtual void OnItemCopying(ContentItem source, ContentItem destination)
		{
			if (!_security.IsAuthorized(source, _webContext.User, Operations.Read) || !_security.IsAuthorized(destination, _webContext.User, Operations.Create))
			{
				throw new PermissionDeniedException(source, _webContext.User, Operations.Create);
			}
		}
		#endregion

		#region IDisposable Support
		private bool disposedValue = false; // To detect redundant calls

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					// TODO: dispose managed state (managed objects).
					_persister.ItemSaving -= ItemSavingEventHandler;
					_persister.ItemCopying -= ItemCopyingEvenHandler;
					_persister.ItemDeleting -= ItemDeletingEvenHandler;
					_persister.ItemMoving -= ItemMovingEvenHandler;
				}

				// TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
				// TODO: set large fields to null.

				disposedValue = true;
			}
		}

		// TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
		// ~SecurityEnforcer()
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
