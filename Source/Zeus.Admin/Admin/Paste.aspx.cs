﻿using System;
using Isis.Web.Hosting;
using Zeus.Security;

[assembly: EmbeddedResourceFile("Zeus.Admin.Paste.aspx", "Zeus.Admin")]
namespace Zeus.Admin
{
	[ActionPlugin("Paste", "Paste", Operations.Create, "CutCopyPaste", 3, null, "Zeus.Admin.Paste.aspx", "selected={selected}&memory={memory}&action={action}", Targets.Preview, "Zeus.Admin.Resources.page_paste.png", JavascriptEnableCondition = "window.top.zeus.getMemory()")]
	public partial class Paste : PreviewFrameAdminPage
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			string selected = Request.QueryString["selected"];
			string memory = Request.QueryString["memory"];
			string action = Request.QueryString["action"];
			if (string.IsNullOrEmpty(memory) || string.IsNullOrEmpty(action))
			{
				Title = "You must select what to paste and click on the appropriate action first.";
			}
			else
			{
				string url = string.Format("{0}?selected={1}&memory={2}", action, Server.UrlEncode(selected), Server.UrlEncode(memory));
				Response.Redirect(url);
			}
		}
	}
}