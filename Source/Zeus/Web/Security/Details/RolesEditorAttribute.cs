using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;
using Zeus.ContentProperties;
using Zeus.ContentTypes;
using Zeus.Design.Editors;

namespace Zeus.Web.Security.Details
{
	public class RolesEditorAttribute : AbstractEditorAttribute
	{
		public override bool UpdateItem(IEditableObject item, Control editor)
		{
			var cbl = editor as CheckBoxList;
			var roles = new List<Items.Role>();
			foreach (ListItem li in cbl.Items)
			{
				if (li.Selected)
				{
					var role = Context.Current.Resolve<IWebSecurityManager>().GetRole(li.Value);
					roles.Add(role);
				}
			}

			var dc = item.GetDetailCollection(Name, true);
			dc.Replace(roles);

			return true;
		}

		protected override void UpdateEditorInternal(IEditableObject item, Control editor)
		{
			var cbl = editor as CheckBoxList;
			var dc = item.GetDetailCollection(Name, false);
			if (dc != null)
			{
				foreach (Items.Role role in dc)
				{
					var li = cbl.Items.FindByValue(role.Name);
					if (li != null)
					{
						li.Selected = true;
					}
					else
					{
						li = new ListItem(role.Name);
						li.Selected = true;
						li.Attributes["style"] = "color:silver";
						cbl.Items.Add(li);
					}
				}
			}
		}

		protected override Control AddEditor(Control container)
		{
			var cbl = new CheckBoxList();
			foreach (var role in Context.Current.Resolve<IWebSecurityManager>().GetRoles(container.Page.User))
			{
				cbl.Items.Add(role.Name);
			}

			container.Controls.Add(cbl);
			return cbl;
		}

		protected override void DisableEditor(Control editor)
		{
			((CheckBoxList) editor).Enabled = false;
		}
	}
}
