using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zeus.BaseLibrary
{
	/// <summary>
	/// Implement IServiceRegistration to register additional services against the DI Containers
	/// </summary>
	public interface IServiceRegistration
	{
		void RegisterServices(ref IServiceCollection services);
	}
}
