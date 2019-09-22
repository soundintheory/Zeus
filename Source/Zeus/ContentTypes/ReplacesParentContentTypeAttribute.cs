using System;
using System.Collections.Generic;

namespace Zeus.ContentTypes
{
	[AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
	/// <summary>
	/// Replaces the parent content type with the one decorated by this 
	/// attribute. This can be used to disable and replace items in external
	/// class libraries.
	/// </summary>
	public class ReplacesParentContentTypeAttribute : AbstractContentTypeRefiner, IDefinitionRefiner
	{
		public override void Refine(ContentType currentContentType, IList<ContentType> allContentTypes)
		{
			var t = currentContentType.ItemType;
			foreach (var contentType in allContentTypes)
			{
				if (contentType.ItemType == t.BaseType)
				{
					contentType.Enabled = false;
					return;
				}
			}
		}
	}
}