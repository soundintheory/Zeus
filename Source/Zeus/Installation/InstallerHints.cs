using System;

namespace Zeus.Installation
{
	/// <summary>
	/// Hint for the installer how to treat a certain item definition.
	/// </summary>
	[Flags]
	public enum InstallerHints
	{
		/// <summary>May be either start or root page unless another item is preferred as start or root page.</summary>
		Default = 0,
		/// <summary>Is preferred as root page by the installer.</summary>
		PreferredRootPage = 1,
		/// <summary>Is preferred as start page by the installer.</summary>
		PreferredStartPage = 2,
		/// <summary>Will never be placed as root page by the installer.</summary>
		NeverRootPage = 4,
		/// <summary>Will never be placed as start page by the installer.</summary>
		NeverStartPage = 8,
		/// <summary>Will never be placed as start or root page by the installer.</summary>
		NeverRootOrStartPage = NeverRootPage | NeverStartPage
	}
}