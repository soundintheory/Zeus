using System;
using System.Collections.Generic;
using Zeus.BaseLibrary.ExtensionMethods.Web;
using Zeus.Security;

namespace Zeus.Admin.Plugins.MoveItem
{
	[ActionPluginGroup("CutCopyPaste", 20)]
	public partial class Default : PreviewFrameAdminPage
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			var sourceContentItem = this.SelectedItem;
			var destinationContentItem = Zeus.Context.Current.Resolve<Navigator>().Navigate(Request.GetRequiredString("destination"));

			// Check user has permission to create items under the SelectedItem
			if (!Engine.SecurityManager.IsAuthorized(destinationContentItem, User, Operations.Create))
			{
				lblNotAuthorised.Visible = true;
				return;
			}

			// Change parent if necessary.
			if (sourceContentItem.Parent.ID != destinationContentItem.ID)
			{
				Zeus.Context.Persister.Move(sourceContentItem, destinationContentItem);
			}

			// Update sort order based on new pos.
			var pos = Request.GetRequiredInt("pos");
			var siblings = sourceContentItem.Parent.Children;
			Utility.MoveToIndex(siblings, sourceContentItem, pos);
			foreach (var updatedItem in Utility.UpdateSortOrder(siblings))
			{
				Zeus.Context.Persister.Save(updatedItem);
			}

			Refresh(sourceContentItem, AdminFrame.Both, false);
		}
	}
}
