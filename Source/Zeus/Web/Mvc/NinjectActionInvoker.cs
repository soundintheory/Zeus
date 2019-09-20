using System.Linq;
using System.Web.Mvc;
using Ninject;

namespace Zeus.Web.Mvc
{
	/// <summary>
	/// An <see cref="IActionInvoker"/> that injects filters with dependencies.
	/// </summary>
	public class NinjectActionInvoker : ControllerActionInvoker
	{
		/// <summary>
		/// Gets or sets the kernel.
		/// </summary>
		public IKernel Kernel { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="NinjectActionInvoker"/> class.
		/// </summary>
		/// <param name="kernel">The kernel.</param>
		public NinjectActionInvoker(IKernel kernel)
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
			var filterInfo = base.GetFilters(controllerContext, actionDescriptor);

			foreach (var filter in filterInfo.ActionFilters.Where(f => f != null))
			{
				Kernel.Inject(filter);
			}

			foreach (var filter in filterInfo.AuthorizationFilters.Where(f => f != null))
			{
				Kernel.Inject(filter);
			}

			foreach (var filter in filterInfo.ExceptionFilters.Where(f => f != null))
			{
				Kernel.Inject(filter);
			}

			foreach (var filter in filterInfo.ResultFilters.Where(f => f != null))
			{
				Kernel.Inject(filter);
			}

			return filterInfo;
		}
	}
}