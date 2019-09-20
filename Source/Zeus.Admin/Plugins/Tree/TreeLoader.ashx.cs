using System;
using System.Linq;
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
			var fromRootTemp = context.Request["fromRoot"];
			var fromRoot = false;
			if (!string.IsNullOrEmpty(fromRootTemp))
				fromRoot = Convert.ToBoolean(fromRootTemp);
			var sync = (context.Request["sync"] == "true");
			var nodeId = !string.IsNullOrEmpty(context.Request["node"]) ? Convert.ToInt32(context.Request["node"]) as int? : null;
			var overrideNodeId = !string.IsNullOrEmpty(context.Request["overrideNode"]) ? Convert.ToInt32(context.Request["overrideNode"]) as int? : null;

			nodeId = overrideNodeId ?? nodeId;
			if (nodeId != null)
			{
				var selectedItem = Context.Persister.Get(nodeId.Value);

				SiteTree tree;
				if (sync)
					tree = SiteTree.From(selectedItem, int.MaxValue);
				else if (fromRoot)
					tree = SiteTree.Between(selectedItem, Find.RootItem, true)
						.OpenTo(selectedItem);
				else
					tree = SiteTree.From(selectedItem.TranslationOf ?? selectedItem, 2);

				if (sync)
					tree = tree.ForceSync();

				//if (context.User.Identity.Name != "administrator")
				//	filter = new CompositeSpecification<ContentItem>(new PageSpecification<ContentItem>(), filter);
				var treeNode = tree.Filter(items => items
						.Authorized(context.User, Context.SecurityManager, Operations.Read)
						.Where(TreeMainInterfacePlugin.IsVisibleInTree)
						.Where(ci => !(ci is WidgetContentItem)))
					.ToTreeNode(false);

				if (treeNode is TreeNode)
				{
					var json = ((TreeNode) treeNode).Nodes.ToJson();
					context.Response.Write(json);
					context.Response.End();
				}
			}
		}

		public bool IsReusable
		{
			get { return false; }
		}
	}
}