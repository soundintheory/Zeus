using System.Collections.Generic;
using System.Configuration;
using System.Reflection;
using System.Web.Routing;
using Zeus.Configuration;
using Zeus.Web.Hosting;

namespace Zeus.Admin
{
	public class ZeusAdminEmbeddedResourcePackage : EmbeddedResourcePackageBase
	{
		public override void Register(RouteCollection routes, ResourceSettings resourceSettings)
		{
			var adminSection = (AdminSection) ConfigurationManager.GetSection("zeus/admin");
			RegisterStandardArea(routes, resourceSettings, adminSection.Path, "Assets");
		}
	}
}