﻿using Ext.Net;
using Zeus.Integrity;
using Zeus.ContentTypes;

namespace Zeus.Templates.ContentTypes.ReferenceData
{
	[ContentType("Reference Data", Description = "Container for all types of reference data, such as a list of countries.")]
	[RestrictParents(typeof(SystemNode))]
	public class ReferenceDataNode : BaseContentItem, ISelfPopulator
	{
		public ReferenceDataNode()
		{
			Name = "reference-data";
			Title = "Reference Data";
		}

		public override string IconUrl
		{
			get { return Utility.GetCooliteIconUrl(Icon.BookOpen); }
		}

		public void Populate()
		{
			Children.Add(new CountryList() { Parent = this });
		}
	}
}
