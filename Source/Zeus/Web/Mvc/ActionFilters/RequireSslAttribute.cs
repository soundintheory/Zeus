using System.Web;
using System.Web.Mvc;

namespace Zeus.Web.Mvc.ActionFilters
{
	public class RequireSslAttribute : ActionFilterAttribute
	{
		public override void OnActionExecuting(ActionExecutingContext filterContext)
		{
			var request = filterContext.HttpContext.Request;
			var response = filterContext.HttpContext.Response;

			// Check if we're secure or not and if we're on the local box
			if (!request.IsSecureConnection && !request.IsLocal)
			{
				var url = request.Url.ToString().ToLower().Replace("http:", "https:");
				response.Redirect(url);
			}
			base.OnActionExecuting(filterContext);
		}
	}
}