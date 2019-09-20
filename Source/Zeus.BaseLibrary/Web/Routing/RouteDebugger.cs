using System.Web.Routing;

namespace Zeus.BaseLibrary.Web.Routing
{
	public static class RouteDebugger
	{
		// Methods
		public static void RewriteRoutesForTesting(RouteCollection routes)
		{
			using (routes.GetReadLock())
			{
				var flag = false;
				foreach (var base2 in routes)
				{
					var route = base2 as Route;
					if (route != null)
						route.RouteHandler = new DebugRouteHandler();
					if (route == DebugRoute.Singleton)
						flag = true;
				}
				if (!flag)
					routes.Add(DebugRoute.Singleton);
			}
		}
	}
}