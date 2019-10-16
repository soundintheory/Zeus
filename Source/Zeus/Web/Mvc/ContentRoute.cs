using System;
using System.Configuration;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Zeus.BaseLibrary.Web;
using Zeus.Configuration;
using Zeus.Engine;
using System.Linq;
using System.IO;
using Zeus.Persistence;

namespace Zeus.Web.Mvc
{
    /// <summary>
    /// An ASP.NET MVC route that gets route data for content item paths.
    /// </summary>
    public class ContentRoute : Route
    {
        public const string ActionKey = "action";
        public const string AreaKey = "area";
        public const string ContentEngineKey = "engine";
        public const string ContentItemIdKey = "id";
        public const string ContentItemKey = "item";
        public const string ContentUrlKey = "url";
        public const string ControllerKey = "controller";

		private readonly IRouteHandler _routeHandler = new MvcRouteHandler();
		private readonly AdminSection _adminSection;
		private readonly IControllerMapper _controllerMapper;
		private readonly IUrlParser _parser;
		private readonly IPersister _persister;

		public ContentRoute(RoutingSection section, IControllerMapper controllerMapper, AdminSection adminSection, IUrlParser parser, IPersister persister, IRouteHandler routeHandler = null)
			: base("{controller}/{action}/{*remainingUrl}", new RouteValueDictionary(new { Action = "Index" }), routeHandler)
        {

			if (routeHandler != null)
			{
				_routeHandler = routeHandler;
			}

			_controllerMapper = controllerMapper;
			_parser = parser;
			_persister = persister;

            DataTokens = new RouteValueDictionary();

            var namespaces = section.Controllers.ToArray();

            if (namespaces?.Length > 0)
            {
                DataTokens["Namespaces"] = namespaces;
                DataTokens["UseNamespaceFallback "] = false;
            }

			_adminSection = adminSection;
        }

        public override RouteData GetRouteData(HttpContextBase httpContext)
        {
            var path = httpContext.Request.AppRelativeCurrentExecutionFilePath;
            if (path.StartsWith("~/" + _adminSection + "/", StringComparison.InvariantCultureIgnoreCase))
			{
				return null;
			}

			if (path.EndsWith(".axd", StringComparison.InvariantCultureIgnoreCase))
			{
				return null;
			}

			return GetRouteDataForPath(httpContext.Request);
        }

        public RouteData GetRouteDataForPath(HttpRequestBase request)
        {
            var td = _parser.ResolvePath(request.Url.ToString());

            var extraParam = "";

            if (td.CurrentItem != null && !td.Is404)
            {
                //all ok, item found, we can continue
            }
            else
            {
                var originalLookUpWas404 = td.Is404;
                PathData originalPathData = null;
                if (originalLookUpWas404)
				{
					originalPathData = td;
				}

				//test for extra param being passed in...so /item/myParam - needs to ignore querystring of course
				var fullPath = request.Url.ToString();
                if (fullPath.IndexOf('?') > -1)
				{
					fullPath = fullPath.Substring(0, fullPath.IndexOf('?'));
				}

				var thePath = new Uri(fullPath);
                var thePathWithOutLastParam = new Uri(thePath.AbsoluteUri.Remove(thePath.AbsoluteUri.Length - (thePath.Segments.Last().Length + 1))).PathAndQuery;
                td = _parser.ResolvePath(thePathWithOutLastParam);

                //check to see if the content item has been and is a page and if so, if it allows the Index(Param) option
                if (!td.Is404 && td.CurrentItem != null && td.CurrentItem is PageContentItem && (td.CurrentItem as PageContentItem)?.AllowParamsOnIndex == true)
                {
                    extraParam = thePath.Segments.Last();
                }
                else
                {
                    //check for 404
                    if (td.Is404 || originalLookUpWas404)
                    {
                        if (!td.Is404 && originalLookUpWas404)
                        {
                            //need to return to our original 404 page here...
                            td = originalPathData;
                        }

                        //return correct response!  Code will continue from here and all will be fine...
                        System.Web.HttpContext.Current.Response.StatusCode = 404;
                    }
                    else
					{
						return null;
					}
				}
            }

            var item = td.CurrentItem;
            var action = td.Action;

            if (item == null)
			{
				return null;
			}

			if (td.QueryParameters.ContainsKey("preview")
				&& int.TryParse(td.QueryParameters["preview"], out var itemId))
			{
				item = _persister.Get(itemId);
			}
			var controllerName = _controllerMapper.GetControllerName(item.GetType());

            if (controllerName == null)
			{
				return null;
			}

			var areaName = _controllerMapper.GetAreaName(item.GetType());

            var data = new RouteData(this, _routeHandler);
            data.Values[ContentItemKey] = item;
            data.Values[ControllerKey] = controllerName;
            data.Values[ActionKey] = action;
            data.Values[AreaKey] = areaName;
            if (!string.IsNullOrEmpty(extraParam))
			{
				data.Values["param"] = extraParam;
			}
			// trigger ASP.net areas
			data.DataTokens["area"] = areaName;

            if (DataTokens.ContainsKey("Namespaces"))
			{
				data.DataTokens["Namespaces"] = DataTokens["Namespaces"];
			}

			return data;
        }

        public override VirtualPathData GetVirtualPath(RequestContext requestContext, RouteValueDictionary values)
        {
			var item = values.ContainsKey(ContentItemKey)
				? values[ContentItemKey] as ContentItem
				: requestContext.RouteData.Values[ContentItemKey] as ContentItem;
			if (item == null)
			{
				return null;
			}

			var requestedController = values[ControllerKey] as string;
            var itemController = _controllerMapper.GetControllerName(item.GetType());
            if (!string.Equals(requestedController, itemController, StringComparison.InvariantCultureIgnoreCase))
			{
				return null;
			}

			var pathData = base.GetVirtualPath(requestContext, values);
            Url itemUrl = item.Url;
            Url pathUrl = pathData.VirtualPath;

            if (item.IsPage)
            {
                pathUrl = pathUrl.RemoveSegment(0).PrependSegment(itemUrl.PathWithoutExtension.TrimStart('/'))
                    .RemoveQuery(ContentItemKey).RemoveQuery(AreaKey) // base.GetVirtualPath will add these
                    .PathAndQuery.TrimStart('/');
            }
            else
            {
                //pathUrl = pathUrl.PrependSegment(itemUrl.PathWithoutExtension.TrimStart('/'))
                //    .PathAndQuery.TrimStart('/');

                pathUrl = pathUrl.AppendSegment(item.ID.ToString());
            }
            pathData.VirtualPath = pathUrl;

            return pathData;
        }

        
    }
}