using Ext.Net;
using System.Xml.Linq;
using System;

namespace Zeus.Admin.Plugins.MoveItem
{
	public class MoveItemTreePlugin : TreePluginBase
	{
		public override string[] RequiredUserControls
		{
			get { return new[] { GetPageUrl(GetType(), "Zeus.Admin.Plugins.MoveItem.MoveUserControl.ascx") }; }
		}

		public override void ModifyTree(TreePanel treePanel, IMainInterface mainInterface)
		{

			if (!Context.SecurityManager.IsAdmin(Context.Current.WebContext.User))
			{
				//useful arguments for the handler :) 
				//console.info('tree', tree)
				//console.info('node', node)
				//console.info('oldParent', oldParent)
				//console.info('newParent', newParent)
				//console.info('index', index)
				treePanel.Listeners.BeforeMoveNode.Handler = @"		
					if(oldParent.id !== newParent.id){{
						var confirmed = confirm('Are you sure you want to move this node?');
						return confirmed;
					}}

					return true;
				";
			}
			
            treePanel.Listeners.MoveNode.Handler = string.Format(@"
				{0}.showBusy();					
				Ext.net.DirectMethods.Move.MoveNode(node.id, newParent.id, index,
				{{
					url: '{1}',
					success: function() {{ {0}.setStatus({{ text: 'Moved item', iconCls: '', clear: true }}); }}
				}});
				",
					mainInterface.StatusBar.ClientID, Context.AdminManager.GetAdminDefaultUrl());
		}
	}
}