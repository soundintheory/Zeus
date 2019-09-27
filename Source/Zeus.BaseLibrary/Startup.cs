using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Zeus.BaseLibrary.DependencyInjection;
using Zeus.BaseLibrary.Reflection;

namespace Zeus.BaseLibrary
{
	public static class Startup
	{
		/// <summary>
		/// Registers services at app start. 
		/// </summary>
		/// <remarks>Call this from ApplicationStart</remarks>
		public static void ConfigureStartup()
		{
			var provider = Configuration();
			var resolver = new DependencyInjectionResolver(provider);
			DependencyResolver.SetResolver(resolver);
		}

		private static IServiceProvider Configuration()
		{
			var services = new ServiceCollection();

			services.AddServiceCollections();

			return services.BuildServiceProvider();
		}

		private static IServiceCollection AddServiceCollections(this IServiceCollection provider)
		{
			var typeFinder = new TypeFinder();
			foreach (var type in typeFinder.Find(typeof(IServiceRegistration)).Cast<IServiceRegistration>())
			{
				type.RegisterServices(out provider);
			}
			return provider;
		}
	}
}
