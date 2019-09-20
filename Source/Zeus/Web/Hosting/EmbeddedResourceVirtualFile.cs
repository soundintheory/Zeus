using System;
using System.Diagnostics;
using System.Reflection;

namespace Zeus.Web.Hosting
{
	[DebuggerDisplay("Assembly = {ContainingAssembly.FullName}, ResourcePath = {ResourcePath}, VirtualPath = {VirtualPath}")]
	public class EmbeddedResourceVirtualFile : System.Web.Hosting.VirtualFile
	{
		public Assembly ContainingAssembly { get; }

		public string ResourcePath { get; }

		public EmbeddedResourceVirtualFile(string virtualPath, Assembly containingAssembly, string resourcePath)
			: base(virtualPath)
		{
			if (containingAssembly == null)
			{
				throw new ArgumentNullException(nameof(containingAssembly));
			}

			if (resourcePath == null)
			{
				throw new ArgumentNullException(nameof(resourcePath));
			}

			if (resourcePath.Length == 0)
			{
				throw new ArgumentOutOfRangeException(nameof(resourcePath));
			}

			ContainingAssembly = containingAssembly;
			ResourcePath = resourcePath;
		}

		public override System.IO.Stream Open()
		{
			return ContainingAssembly.GetManifestResourceStream(ResourcePath);
		}
	}
}