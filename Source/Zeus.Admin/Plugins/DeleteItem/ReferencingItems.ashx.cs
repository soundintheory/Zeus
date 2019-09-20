using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Ext.Net;
using Zeus.BaseLibrary.ExtensionMethods.Linq;
using Zeus.ContentProperties;

namespace Zeus.Admin.Plugins.DeleteItem
{
	public class ReferencingItems : IHttpHandler
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

					var referrers = new List<ContentItem>();
					AddReferencesRecursive(selectedItem, referrers);

					foreach (var contentItem in referrers.Distinct(ci => ci.ID))
					{
						var treeNode = new TreeNode();
						treeNode.Text = ((INode) contentItem).Contents;
						treeNode.IconFile = contentItem.IconUrl;
						treeNode.NodeID = contentItem.ID.ToString();
						treeNode.Leaf = true;

						treeNodes.Add(treeNode);
					}
				}

				var json = treeNodes.ToJson();
				context.Response.Write(json);
				context.Response.End();
			}
		}

		protected void AddReferencesRecursive(ContentItem current, List<ContentItem> referrers)
		{
			//referrers.AddRange(Context.Finder.QueryItems().Where(ci => ci.Details.OfType<LinkProperty>().Any(ld => ld.LinkedItem == current)));
			foreach (var child in current.GetChildren())
				AddReferencesRecursive(child, referrers);
		}

		public bool IsReusable
		{
			get { return false; }
		}
	}
}