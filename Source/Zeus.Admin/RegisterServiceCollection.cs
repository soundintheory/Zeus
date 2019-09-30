using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Extensions.DependencyInjection;
using Zeus.Admin.Plugins;
using Zeus.Admin.Plugins.ContextMenu;
using Zeus.Admin.Plugins.FileManager;
using Zeus.Admin.Plugins.ManageZones;
using Zeus.Admin.Plugins.Tree;
using Zeus.BaseLibrary;

namespace Zeus.Admin
{
	public class RegisterServiceCollection : IServiceRegistration
	{
		public void RegisterServices(ref IServiceCollection services)
		{
			// TODO: add IMainInterface IEnumerable to 
			services.AddSingleton<IMainInterfacePlugin, ContextMenuMainInterfacePlugin>();
			services.AddSingleton<IMainInterfacePlugin, FileManagerMainInterfacePlugin>();
			services.AddSingleton<IMainInterfacePlugin, ManageZonesMainInterfacePlugin>();
			services.AddSingleton<IMainInterfacePlugin, TreeMainInterfacePlugin>();
		}
	}
}