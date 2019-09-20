using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.Routing;
using Zeus.Web.UI;

namespace Zeus.Web.Mvc.Html
{
	public class TemplateRenderer : ITemplateRenderer
	{
		private readonly IControllerMapper _controllerMapper;

		public TemplateRenderer(IControllerMapper controllerMapper)
		{
			_controllerMapper = controllerMapper;
		}

		public string RenderTemplate(HtmlHelper htmlHelper, ContentItem item, IContentItemContainer container, string action)
		{
			var routeValues = new RouteValueDictionary
			{
				{ ContentRoute.ContentItemKey, item },
				{ ContentRoute.AreaKey, _controllerMapper.GetAreaName(item.GetType()) }
			};

			return htmlHelper.Action(action,
				_controllerMapper.GetControllerName(item.GetType()),
				routeValues).ToString();
		}
	}
}