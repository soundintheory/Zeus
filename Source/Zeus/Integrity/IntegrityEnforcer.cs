using System;
using Zeus.Persistence;

namespace Zeus.Integrity
{
	public class IntegrityEnforcer : IIntegrityEnforcer, IDisposable
	{
		private readonly IPersister _persister;
		private readonly IIntegrityManager _integrityManager;

		public IntegrityEnforcer(IPersister persister, IIntegrityManager integrityManager)
		{
			_persister = persister;
			_integrityManager = integrityManager;

			_persister.ItemCopying += ItemCopyingEventHandler;
			_persister.ItemDeleting += ItemDeletingEventHandler;
			_persister.ItemMoving += ItemMovingEventHandler;
			_persister.ItemSaving += ItemSavingEventHandler;
		}

		#region Event Dispatchers

		private void ItemSavingEventHandler(object sender, CancelItemEventArgs e)
		{
			OnItemSaving(e.AffectedItem);
		}

		private void ItemMovingEventHandler(object sender, CancelDestinationEventArgs e)
		{
			OnItemMoving(e.AffectedItem, e.Destination);
		}

		private void ItemDeletingEventHandler(object sender, CancelItemEventArgs e)
		{
			OnItemDeleting(e.AffectedItem);
		}

		private void ItemCopyingEventHandler(object sender, CancelDestinationEventArgs e)
		{
			OnItemCopying(e.AffectedItem, e.Destination);
		}

		#endregion

		#region Event Handlers

		protected virtual void OnItemCopying(ContentItem source, ContentItem destination)
		{
			var ex = _integrityManager.GetCopyException(source, destination);
			if (ex != null)
			{
				throw ex;
			}
		}

		protected virtual void OnItemDeleting(ContentItem item)
		{
			var ex = _integrityManager.GetDeleteException(item);
			if (ex != null)
			{
				throw ex;
			}
		}

		protected virtual void OnItemMoving(ContentItem source, ContentItem destination)
		{
			var ex = _integrityManager.GetMoveException(source, destination);
			if (ex != null)
			{
				throw ex;
			}
		}

		protected virtual void OnItemSaving(ContentItem item)
		{
			var ex = _integrityManager.GetSaveException(item);
			if (ex != null)
			{
				throw ex;
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
					_persister.ItemCopying -= ItemCopyingEventHandler;
					_persister.ItemDeleting -= ItemDeletingEventHandler;
					_persister.ItemMoving -= ItemMovingEventHandler;
					_persister.ItemSaving -= ItemSavingEventHandler;
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
