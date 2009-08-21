using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;
using Ninject;
using Zeus.Web.Mvc.Modules;

namespace Zeus.Templates.Mvc
{
	public class TemplatesWebPackage : WebPackageBase
	{
		public const string AREA_NAME = "Templates";

		public override void Register(IKernel container, ICollection<RouteBase> routes, ICollection<IViewEngine> viewEngines)
		{
			RegisterStandardArea(container, routes, viewEngines, "Templates");
		}
	}
}