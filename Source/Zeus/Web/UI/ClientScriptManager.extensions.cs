using Microsoft.SqlServer.Management.Smo;
using System.Web.UI;
using Zeus.BaseLibrary.ExtensionMethods.Web.UI;
using Zeus.Web.UI.WebControls;

namespace Zeus.Web.UI
{
	public static class ClientScriptManagerExtensionMethods
	{
		public static void RegisterJQuery(this ClientScriptManager clientScriptManager)
		{
			clientScriptManager.RegisterJavascriptResource(typeof(HtmlTextBox), "Zeus.Web.Resources.jQuery.jquery.js", ResourceInsertPosition.HeaderTop);
			clientScriptManager.RegisterClientScriptBlock(typeof(HtmlTextBox), "JQueryNoConflict", "jQuery.noConflict();", true);
		}

		public static void RegisterSelect2(this ClientScriptManager clientScriptManager)
		{
            clientScriptManager.RegisterJavascriptResource(typeof(HtmlTextBox), "Zeus.Web.Resources.Select2.js.select2.min.js", ResourceInsertPosition.HeaderTop);
            clientScriptManager.RegisterCssResource(typeof(HtmlTextBox), "Zeus.Web.Resources.Select2.css.select2.min.css", ResourceInsertPosition.HeaderTop);
        }
	}
}