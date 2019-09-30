using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Zeus.BaseLibrary;
using Zeus.BaseLibrary.Web;
using Zeus.Configuration;
using Zeus.ContentProperties;
using Zeus.ContentTypes;
using Zeus.DynamicContent;
using Zeus.FileSystem;
using Zeus.Globalization;
using Zeus.Installation;
using Zeus.Integrity;
using Zeus.Net;
using Zeus.Persistence;
using Zeus.Security;
using Zeus.Serialization;
using Zeus.Web;
using Zeus.Web.Caching;
using Zeus.Web.Hosting;
using Zeus.Web.Mvc;
using Zeus.Web.Mvc.Html;
using Zeus.Web.Security;
using Zeus.Web.TextTemplating;

namespace Zeus.Engine
{
	public class RegisterServiceCollection : IServiceRegistration
	{
		public void RegisterServices(ref IServiceCollection services)
		{
			services.AddSingleton<EventBroker>();

			var hostSection = ConfigurationManager.GetSection("zeus/host") as HostSection;
			if (hostSection?.Web != null)
			{
				Url.DefaultExtension = hostSection.Web.Extension;
			}
			else
			{
				hostSection = new HostSection();
			}

			services.AddSingleton(ConfigurationManager.GetSection("zeus/database") as DatabaseSection);
			services.AddSingleton(hostSection);
			services.AddSingleton(ConfigurationManager.GetSection("zeus/admin") as AdminSection);
			services.AddSingleton(ConfigurationManager.GetSection("zeus/contentTypes") as ContentTypesSection);
			services.AddSingleton(ConfigurationManager.GetSection("zeus/dynamicContent") as DynamicContentSection);
			services.AddSingleton(ConfigurationManager.GetSection("zeus/globalization") as GlobalizationSection ?? new GlobalizationSection());
			services.AddSingleton(ConfigurationManager.GetSection("zeus/customUrls") as CustomUrlsSection ?? new CustomUrlsSection());
			services.AddSingleton(ConfigurationManager.GetSection("zeus/routing") as RoutingSection ?? new RoutingSection());

			//services.AddSingleton<INavigationCachingService, NavigationCachingService>();
			services.AddSingleton<IContentAdapterProvider, ContentAdapterProvider>();

			// Admin
			services.AddSingleton<IContentManager, ContentManager>();

			// Admin
			//services.AddSingleton<IAdminManager, AdminManager>();
			//services.AddSingleton<Navigator>();

			// Content Properties
			services.AddSingleton<IContentPropertyManager, ContentPropertyManager>();

			// Content Types
			services.TryAdd(ServiceDescriptor.Singleton(typeof(IAttributeExplorer<>), typeof(AttributeExplorer<>)));
			services.TryAdd(ServiceDescriptor.Singleton(typeof(IEditableHierarchyBuilder<>), typeof(EditableHierarchyBuilder<>)));
			services.AddSingleton<IContentTypeBuilder, ContentTypeBuilder>();
			services.AddSingleton<IContentTypeManager, ContentTypeManager>();
			services.AddSingleton<ContentTypeConfigurationService>();

			// Dynamic Content
			services.AddSingleton<IDynamicContentManager, DynamicContentManager>();

			// Engine
			services.AddSingleton<IContentAdapterProvider, ContentAdapterProvider>();

			// File System
			services.AddSingleton<IFileSystemService, FileSystemService>();

			// Globalization
			services.AddSingleton<ILanguageManager, LanguageManager>();

			// Installation
			services.AddSingleton<InstallationManager, InstallationManager>();

			// Integrity
			services.AddSingleton<IIntegrityManager, IntegrityManager>();
			services.AddSingleton<IIntegrityEnforcer, IntegrityEnforcer>();

			// Net
			services.AddSingleton<IHttpClient, HttpClient>();

			// Persistence
			services.AddSingleton<IItemNotifier, ItemNotifier>();
			services.AddSingleton<IPersister, ContentPersister>();
			services.AddSingleton<IVersionManager, VersionManager>();

			// Security
			services.AddSingleton<ISecurityEnforcer, SecurityEnforcer>();
			services.AddSingleton<ISecurityManager, SecurityManager>();

			// Serialization
			services.AddSingleton<Exporter, GZipExporter>();
			services.AddSingleton<ItemXmlWriter, ItemXmlWriter>();
			services.AddSingleton<Importer, GZipImporter>();
			services.AddSingleton<ItemXmlReader, ItemXmlReader>();

			// Web
			services.AddSingleton<IErrorHandler, ErrorHandler>();
			services.AddSingleton<IHost, Host>();
			services.AddSingleton<IUrlParser, MultipleSitesUrlParser>();
			services.AddSingleton<IPermanentLinkManager, PermanentLinkManager>();
			services.AddSingleton<IWebSecurityManager, CredentialStore>();
			services.AddSingleton<PermissionDeniedHandler, PermissionDeniedHandler>(); // FIX
			services.AddSingleton<IRequestDispatcher, RequestDispatcher>();
			services.AddSingleton<IRequestLifecycleHandler, RequestLifecycleHandler>();
			services.AddSingleton<Web.IWebContext, WebRequestContext>();
			services.AddSingleton<IEmbeddedResourceBuilder, EmbeddedResourceBuilder>();
			services.AddSingleton<IEmbeddedResourceManager, EmbeddedResourceManager>();

			// Web / Caching
			services.AddSingleton<ICachingService, CachingService>();

			// Web / Text Templating
			services.AddSingleton<IMessageBuilder, DefaultMessageBuilder>();

			// Web / MVC
			services.AddSingleton<ITemplateRenderer, TemplateRenderer>();
			services.AddSingleton<IControllerMapper, ControllerMapper>();
			services.AddSingleton<IControllerFactory, ControllerFactory>();

			// Web / Security
			services.AddSingleton<BaseLibrary.Web.IWebContext, WebContext>();
			services.AddSingleton<IAuthorizationService, AuthorizationService>();
			services.AddSingleton<ICredentialStore, CredentialStore>();
			services.AddSingleton<ICredentialService, CredentialService>();
			services.AddSingleton<IAuthenticationContextService, AuthenticationContextService>();
			//services.AddSingleton<IAuthenticationContextInitializer, SecurityInitializer>();
			//services.AddSingleton<IAuthorizationInitializer, SecurityInitializer>();
			services.AddSingleton<IWebSecurityService, WebSecurityService>();
		}
	}
}
