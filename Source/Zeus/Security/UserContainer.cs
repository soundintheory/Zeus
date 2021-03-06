﻿using Ext.Net;
using Zeus.Integrity;
using Zeus.Security.ContentTypes;

namespace Zeus.Web.Security.Items
{
	[ContentType("User Container")]
	[RestrictParents(typeof(SecurityContainer))]
	public class UserContainer : DataContentItem
	{
		public const string ContainerName = "users";
		public const string ContainerTitle = "Users";

		public UserContainer()
		{
			Name = ContainerName;
			Title = ContainerTitle;
		}

		public override string IconUrl
		{
			get { return Utility.GetCooliteIconUrl(Icon.Group); }
		}
	}
}
