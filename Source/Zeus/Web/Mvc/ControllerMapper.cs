using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Ninject;
using Zeus.BaseLibrary.Reflection;
using Zeus.ContentTypes;

namespace Zeus.Web.Mvc
{
	public class ControllerMapper : IControllerMapper
	{
		public ControllerMapper(ITypeFinder typeFinder, IContentTypeManager definitionManager, IKernel kernel)
		{
			var controllerDefinitions = FindControllers(typeFinder);
			foreach (var id in definitionManager.GetContentTypes())
			{
				var controllerDefinition = GetControllerFor(id.ItemType, controllerDefinitions);
				if (controllerDefinition != null)
				{
					var controllerName = GetControllerName(controllerDefinition.AdapterType, controllerDefinition.AreaName);

					ControllerMap[id.ItemType] = controllerDefinition.ControllerName;
					AreaMap[id.ItemType] = controllerDefinition.AreaName;
					_ = ControllerNames.Add(controllerName);

					if (!kernel.GetBindings(typeof(IController)).Any(b => b.Metadata.Name == controllerName))
					{
						kernel.Bind<IController>().To(controllerDefinition.AdapterType)
							.InTransientScope()
							.Named(controllerName);
					}

					IList<IPathFinder> finders = PathDictionary.GetFinders(id.ItemType);
					if (0 == finders.Count(f => f is ActionResolver))
					{
						// TODO: Get the list of methods from a list of actions retrieved from somewhere within MVC
						var methods = controllerDefinition.AdapterType.GetMethods().Select(m => m.Name).ToArray();
						var actionResolver = new ActionResolver(this, methods);
						PathDictionary.PrependFinder(id.ItemType, actionResolver);
					}
				}
			}
		}

		private static string GetControllerName(Type type, string areaName)
		{
			var name = type.Name.ToLowerInvariant();

			if (name.EndsWith("controller"))
			{
				name = name.Substring(0, name.IndexOf("controller"));
			}

			if (!string.IsNullOrEmpty(areaName))
			{
				name = areaName.ToLowerInvariant() + "." + name;
			}

			return name;
		}

		public string GetControllerName(Type type)
		{
			string name;
			ControllerMap.TryGetValue(type, out name);
			return name;
		}

		public string GetAreaName(Type type)
		{
			string name;
			AreaMap.TryGetValue(type, out name);
			return name;
		}

		public bool Contains(string name)
		{
			var query = name.Trim().ToLower();

			return ControllerNames.Contains(query);
		}

		private IDictionary<Type, string> ControllerMap { get; } = new Dictionary<Type, string>();

		private IDictionary<Type, string> AreaMap { get; } = new Dictionary<Type, string>();

		private HashSet<string> ControllerNames { get; } = new HashSet<string>();

		private static IAdapterDescriptor GetControllerFor(Type itemType, IList<ControlsAttribute> controllerDefinitions)
		{
			var controllers = new List<ControlsAttribute>();
			foreach (var controllerDefinition in controllerDefinitions)
			{
				if (controllerDefinition.ItemType.IsAssignableFrom(itemType))
				{
					controllers.Add(controllerDefinition);
				}
			}

			return controllers.OrderByDescending(c => c.Priority).FirstOrDefault();
		}

		private static IList<ControlsAttribute> FindControllers(ITypeFinder typeFinder)
		{
			var controllerDefinitions = new List<ControlsAttribute>();
			foreach (var controllerType in typeFinder.Find(typeof(IController)))
			{
				foreach (ControlsAttribute attr in controllerType.GetCustomAttributes(typeof(ControlsAttribute), false))
				{
					attr.AdapterType = controllerType;
					controllerDefinitions.Add(attr);
				}
			}
			controllerDefinitions.Sort();
			return controllerDefinitions;
		}
	}
}