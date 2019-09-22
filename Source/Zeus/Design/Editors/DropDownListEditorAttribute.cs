using System.Web.UI;
using System.Web.UI.WebControls;

namespace Zeus.Design.Editors
{
	[System.AttributeUsage(System.AttributeTargets.All, AllowMultiple = false)]
	public abstract class DropDownListEditorAttribute : ListEditorAttribute
	{
		#region Constructors

		protected DropDownListEditorAttribute()
		{
		}

		protected DropDownListEditorAttribute(string title, int sortOrder)
			: base(title, sortOrder)
		{
		}

		#endregion

		protected override ListControl CreateEditor()
		{
			return new DropDownList();
		}
	}
}