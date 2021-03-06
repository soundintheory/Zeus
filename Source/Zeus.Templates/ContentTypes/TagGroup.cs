using Ext.Net;
using Zeus.Integrity;

namespace Zeus.Templates.ContentTypes
{
	[ContentType("Tag Group", Description = "Defines a tag group that pages can be associated with.")]
	[RestrictParents(typeof(ITagGroupContainer))]
	public class TagGroup : BasePage
	{
		public override string IconUrl
		{
			get { return Utility.GetCooliteIconUrl(Icon.TagBlue); }
		}
	}
}