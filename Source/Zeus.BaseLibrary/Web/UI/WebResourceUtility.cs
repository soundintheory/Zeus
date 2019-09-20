using System;
using System.Web;
using System.Web.UI;

namespace Zeus.BaseLibrary.Web.UI
{
	public static class WebResourceUtility
	{
		public static string GetUrl(Type type, string resourceName)
		{
			var csm = HttpContext.Current?.Handler is Page ? ((Page) HttpContext.Current.Handler).ClientScript : new Page().ClientScript;
			return csm.GetWebResourceUrl(type, resourceName);
		}
	}
}