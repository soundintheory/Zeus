using System;
using System.Globalization;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.SessionState;
using Ninject;
using System.Linq;

namespace Zeus.Web.Mvc
{
	public class ControllerFactory : DefaultControllerFactory
	{

		private readonly IControllerMapper _mapper;

		public ControllerFactory(IControllerMapper mapper)
		{
			_mapper = mapper;
		}

		public override IController CreateController(RequestContext requestContext, string controllerName)
		{
			var controllerKey = controllerName.ToLowerInvariant();

			if (requestContext.RouteData.Values.TryGetValue("area", out var area))
			{
				var areaControllerKey = Convert.ToString(area).ToLowerInvariant() + "." + controllerKey;
				var areaController = InstantiateController(areaControllerKey);
				if (areaController != null)
				{
					return areaController;
				}
			}

			var controller = InstantiateController(controllerKey);

			return controller != null ? controller : base.CreateController(requestContext, controllerName);
		}

		public SessionStateBehavior GetControllerSessionBehavior(RequestContext requestContext, string controllerName)
		{
			return SessionStateBehavior.Default;
		}

		private IController InstantiateController(string controllerName)
		{
			if (!_mapper.Contains(controllerName))
			{
				// not a zeus route.
				// let other handlers take over
				return null;
			}

#if DEBUG
			IController controller = _kernel.Get<IController>(controllerName.ToLowerInvariant());
#else
			var controller = _kernel.TryGet<IController>(controllerName.ToLowerInvariant());
#endif

			var standardController = controller as Controller;

			return controller;
		}
	}
}