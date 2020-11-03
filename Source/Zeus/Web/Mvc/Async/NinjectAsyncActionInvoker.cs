using Ninject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Mvc.Async;

namespace Zeus.Web.Mvc.Async
{
    /// <summary>
	/// An <see cref="IAsyncActionInvoker"/> that injects filters with dependencies.
	/// </summary>
    public class NinjectAsyncActionInvoker : AsyncControllerActionInvoker
    {
		/// <summary>
		/// Gets or sets the kernel.
		/// </summary>
		public IKernel Kernel { get; private set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="NinjectAsyncActionInvoker"/> class.
		/// </summary>
		/// <param name="kernel">The kernel.</param>
		public NinjectAsyncActionInvoker(IKernel kernel)
		{
			Kernel = kernel;
		}

		/// <summary>
		/// Gets the filters for the specified request and action.
		/// </summary>
		/// <param name="controllerContext">The controller context.</param>
		/// <param name="actionDescriptor">The action descriptor.</param>
		/// <returns>The filters.</returns>
		protected override FilterInfo GetFilters(ControllerContext controllerContext, ActionDescriptor actionDescriptor)
		{
			FilterInfo filterInfo = base.GetFilters(controllerContext, actionDescriptor);

			foreach (IActionFilter filter in filterInfo.ActionFilters.Where(f => f != null))
				Kernel.Inject(filter);

			foreach (IAuthorizationFilter filter in filterInfo.AuthorizationFilters.Where(f => f != null))
				Kernel.Inject(filter);

			foreach (IExceptionFilter filter in filterInfo.ExceptionFilters.Where(f => f != null))
				Kernel.Inject(filter);

			foreach (IResultFilter filter in filterInfo.ResultFilters.Where(f => f != null))
				Kernel.Inject(filter);

			return filterInfo;
		}
	}
}
