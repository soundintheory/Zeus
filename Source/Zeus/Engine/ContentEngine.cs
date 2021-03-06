﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Reflection;
using Zeus.Admin;
using Zeus.BaseLibrary.DependencyInjection;
using Zeus.BaseLibrary.Reflection;
using Zeus.BaseLibrary.Web;
using Zeus.Configuration;
using Zeus.ContentTypes;
using Zeus.Globalization;
using Zeus.Persistence;
using Zeus.Plugin;
using Zeus.Security;
using Zeus.Web;
using Zeus.Web.Hosting;
using Zeus.Web.Security;
using IWebContext=Zeus.Web.IWebContext;

namespace Zeus.Engine
{
	public class ContentEngine
	{
		private readonly DependencyInjectionManager _dependencyInjectionManager;

		#region Properties

		public IAdminManager AdminManager
		{
			get { return Resolve<IAdminManager>(); }
		}

		public IFinder Finder
		{
			get { return Resolve<IFinder>(); }
		}

		public IContentManager ContentManager
		{
			get { return Resolve<IContentManager>(); }
		}

		public IContentTypeManager ContentTypes
		{
			get { return Resolve<IContentTypeManager>(); }
		}

		public ILanguageManager LanguageManager
		{
			get { return Resolve<ILanguageManager>(); }
		}

		public IPersister Persister
		{
			get { return Resolve<IPersister>(); }
		}

		public ISecurityManager SecurityManager
		{
			get { return Resolve<ISecurityManager>(); }
		}

		public IWebSecurityService WebSecurity
		{
			get { return Resolve<IWebSecurityService>(); }
		}

		public IHost Host
		{
			get { return Resolve<IHost>(); }
		}

		public IUrlParser UrlParser
		{
			get { return Resolve<IUrlParser>(); }
		}

		public IWebContext WebContext
		{
			get { return Resolve<IWebContext>(); }
		}

		#endregion

		#region Constructor

		public ContentEngine(EventBroker eventBroker)
		{
			HostSection hostSection = (HostSection) ConfigurationManager.GetSection("zeus/host");

			_dependencyInjectionManager = new DependencyInjectionManager();
			_dependencyInjectionManager.Bind<IAssemblyFinder, AssemblyFinder>();

			_dependencyInjectionManager.BindInstance(eventBroker);
			_dependencyInjectionManager.BindInstance(ConfigurationManager.GetSection("zeus/database") as DatabaseSection);
			_dependencyInjectionManager.BindInstance(hostSection);
			_dependencyInjectionManager.BindInstance(ConfigurationManager.GetSection("zeus/admin") as AdminSection);
			_dependencyInjectionManager.BindInstance(ConfigurationManager.GetSection("zeus/contentTypes") as ContentTypesSection);
			_dependencyInjectionManager.BindInstance(ConfigurationManager.GetSection("zeus/dynamicContent") as DynamicContentSection);
            _dependencyInjectionManager.BindInstance(ConfigurationManager.GetSection("zeus/globalization") as GlobalizationSection ?? new GlobalizationSection());
            _dependencyInjectionManager.BindInstance(ConfigurationManager.GetSection("zeus/customUrls") as CustomUrlsSection ?? new CustomUrlsSection());
            _dependencyInjectionManager.BindInstance(ConfigurationManager.GetSection("zeus/routing") as RoutingSection ?? new RoutingSection());

            if (hostSection != null && hostSection.Web != null)
				Url.DefaultExtension = hostSection.Web.Extension;
		}

		#endregion

		public void AddComponent(string key, Type serviceType, Type classType)
		{
			_dependencyInjectionManager.Bind(serviceType, classType);
		}

		public void AddComponent(string key, Type classType)
		{
			_dependencyInjectionManager.Bind(classType);
		}

		public void AddComponentInstance(string key, object instance)
		{
			_dependencyInjectionManager.BindInstance(instance);
		}

		public void Bind<TService, TComponent>()
			where TComponent : TService
		{
			_dependencyInjectionManager.Bind<TService, TComponent>();
		}

		public void Initialize()
		{
			_dependencyInjectionManager.BindInstance(this);

			WebSecurityEngine.DependencyInjectionManager = _dependencyInjectionManager;

			IPluginBootstrapper invoker = Resolve<IPluginBootstrapper>();
			invoker.InitializePlugins(this, invoker.GetPluginDefinitions());

			_dependencyInjectionManager.Initialize();
		}

		public T Resolve<T>()
		{
			return _dependencyInjectionManager.Get<T>();
		}

		public IEnumerable<T> ResolveAll<T>()
		{
			return _dependencyInjectionManager.GetAll<T>();
		}

		/// <summary>Resolves a service configured for the factory.</summary>
		/// <param name="serviceType">The type of service to resolve.</param>
		/// <returns>An instance of the resolved service.</returns>
		public object Resolve(Type serviceType)
		{
			return _dependencyInjectionManager.Get(serviceType);
		}

		public string GetServerResourceUrl(Assembly assembly, string resourcePath)
		{
			return Resolve<IEmbeddedResourceManager>().GetServerResourceUrl(assembly, resourcePath);
		}

		public string GetServerResourceUrl(Type type, string resourcePath)
		{
			return GetServerResourceUrl(type.Assembly, resourcePath);
		}

		/*/// <summary>Releases a component from the IoC container.</summary>
		/// <param name="instance">The component instance to release.</param>
		public void Release(object instance)
		{
			IoC.Container.Release(instance);
		}*/

		/*public void AddComponentLifeStyle(string key, Type classType, ComponentLifeStyle lifeStyle)
		{
			LifestyleType lifeStyleType = lifeStyle == ComponentLifeStyle.Singleton
				? LifestyleType.Singleton
				: LifestyleType.Transient;

			Container.AddComponentLifeStyle(key, classType, lifeStyleType);
		}*/
	}

	public enum ComponentLifeStyle
	{
		Singleton = 0,
		Transient = 1,
	}
}
