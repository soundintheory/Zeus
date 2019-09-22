using System.Web.UI;
using System.Web.UI.WebControls;
using Zeus.ContentProperties;
using Zeus.Web.UI.WebControls;

namespace Zeus.Design.Editors
{
	[System.AttributeUsage(System.AttributeTargets.All, AllowMultiple = false)]
	public class StringCollectionEditorAttribute : BaseDetailCollectionEditorAttribute
	{
		public StringCollectionEditorAttribute(string title, int sortOrder)
			: base(title, sortOrder)
		{
		}

		protected override BaseDetailCollectionEditor CreateEditor()
		{
			return new StringCollectionEditor { ID = Name };
		}

		protected override void CreateOrUpdateDetailCollectionItem(ContentItem contentItem, PropertyData existingDetail, Control editor, out object newDetail)
		{
			newDetail = ((TextBox) editor).Text;
		}
	}
}