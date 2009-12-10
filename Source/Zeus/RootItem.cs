using Coolite.Ext.Web;
using Zeus.ContentTypes;
using Zeus.Integrity;

namespace Zeus
{
	[ContentType("Root Item", Installer = Installation.InstallerHints.PreferredRootPage)]
	[RestrictParents(AllowedTypes.None)]
	public class RootItem : ContentItem, IRootItem
	{
		public override string IconUrl
		{
			get { return Utility.GetCooliteIconUrl(Icon.PageGear); }
		}
	}
}