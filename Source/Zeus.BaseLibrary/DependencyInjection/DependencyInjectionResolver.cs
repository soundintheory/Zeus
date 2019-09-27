using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Zeus.BaseLibrary.DependencyInjection
{
	public class DependencyInjectionResolver : IDependencyResolver
	{
		private readonly IServiceProvider _services;

		public DependencyInjectionResolver(IServiceProvider services)
		{
			_services = services;
		}

		public object GetService(Type serviceType) => _services.GetService(serviceType);
		public IEnumerable<object> GetServices(Type serviceType) => _services.GetServices(serviceType);
	}
}
