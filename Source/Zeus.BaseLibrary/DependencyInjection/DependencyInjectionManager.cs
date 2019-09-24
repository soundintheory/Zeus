using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using Ninject;
using Ninject.Activation;
using Ninject.Activation.Strategies;
using Ninject.Planning.Bindings;
using Zeus.BaseLibrary.Reflection;

namespace Zeus.BaseLibrary.DependencyInjection
{
	public class DependencyInjectionManager
	{
		public readonly InitializableKernel Kernel;

		public class InitializableKernel : StandardKernel
		{
			private readonly List<IBinding> _bindings = new List<IBinding>();

			public InitializableKernel()
			{
			}

			public override void AddBinding(IBinding binding)
			{
				_bindings.Add(binding);
				base.AddBinding(binding);
			}

			public void InitializeServices()
			{
				var initializableInterfaceType = typeof(IInitializable);
				var startableInterfaceType = typeof(IStartable);
				Components.Add<IActivationStrategy, StartStrategy>();
				//AddComponents();
				foreach (var binding in _bindings)
				{
					if (initializableInterfaceType.IsAssignableFrom(binding.Service) || startableInterfaceType.IsAssignableFrom(binding.Service))
					{
						var items = this.GetAll(binding.Service); // Force creation.
					}
				}
			}
		}

		private class StartStrategy : ActivationStrategy
		{
			public override void Activate(IContext context, InstanceReference reference)
			{
				reference.IfInstanceIs<IStartable>(x => x.Start());
				reference.IfInstanceIs<IInitializable>(x => x.Initialize());
			}

			public override void Deactivate(IContext context, InstanceReference reference)
			{
				reference.IfInstanceIs<IStartable>(x => x.Stop());
			}
		}

		public DependencyInjectionManager()
		{
			// Create kernel.
			Kernel = new InitializableKernel();
			try
			{
				// Get all DLLS in bin folder.
				//IEnumerable<string> files = Directory.GetFiles(GetBinDirectory(), "*.dll");
				//IEnumerable<string> files = Directory.GetFiles(Path.GetDirectoryName(GetType().Assembly.Location), "*.dll");

				// Load modules in Zeus DLLs first.
				//_kernel.Load(FindAssemblies(files.Where(s => Path.GetFileName(s).StartsWith("Zeus."))));

				// Then load non-Zeus DLLs - this gives projects a chance to override Zeus modules.
				// Actually we just load all DLLs, because DLLs that have already been loaded
				// won't get loaded again.
				//_kernel.Load(FindAssemblies(files.Where(s => !Path.GetFileName(s).StartsWith("Zeus."))));

				var assemblies = AssemblyFinder.GetAssembliesWithKnownExclusions(null).ToList();
				var zeusAssemblies = new HashSet<Assembly>();
				var nonZeusAssemblies = new HashSet<Assembly>();

				for(var i = 0; i < assemblies.Count; i++)
				{
					var assembly = assemblies[i];
					if (assembly.FullName.StartsWith("Zeus.", StringComparison.OrdinalIgnoreCase))
					{
						zeusAssemblies.Add(assembly);
					}
					else
					{
						nonZeusAssemblies.Add(assembly);
					}
				}

				// load zeus assemblies
				Kernel.Load(zeusAssemblies);
				// load non-zeus
				Kernel.Load(nonZeusAssemblies);
			}
			catch (TypeLoadException)
			{
			}
		}

        /// <summary>
        /// Gets the direcory containing the application
        /// </summary>
        /// <returns></returns>
        private string GetBinDirectory()
        {
            try
            {
                return Path.GetDirectoryName(HttpRuntime.BinDirectory);
            }
            catch (System.Exception)
            {
                return Path.GetDirectoryName(GetType().Assembly.Location);
            }
        }

        private static IEnumerable<Assembly> FindAssemblies(IEnumerable<string> filenames)
		{
			return FindAssemblyNames(filenames, a => true).Select(an => Assembly.Load(an));
		}

		private static IEnumerable<AssemblyName> FindAssemblyNames(IEnumerable<string> filenames, Predicate<Assembly> filter)
		{
			var temporaryDomain = CreateTemporaryAppDomain();

			foreach (var file in filenames)
			{
				Assembly assembly;

				if (File.Exists(file))
				{
					try
					{
						var name = new AssemblyName { CodeBase = file };
						assembly = temporaryDomain.Load(name);
					}
					catch (BadImageFormatException)
					{
						// Ignore native assemblies
						continue;
					}
				}
				else
				{
					assembly = temporaryDomain.Load(file);
				}

				if (filter(assembly))
				{
					yield return assembly.GetName();
				}
			}

			AppDomain.Unload(temporaryDomain);
		}

		private static AppDomain CreateTemporaryAppDomain()
		{
			return AppDomain.CreateDomain(
					"AssemblyScanner",
					AppDomain.CurrentDomain.Evidence,
					AppDomain.CurrentDomain.SetupInformation);
		}

		public void Initialize()
		{
			Kernel.InitializeServices();
		}

		public void Bind<TService, TComponent>()
			where TComponent : TService
		{
			Kernel.Bind<TService>().To<TComponent>();
		}

		public void Bind(Type serviceType, Type componentType)
		{
			Kernel.Bind(serviceType).To(componentType);
		}

		public void Bind(Type componentType)
		{
			Kernel.Bind(componentType).ToSelf();
		}

		public void BindInstance(object instance)
		{
			if (instance == null)
			{
				return;
			}

			Kernel.Bind(instance.GetType()).ToConstant(instance);
		}

		public TService Get<TService>()
		{
			return Kernel.GetAll<TService>().First();
		}

		public IEnumerable<TService> GetAll<TService>()
		{
			return Kernel.GetAll<TService>();
		}

		public object Get(Type serviceType)
		{
			return Kernel.Get(serviceType);
		}
	}
}