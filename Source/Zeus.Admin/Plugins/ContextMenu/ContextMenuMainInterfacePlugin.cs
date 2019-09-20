using System.Web.UI;
using Ext.Net;
using Zeus.BaseLibrary.Web.UI;

[assembly: WebResource("Zeus.Admin.Plugins.ContextMenu.Resources.Ext.ux.zeus.ContextMenuPlugin.js", "text/javascript")]

namespace Zeus.Admin.Plugins.ContextMenu
{
	public class ContextMenuMainInterfacePlugin : MainInterfacePluginBase
	{
		public override void ModifyInterface(IMainInterface mainInterface)
		{
			foreach (var plugin in Context.Current.ResolveAll<IContextMenuPlugin>())
			{
				var requiredUserControls = plugin.RequiredUserControls;
				if (requiredUserControls != null)
					mainInterface.LoadUserControls(requiredUserControls);
			}
		}

		public override void RegisterScripts(ResourceManager scriptManager)
		{
			// Render action plugin scripts.
			scriptManager.RegisterClientScriptInclude("ActionPlugin",
				WebResourceUtility.GetUrl(typeof(ContextMenuMainInterfacePlugin), "Zeus.Admin.Plugins.ContextMenu.Resources.Ext.ux.zeus.ContextMenuPlugin.js"));

			foreach (var plugin in Context.Current.ResolveAll<IContextMenuPlugin>())
			{
				var requiredScripts = plugin.RequiredScripts;
				if (requiredScripts != null)
					foreach (var requiredScript in requiredScripts)
						scriptManager.RegisterClientScriptInclude(plugin.GetType().FullName, requiredScript);
			}
		}
	}
}