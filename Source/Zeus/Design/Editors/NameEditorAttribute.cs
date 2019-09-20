using System;
using System.Web.UI;
using Zeus.ContentTypes;
using Zeus.Web.UI.WebControls;

[assembly: WebResource("NameEditorAttribute.js", "text/javascript")]

namespace Zeus.Design.Editors
{
	public class NameEditorAttribute : AbstractEditorAttribute
	{
		public NameEditorAttribute(string title, int sortOrder)
			: base(title, "Name", sortOrder)
		{
			Required = true;
		}

		protected override Control AddEditor(Control container)
		{
			var nameEditor = new NameEditor { ID = "txtNameEditor" };
			if (Required)
				nameEditor.CssClass += " required";
			container.Controls.Add(nameEditor);
			return nameEditor;
		}

		protected override void DisableEditor(Control editor)
		{
			((NameEditor) editor).Enabled = false;
		}

		public override bool UpdateItem(IEditableObject item, Control editor)
		{
            object editorValue = ((NameEditor)editor).Text;
            var itemValue = item[Name];
            if (!AreEqual(editorValue, itemValue))
            {
                item[Name] = ((NameEditor)editor).Text;
                return true;
            }
            return false;			
		}

		protected override void UpdateEditorInternal(IEditableObject item, Control editor)
		{
			var contentItem = (ContentItem) item;
			var ne = (NameEditor) editor;
			ne.Text = contentItem.Name;
			ne.Prefix = "/";
			ne.Suffix = contentItem.Extension;
			try
			{
				if (Context.UrlParser.StartPage == item || contentItem.GetParent() == null)
				{
					ne.Prefix = string.Empty;
					ne.Suffix = string.Empty;
				}
				else if (Context.UrlParser.StartPage != contentItem.GetParent())
				{
					var parentUrl = contentItem.GetParent().GetUrl(contentItem.Language);
					if (!parentUrl.Contains("?"))
					{
						var prefix = parentUrl;
						if (!string.IsNullOrEmpty(contentItem.Extension))
						{
							var aspxIndex = parentUrl.IndexOf(contentItem.Extension, StringComparison.InvariantCultureIgnoreCase);
							prefix = parentUrl.Substring(0, aspxIndex);
						}
						prefix += "/";
						if (prefix.Length > 60)
							prefix = prefix.Substring(0, 50) + ".../";
						ne.Prefix = prefix;
					}
				}
			}
			catch (Exception)
			{
			}
		}
	}
}