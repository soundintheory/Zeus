using System;
using System.Collections.Specialized;
using System.Web;
using Zeus.BaseLibrary.Web;
using Zeus.Configuration;
using Zeus.Engine;

namespace Zeus.Web
{
	/// <summary>
	/// Resolves the controller to handle a certain request. Supports a default 
	/// controller or additional imprativly using the ConnectControllers method 
	/// or declarativly using the [Controls] attribute registered.
	/// </summary>
	public class RequestDispatcher : IRequestDispatcher
	{
		private readonly IContentAdapterProvider aspectProvider;
		private readonly IWebContext webContext;
		private readonly IUrlParser parser;
		private readonly IErrorHandler errorHandler;
		private readonly bool rewriteEmptyExtension = true;
		private readonly bool observeAllExtensions = true;
		private readonly string[] observedExtensions = new[] { ".aspx" };
		private readonly string[] nonRewritablePaths = new[] { "~/admin/" };

		public RequestDispatcher(IContentAdapterProvider aspectProvider, IWebContext webContext, IUrlParser parser, IErrorHandler errorHandler, HostSection config)
		{
			this.aspectProvider = aspectProvider;
			this.webContext = webContext;
			this.parser = parser;
			this.errorHandler = errorHandler;
			//observeAllExtensions = config.Web.ObserveAllExtensions;
			rewriteEmptyExtension = config.Web.ObserveEmptyExtension;
			var additionalExtensions = config.Web.ObservedExtensions;
			if (additionalExtensions?.Count > 0)
			{
				observedExtensions = new string[additionalExtensions.Count + 1];
				additionalExtensions.CopyTo(observedExtensions, 1);
			}
			observedExtensions[0] = config.Web.Extension;
			//nonRewritablePaths = config.Web.Urls.NonRewritable.GetPaths(webContext);
		}

		/// <summary>Resolves the controller for the current Url.</summary>
		/// <returns>A suitable controller for the given Url.</returns>
		public virtual T ResolveAdapter<T>() where T : class, IContentAdapter
		{
			var controller = RequestItem<T>.Instance;
			if (controller != null)
			{
				return controller;
			}

			var url = webContext.Url;
			var path = url.Path;
			foreach (var nonRewritablePath in nonRewritablePaths)
			{
				if (path.StartsWith(VirtualPathUtility.ToAbsolute(nonRewritablePath)))
				{
					return null;
				}
			}

			var data = ResolveUrl(url);
			controller = aspectProvider.ResolveAdapter<T>(data);

			RequestItem<T>.Instance = controller;
			return controller;
		}

		public PathData ResolveUrl(string url)
		{
			try
			{
				if (IsObservable(url))
				{
					return parser.ResolvePath(url);
				}
			}
			catch (Exception ex)
			{
				errorHandler.Notify(ex);
			}
			return PathData.Empty;
		}

		private bool IsObservable(Url url)
		{
			if (observeAllExtensions)
			{
				return true;
			}

			if (url.LocalUrl == Url.ApplicationPath)
			{
				return true;
			}

			var extension = url.Extension;
			if (rewriteEmptyExtension && string.IsNullOrEmpty(extension))
			{
				return true;
			}

			foreach (var observed in observedExtensions)
			{
				if (string.Equals(observed, extension, StringComparison.InvariantCultureIgnoreCase))
				{
					return true;
				}
			}

			if (url.GetQuery(PathData.PageQueryKey) != null)
			{
				return true;
			}

			return false;
		}
	}
}