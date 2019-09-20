using System;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Zeus.BaseLibrary.ExtensionMethods.Web.UI;
using Zeus.ContentProperties;
using Zeus.ContentTypes;
using Zeus.Design.Displayers;

namespace Zeus.Admin
{
	[ActionPluginGroup("ViewPreview", 30)]
	public partial class View : PreviewFrameAdminPage
	{
		protected override void OnInit(EventArgs e)
		{
			Title = "View \"" + SelectedItem.Title + "\"";

			// Get selected property from content item.
			var contentType = Zeus.Context.Current.ContentTypes[SelectedItem.GetType()];
			foreach (var property in contentType.Properties)
			{
				var plcDisplay = new PlaceHolder();
				var panel = new Panel { CssClass = "editDetail" };
				var label = new HtmlGenericControl("label");
				label.Attributes["class"] = "editorLabel";
				label.InnerText = property.Name;
				panel.Controls.Add(label);
				plcDisplay.Controls.Add(panel);
				plcDisplayers.Controls.Add(plcDisplay);

				var displayer = contentType.GetDisplayer(property.Name);
				if (displayer != null)
				{
					//displayer.AddTo(this, contentItem, this.PropertyName);
					displayer.InstantiateIn(panel);
					displayer.SetValue(panel, SelectedItem, property.Name);
				}
				else
				{
					panel.Controls.Add(new LiteralControl("{No displayer}"));
				}
				panel.Controls.Add(new LiteralControl("&nbsp;"));
			}

			base.OnInit(e);
		}

		protected override void OnPreRender(EventArgs e)
		{
			Page.ClientScript.RegisterCssResource(typeof(View), "Zeus.Admin.Assets.Css.edit.css");
			base.OnPreRender(e);
		}
	}
}
