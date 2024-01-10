using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Web;
using System.Web.Routing;

namespace Zeus.Web.Routing
{
	public class EmbeddedContentRouteHandler : IRouteHandler
	{
		private readonly Assembly _assembly;
		private readonly string _resourcePath;

		public EmbeddedContentRouteHandler(Assembly assembly, string resourcePath)
		{
			_assembly = assembly;
			_resourcePath = resourcePath;
		}

		public IHttpHandler GetHttpHandler(RequestContext requestContext)
		{
			return new EmbeddedContentHttpHandler(this, requestContext);
		}

		public Stream GetStream(string resource)
		{
			return _assembly.GetManifestResourceStream(_resourcePath + "." + resource.Replace('/', '.'));
		}

		internal class EmbeddedContentHttpHandler : IHttpHandler
		{
			private readonly EmbeddedContentRouteHandler _routeHandler;
			private readonly RequestContext _requestContext;

			public EmbeddedContentHttpHandler(EmbeddedContentRouteHandler routeHandler, RequestContext requestContext)
			{
				_routeHandler = routeHandler;
				_requestContext = requestContext;
			}

			public bool IsReusable { get { return false; } }

			private static readonly ConcurrentDictionary<Assembly, Lazy<DateTime>> _assemblyLastModifiedCache =
				new ConcurrentDictionary<Assembly, Lazy<DateTime>>();

			private static DateTime GetAssemblyLastModified(Assembly assembly)
			{
				var entry = _assemblyLastModifiedCache.GetOrAdd(assembly, (a) =>
				{
					return new Lazy<DateTime>(() => new DateTime(File.GetLastWriteTime(new Uri(a.GetName().CodeBase).LocalPath).Ticks));
				});

				return entry.Value;
			}

			public void ProcessRequest(HttpContext context)
			{
				HttpCachePolicy cache = context.Response.Cache;
				cache.SetCacheability(HttpCacheability.Public);
				cache.SetOmitVaryStar(true);
				cache.SetExpires(DateTime.Now + TimeSpan.FromDays(365.0));
				cache.SetValidUntilExpires(true);
				cache.SetLastModified(GetAssemblyLastModified(_routeHandler._assembly));

				var resource = _requestContext.RouteData.GetRequiredString("resource");
				switch (Path.GetExtension(resource))
				{
					case ".css":
						context.Response.ContentType = "text/css";
						break;
					case ".js":
						context.Response.ContentType = "application/x-javascript";
						break;
					case ".png":
						context.Response.ContentType = "image/png";
						break;
					case ".gif":
						context.Response.ContentType = "image/gif";
						break;
					case ".jpg":
						context.Response.ContentType = "image/jpeg";
						break;
					case ".bmp":
						context.Response.ContentType = "image/bmp";
						break;
				}
				using (var stream = _routeHandler.GetStream(resource))
				{
					if (stream == null)
						throw new ZeusException("Could not find embedded resource with name '" + resource + "'.");

					var buffer = new byte[1024];
					for (; ; )
					{
						var size = stream.Read(buffer, 0, buffer.Length);
						if (size == 0)
							break;
						context.Response.OutputStream.Write(buffer, 0, size);
					}
				}
			}
		}
	}
}