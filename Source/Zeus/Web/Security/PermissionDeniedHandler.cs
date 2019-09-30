using System;
using Zeus.Security;

namespace Zeus.Web.Security
{
	public class PermissionDeniedHandler : IDisposable
	{
		private readonly ISecurityEnforcer _securityEnforcer;
		private readonly IAuthenticationContextService _authenticationContextService;

		public PermissionDeniedHandler(ISecurityEnforcer securityEnforcer, IAuthenticationContextService authenticationContextService)
		{
			_securityEnforcer = securityEnforcer;
			_authenticationContextService = authenticationContextService;

			_securityEnforcer.AuthorizationFailed += securityEnforcer_AuthorizationFailed;
		}

		private void securityEnforcer_AuthorizationFailed(object sender, CancelItemEventArgs e)
		{
			e.Cancel = true;
			_authenticationContextService.GetCurrentService().RedirectToLoginPage();
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
					_securityEnforcer.AuthorizationFailed -= securityEnforcer_AuthorizationFailed;
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