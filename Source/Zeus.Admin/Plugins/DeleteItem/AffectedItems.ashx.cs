using System;
using System.Linq;
using System.Web;
using Ext.Net;
using Zeus.Admin.Plugins.Tree;
using Zeus.Linq;
using Zeus.Security;

namespace Zeus.Admin.Plugins.DeleteItem
{
	public class AffectedItems : IHttpHandler
	{
		public void ProcessRequest(HttpContext context)
		{
			context.Response.ContentType = "text/json";

			// "node" will be a comma-separated list of nodes that are going to be deleted.
			var node = context.Request["node"];

			if (!string.IsNullOrEmpty(node))
			{
				var nodeIDsTemp = node.Split(',');
				var nodeIDs = nodeIDsTemp.Select(s => Convert.ToInt32(s));

				var treeNodes = new TreeNodeCollection();

				foreach (var nodeID in nodeIDs)
				{
					var selectedItem = Context.Persister.Get(nodeID);

					var tree = SiteTree.From(selectedItem, int.MaxValue);

					var treeNode = tree.Filter(items => items.Authorized(context.User, Context.SecurityManager, Operations.Read))
						.ToTreeNode(false);

					treeNodes.Add(treeNode);
				}

				var json = treeNodes.ToJson();
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