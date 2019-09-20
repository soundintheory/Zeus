using Zeus.Design.Editors;
using Zeus.Templates.ContentTypes;

namespace Zeus.Examples.MinimalMvcExample.ContentTypes
{
	[ContentType("My Little Type")]
	public class MyLittleType : BaseContentItem
	{
		[ContentProperty("Test String", 10, Shared = false)]
		public virtual string TestString
		{
			get { return GetDetail("TestString", string.Empty); }
			set { SetDetail("TestString", value); }
		}

        /*
		[XhtmlStringContentProperty("Test Rich String", 35)]
		public virtual string TestRichString
		{
			get { return GetDetail("TestRichString", string.Empty); }
			set { SetDetail("TestRichString", value); }
		}
         */

		[ContentProperty("Multi Line Textbox", 35)]
		[TextAreaEditor(Height = 200, Width = 500)]
		public virtual string MultiTextBox
		{
			get { return GetDetail("MultiTextBox", string.Empty); }
			set { SetDetail("MultiTextBox", value); }
		}
	}
}