using System.Web.Routing;

namespace Zeus.BaseLibrary.Web.Routing
{
	public class DebugRoute : Route
	{
		// Methods
		private DebugRoute()
			: base("{*catchall}", new DebugRouteHandler())
		{
		}

		// Properties
		public static DebugRoute Singleton { get; } = new DebugRoute();
	}
}