using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using Zeus.Engine;
using Zeus.Persistence;
using Zeus.Security;
using Zeus.Web.Caching;

namespace Zeus.Web.Mvc
{
	/// <summary>
	/// Base class for content controllers that provides easy access to the content item in scope.
	/// </summary>
	/// <typeparam name="T">The type of content item the controller handles.</typeparam>
	[PageCacheFilter]
	public abstract class ContentController<T> : Controller
		where T : ContentItem
	{
		private T _currentItem;

		private readonly IPersister _persister;

		private readonly ISecurityManager _securityManager;

		protected ContentController(IPersister persister, ISecurityManager securityManager)
		{
			TempDataProvider = new SessionAndPerRequestTempDataProvider();
			_persister = persister;
			_securityManager = securityManager;
		}

		/// <summary>The content item associated with the requested path.</summary>
		public virtual T CurrentItem
		{
			get
			{
				return _currentItem ?? (_currentItem = ControllerContext.RouteData.Values[ContentRoute.ContentItemKey] as T
												 ?? GetCurrentItemById());
			}
			set { _currentItem = value; }
		}

		// TODO: actual tree traversal here
		protected ContentItem CurrentPage
		{
			get
			{
				ContentItem page = CurrentItem;
				while (page?.IsPage == false)
				{
					page = page.Parent;
				}

				return page;
			}
		}

		private T GetCurrentItemById()
		{
			if (int.TryParse(ControllerContext.RouteData.Values[ContentRoute.ContentItemIdKey] as string, out var itemId))
			{
				return _persister.Get<T>(itemId);
			}

			return null;
		}

		protected override void OnActionExecuting(ActionExecutingContext filterContext)
		{
			if (CurrentItem != null && !_securityManager.IsAuthorized(CurrentItem, User, Operations.Read))
			{
				filterContext.Result = new HttpUnauthorizedResult();
			}

			base.OnActionExecuting(filterContext);
		}

		/// <summary>Defaults to the current item's TemplateUrl and pass the item itself as view data.</summary>
		/// <returns>A reference to the item's template.</returns>
		public virtual ActionResult Index()
		{
			return View(CurrentItem);
		}

		protected virtual ActionResult RedirectToParentPage()
		{
			return RedirectToParentPage(string.Empty);
		}

		protected virtual ActionResult RedirectToParentPage(string fragment)
		{
			return Redirect(CurrentItem.Url + fragment);
		}

		///// <summary>
		///// Returns a <see cref="ViewPageResult"/> which calls the default action for the controller that handles the current page.
		///// </summary>
		///// <returns></returns>
		//protected virtual ViewPageResult ViewParentPage()
		//{
		//	if (CurrentItem?.IsPage == true)
		//	{
		//		throw new InvalidOperationException(
		//			"The current page is already being rendered. ViewPage should only be used from content items to render their parent page.");
		//	}

		//	return ViewPage(CurrentPage);
		//}

		///// <summary>
		///// Returns a <see cref="ViewPageResult"/> which calls the default action for the controller that handles the current page.
		///// </summary>
		///// <returns></returns>
		//protected internal virtual ViewPageResult ViewPage(ContentItem thePage)
		//{
		//	if (thePage == null)
		//	{
		//		throw new ArgumentNullException(nameof(thePage));
		//	}

		//	if (!thePage.IsPage)
		//	{
		//		throw new InvalidOperationException("Item " + thePage.GetType().Name +
		//		                                    " is not a page type and cannot be rendered on its own.");
		//	}

		//	if (thePage == CurrentItem)
		//	{
		//		throw new InvalidOperationException(
		//			"The page passed into ViewPage was the current page. This would cause an infinite loop.");
		//	}

		//	return new ViewPageResult(thePage, Engine.Resolve<IControllerMapper>(), Engine.Resolve<IWebContext>(),
		//		ActionInvoker);
		//}

		#region Nested type: SessionAndPerRequestTempDataProvider

		/// <summary>
		/// Overrides the default behaviour in the SessionStateTempDataProvider to make TempData available for the entire request, not just for the first controller it sees
		/// </summary>
		private sealed class SessionAndPerRequestTempDataProvider : ITempDataProvider
		{
			private const string TempDataSessionStateKey = "__ControllerTempData";

			#region ITempDataProvider Members

			public IDictionary<string, object> LoadTempData(ControllerContext controllerContext)
			{
				var httpContext = controllerContext.HttpContext;

				//let these through, non dangerous and stops bots throwing 100s of errors
				if (httpContext.Session?.IsReadOnly != false)
				{
					//throw new InvalidOperationException("Session state appears to be disabled.");
                    return new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
				}
                else
                {
					if (!((httpContext.Session[TempDataSessionStateKey] ?? httpContext.Items[TempDataSessionStateKey]) is Dictionary<string, object> tempDataDictionary))
					{
						return new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
					}

					// If we got it from Session, remove it so that no other request gets it
					httpContext.Session.Remove(TempDataSessionStateKey);
				    // Cache the data in the HttpContext Items to keep available for the rest of the request
				    httpContext.Items[TempDataSessionStateKey] = tempDataDictionary;

				    return tempDataDictionary;
                }
			}

			public void SaveTempData(ControllerContext controllerContext, IDictionary<string, object> values)
			{
				var httpContext = controllerContext.HttpContext;

				//let these through, non dangerous and stops bots throwing 100s of errors
				if (httpContext.Session?.IsReadOnly != false)
				{
					//    throw new InvalidOperationException("Session state appears to be disabled.  User Agent was " + httpContext.Request.UserAgent);
				}
				else
				{
					httpContext.Session[TempDataSessionStateKey] = values;
				}
			}

			#endregion
		}

		#endregion
	}
}
