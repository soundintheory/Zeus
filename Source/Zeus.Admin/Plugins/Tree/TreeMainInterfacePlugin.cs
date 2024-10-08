using Ext.Net;
using Zeus.ContentTypes;
using Zeus.Linq;
using Zeus.Security;
using Zeus.Web.Hosting;
using Zeus.BaseLibrary.Web.UI;

[assembly: System.Web.UI.WebResource("Zeus.Admin.Plugins.Tree.Resources.TreeCssInitializer.js", "text/javascript")]
namespace Zeus.Admin.Plugins.Tree
{
	public class TreeMainInterfacePlugin : MainInterfacePluginBase
	{
		public override void ModifyInterface(IMainInterface mainInterface)
		{
			// Add tree.
			TreePanel treePanel = new TreePanel
			{
				ID = "stpNavigation",
				Width = 200,
				Icon = Icon.SitemapColor,
				Title = "Site",
				AutoScroll = true,
				PathSeparator = "|",
				EnableDD = true,
				UseArrows = true,
				BodyStyle = "padding-top:5px",
				Region = Region.West,
				MinWidth = 175,
				MaxWidth = 400,
				Split = true,
				CollapseFirst = false
			};
			mainInterface.Viewport.Items.Add(treePanel);

			// Setup tree top toolbar.
			Toolbar topToolbar = new Toolbar();
			treePanel.TopBar.Add(topToolbar);

			TriggerField filterField = new TriggerField
			{
				EnableKeyEvents = true,
				Width = 100,
				EmptyText = "Filter..."
			};
			filterField.Triggers.Add(new FieldTrigger
			{
				Icon = TriggerIcon.Clear,
				HideTrigger = true
			});
			filterField.Listeners.KeyUp.Fn = "keyUp";
			filterField.Listeners.KeyUp.Buffer = 250;
			filterField.Listeners.TriggerClick.Fn = "clearFilter";
			topToolbar.Items.Add(filterField);
			topToolbar.Items.Add(new ToolbarFill());

			Button refreshButton = new Button { Icon = Icon.Reload };
			refreshButton.ToolTips.Add(new ToolTip { Html = "Refresh" });
			refreshButton.Listeners.Click.Handler = string.Format("{0}.getRootNode().reload();", treePanel.ClientID);
			topToolbar.Items.Add(refreshButton);

			Button expandAllButton = new Button { IconCls = "icon-expand-all" };
			expandAllButton.ToolTips.Add(new ToolTip { Html = "Expand All" });
			expandAllButton.Listeners.Click.Handler = string.Format("{0}.expandAll();", treePanel.ClientID);
			topToolbar.Items.Add(expandAllButton);

			Button collapseAllButton = new Button { IconCls = "icon-collapse-all" };
			collapseAllButton.ToolTips.Add(new ToolTip { Html = "Collapse All" });
			collapseAllButton.Listeners.Click.Handler = string.Format("{0}.collapseAll();", treePanel.ClientID);
			topToolbar.Items.Add(collapseAllButton);

			topToolbar.Items.Add(new ToolbarSeparator());

			Window helpWindow = new Window
				{
					Modal = true,
					Icon = Icon.Help,
					Title = "Help",
					Hidden = true,
					Html = "This is the site tree. You can use this to view all the pages on your site. Right-click any item to edit or delete it, as well as create additional pages.<br /><br />The main branches of the root node tree are the main sections of the admin system. Each section is broken down into smaller sections, accessed by expanding the + sign. (Wherever you see a + sign, the section can be broken down into further sections).<br /><br />When you receive your admin system, you will notice the first main section (after the Root Node) will be divided into the main sections of your website. (Example sections may include: Homepage, About Us, News, Links and Contact pages.) To see these pages, as they appear on the website, simply click the relevant node � it will then appear in the right hand pane.",
					BodyStyle = "padding:5px",
					Width = 300,
					Height = 200,
					AutoScroll = true
				};
			mainInterface.AddControl(helpWindow);

			Button helpButton = new Button();
			helpButton.Icon = Icon.Help;
			helpButton.ToolTips.Add(new ToolTip { Html = "Help" });
			helpButton.Listeners.Click.Handler = string.Format("{0}.show();", helpWindow.ClientID);
			topToolbar.Items.Add(helpButton);

			// Data loader.
			var treeLoader = new Ext.Net.TreeLoader
				{
					DataUrl = Context.Current.Resolve<IEmbeddedResourceManager>().GetServerResourceUrl(GetType().Assembly,
						"Zeus.Admin.Plugins.Tree.TreeLoader.ashx"),
					Timeout = 300000,
					PreloadChildren = true
				};
			treeLoader.Listeners.Load.Handler = "if (response.getResponseHeader['Content-Type'] == 'text/html; charset=utf-8') { Ext.Msg.alert('Timeout', 'Your session has timed out. Please refresh your browser to login again.'); }";
			treePanel.Loader.Add(treeLoader);

			// Call tree modification plugins and load tree plugin user controls.
			foreach (ITreePlugin treePlugin in Context.Current.ResolveAll<ITreePlugin>())
			{
				string[] requiredUserControls = treePlugin.RequiredUserControls;
				if (requiredUserControls != null)
					mainInterface.LoadUserControls(requiredUserControls);

				treePlugin.ModifyTree(treePanel, mainInterface);
			}

			if (!ExtNet.IsAjaxRequest)
			{
				TreeNodeBase treeNode = SiteTree.Between(Find.StartPage, Find.RootItem, true)
					.OpenTo(Find.StartPage)
					.Filter(items => items.Authorized(Context.Current.WebContext.User, Context.SecurityManager, Operations.Read))
					.ToTreeNode(true);
				treePanel.Root.Add(treeNode);
			}
		}

		public override void RegisterScripts(ResourceManager scriptManager)
		{
			scriptManager.RegisterClientScriptInclude("TreeCssInitializer",
				WebResourceUtility.GetUrl(GetType(), "Zeus.Admin.Plugins.Tree.Resources.TreeCssInitializer.js"));

			// Call tree modification plugins.
			foreach (ITreePlugin treePlugin in Context.Current.ResolveAll<ITreePlugin>())
			{
				string[] requiredScripts = treePlugin.RequiredScripts;
				if (requiredScripts != null)
					foreach (string requiredScript in requiredScripts)
						scriptManager.RegisterClientScriptInclude(treePlugin.GetType().FullName, requiredScript);
			}
		}
	}
}