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


        [ContentProperty("Link Destination", 20)]
        public virtual ContentItem LinkDestination
        {
            get { return GetDetail("LinkDestination", default(ContentItem)); }
            set { SetDetail("LinkDestination", value); }
        }

        [ContentProperty("Link Destination 2", 20)]
        public virtual ContentItem LinkDestination2
        {
            get { return GetDetail("LinkDestination2", default(ContentItem)); }
            set { SetDetail("LinkDestination2", value); }
        }

        [ContentProperty("Link Destination 3", 20)]
        public virtual ContentItem LinkDestination3
        {
            get { return GetDetail("LinkDestination3", default(ContentItem)); }
            set { SetDetail("LinkDestination3", value); }
        }
    }
}