using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Web.Routing;
using Ninject;
using Zeus.BaseLibrary.DependencyInjection;

namespace Zeus.Web.Hosting
{
	public class EmbeddedResourceBuilder : IInitializable, IEmbeddedResourceBuilder
	{
		private readonly IKernel _kernel;

		public EmbeddedResourceBuilder(IKernel kernel)
		{
			_kernel = kernel;
			ResourceSettings = new ResourceSettings();
		}

		public ResourceSettings ResourceSettings { get; }

		public void Initialize()
		{
			var searchPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, AppDomain.CurrentDomain.RelativeSearchPath);
			IEnumerable<string> filenames = Directory.GetFiles(searchPath, "*.dll");
			DependencyInjectionUtility.RegisterAllComponents<IEmbeddedResourcePackage>(_kernel, filenames);

			foreach (var package in _kernel.GetAll<IEmbeddedResourcePackage>())
			{
				package.Register(RouteTable.Routes, ResourceSettings);
			}
		}
	}
}