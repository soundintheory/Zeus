using System;
using System.Web;
using Zeus.BaseLibrary.Web;
using Zeus.Configuration;
using System.Web.Security;
using Zeus.Security;

namespace Zeus.Web.Security
{
	public class AuthenticationModule : IHttpModule
	{
		#region Fields

		private bool _onEnterCalled;

		private readonly IAuthenticationContextService _authService;

		private IAuthenticationService _currentService { get; set; }

		private readonly ICredentialService _credService;

		private AuthenticationSection _config;

		internal AuthenticationSection Config
		{
			get
			{
				return _config ?? (_config = System.Web.Configuration.WebConfigurationManager.GetSection("zeus/authentication") as AuthenticationSection);
			}
		}

		#endregion

		#region Constructor

		public AuthenticationModule(IAuthenticationContextService authenticationContext, ICredentialService credentialService)
		{
			_authService = authenticationContext;
			_credService = credentialService;
		}

		#endregion

		#region Events

		public event EventHandler<AuthenticationEventArgs> Authenticate;

		#endregion

		#region Properties

		private IAuthenticationService CurrentAuthenticationService
		{
			get
			{
				 if (_currentService == null)
				{
					_currentService = _authService.GetCurrentService();
				}
				return _currentService;
			}
		}

		#endregion

		#region Methods

		public void Init(HttpApplication app)
		{
			// Because it is possible to override authentication on a folder-by-folder
			// basis, we can't enable / disable authentication at this stage.
			// We need to do it inside OnEnter.
			app.AuthenticateRequest += OnAuthenticateRequest;
			app.EndRequest += OnEndRequest;
		}

		protected virtual void OnAuthenticateRequest(object source, EventArgs eventArgs)
		{
			var application = (HttpApplication) source;
			HttpContextBase context = new HttpContextWrapper(application.Context);

			if (!context.SkipAuthorization)
			{
				var locationPath = context.Request.Path.ToLower();
				if (Config != null && !_authService.ContainsLocation(locationPath))
				{
					var location = Config.ToAuthenticationLocation();
					location.Path = locationPath;
					_authService.AddLocation(location);
				}

				if (!CurrentAuthenticationService.Enabled)
				{
					return;
				}

				OnAuthenticate(new AuthenticationEventArgs(context));
				if (CurrentAuthenticationService.AccessingLoginPage())
				{
					context.SkipAuthorization = true;
				}

				//if (!context.SkipAuthorization)
				//	context.SkipAuthorization = AssemblyResourceLoader.IsValidWebResourceRequest(context);

				_onEnterCalled = true;
			}
		}

		protected virtual void OnAuthenticate(AuthenticationEventArgs e)
		{
			Authenticate?.Invoke(this, e);

			if (e.Context.User != null)
			{
				return;
			}

			if (e.User != null)
			{
				e.Context.User = e.User;
				return;
			}

			FormsAuthenticationTicket tOld;
			try
			{
				tOld = CurrentAuthenticationService.ExtractTicketFromCookie();
			}
			catch
			{
				tOld = null;
			}

			if (tOld?.Expired != false)
			{
				return;
			}

			var ticket = tOld;
			if (CurrentAuthenticationService.Config.SlidingExpiration)
			{
				ticket = CurrentAuthenticationService.RenewTicketIfOld(tOld);
			}

			User membershipUser = null;
			try
			{
				membershipUser = _credService.GetUserFast(ticket.Name);
			}
			catch
			{
			}
			if (membershipUser == null)
			{
				return;
			}

			e.Context.User = new WebPrincipal(membershipUser, ticket);

			if (ticket == tOld)
			{
				return;
			}

			HttpCookie cookie = null;
			if (!ticket.CookiePath.Equals("/"))
			{
				cookie = e.Context.Request.Cookies[CurrentAuthenticationService.Config.Name];
				if (cookie != null)
				{
					cookie.Path = ticket.CookiePath;
				}
			}

			CurrentAuthenticationService.CreateOrUpdateCookieFromTicket(ticket, cookie);
		}

		protected virtual void OnEndRequest(object source, EventArgs eventArgs)
		{
			if (!_onEnterCalled)
			{
				return;
			}

			_onEnterCalled = false;

			var application = (HttpApplication) source;
			var context = application.Context;
			if (context.Response.StatusCode != 401)
			{
				return;
			}

			// Add new ReturnUrl parameter, which will remove any existing parameter of this name.
			var redirectUrl = new Url(CurrentAuthenticationService.LoginUrl);
			redirectUrl.SetQueryParameter("ReturnUrl", new Url(context.Request.Url).PathAndQuery);
			context.Response.Redirect(redirectUrl.ToString(), false);
		}

		public void Dispose()
		{
		}

		#endregion
	}
}