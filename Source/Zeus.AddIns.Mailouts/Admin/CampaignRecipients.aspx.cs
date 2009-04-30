﻿using System.Collections.Generic;
using Isis.Web.Hosting;
using Zeus.AddIns.Mailouts.ContentTypes;
using Zeus.AddIns.Mailouts.Services;
using Zeus.Admin;

[assembly: EmbeddedResourceFile("Zeus.AddIns.Mailouts.Admin.CampaignRecipients.aspx", "Zeus.AddIns.Mailouts.Admin")]
namespace Zeus.AddIns.Mailouts.Admin
{
	public partial class CampaignRecipients : AdminPage
	{
		protected IEnumerable<IMailoutRecipient> GetRecipients()
		{
			return Zeus.Context.Current.Resolve<IMailoutService>().GetRecipients((Campaign) SelectedItem);
		}
	}
}
