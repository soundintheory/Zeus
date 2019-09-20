using System;

namespace Zeus.Admin.Plugins.PasteItem
{
	public partial class Default : PreviewFrameAdminPage
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			var selected = Request.QueryString["selected"];
			var memory = Request.QueryString["memory"];
			var action = Request.QueryString["action"];
			if (string.IsNullOrEmpty(memory) || string.IsNullOrEmpty(action))
			{
				Title = "You must select what to paste and click on the appropriate action first.";
			}
			else
			{
				var url = string.Format("{0}?selected={1}&memory={2}", action, Server.UrlEncode(selected), Server.UrlEncode(memory));
				Response.Redirect(url);
			}
		}
	}
}