using System;
using System.Linq;
using System.Threading;
using System.Web;
using Ext.Net;
using Zeus.ContentTypes;
using Zeus.Linq;
using Zeus.Security;
using Zeus.Web;

namespace Zeus.Admin.Plugins.Tree
{
	public class TreeLoader : IHttpHandler
	{
		public void ProcessRequest(HttpContext context)
		{
			context.Response.ContentType = "text/json";

			bool fromRoot = context.Request["fromRoot"]?.ToLower() == "true";
			int.TryParse(context.Request["node"], out var nodeId);
            nodeId = int.TryParse(context.Request["overrideNode"], out var overrideNodeId) && overrideNodeId > 0 ? overrideNodeId : nodeId;
            var filter = context.Request["filter"]?.ToLower();
            SiteTree tree;

            if (nodeId == 0)
            {
                nodeId = Context.Current.Host.CurrentSite.RootItemID;
            }

            var selectedItem = Context.Persister.Get(nodeId);

            if (!string.IsNullOrWhiteSpace(filter))
            {
                tree = SiteTree.Filtered(filter);
            }
            else
            {
                if (fromRoot)
                {
                    tree = SiteTree.Between(selectedItem, Find.AdminRootItem, true);
                }
                else
                {
                    tree = SiteTree.From(selectedItem.TranslationOf ?? selectedItem, 2);
                }
            }

            if (selectedItem != null && selectedItem.ID != Context.Current.Host.CurrentSite.RootItemID)
            {
                tree.OpenTo(selectedItem);
            }

            var treeNodeBase = tree.Filter(items => items
                    .Authorized(context.User, Context.SecurityManager, Operations.Read)
                    .Where(ci => ci.IsVisibleInTree)
                    .Where(ci => !(ci is WidgetContentItem)))
                .ToTreeNode(false);

            if (treeNodeBase is TreeNode treeNode)
            {
                string json = treeNode.Nodes.ToJson();
                context.Response.Write(json);
                context.Response.End();
            }
        }

		public bool IsReusable
		{
			get { return false; }
		}
	}
}