using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Routing;
using Zeus.BaseLibrary.Reflection;

namespace Zeus.Web.Hosting
{
	public class EmbeddedResourceBuilder : IEmbeddedResourceBuilder
	{
		private readonly ITypeFinder _typeFinder;

		public EmbeddedResourceBuilder(ITypeFinder typeFinder)
		{
			_typeFinder = typeFinder;
			ResourceSettings = new ResourceSettings();

			foreach (var package in _typeFinder.Find(typeof(IEmbeddedResourcePackage)).Cast<IEmbeddedResourcePackage>())
			{
				package.Register(RouteTable.Routes, ResourceSettings);
			}
		}

		public ResourceSettings ResourceSettings { get; }
	}
}