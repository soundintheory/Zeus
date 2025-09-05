using Zeus;
using Zeus.Web;
using Zeus.Integrity;
using Zeus.Design.Editors;
using System.Collections.Generic;
using Zeus.Web.UI;
using Zeus.ContentTypes;
using Zeus.Templates.ContentTypes;
using Zeus.FileSystem.Images;
using Zeus.FileSystem;

namespace Zeus.Examples.MinimalMvcExample.ContentTypes
{
	[ContentType("Page", IgnoreSEOAssets=true)]
	[AllowedChildren(typeof(Zeus.FileSystem.File))]
	[Panel("NewContainer", "All My Types Go in Here!", 100)]
    public class MyPage : BasePage
	{
		[LinkedItemDropDownListEditor("Product", 100)]
		public virtual ContentItem Product
		{
			get { return GetDetail<ContentItem>("Product", null); }
			set { SetDetail("Product", value); }
		}

		[ImageEditor("Image", 110)]
		public virtual HiddenImage Image
		{
			get { return GetChild<HiddenImage>("Image"); }
			set { SetChild(value, "Image"); }
		}

		[ImageEditor("Cropped Image", 120)]
        public virtual HiddenCroppedImage AnotherImage
		{
			get { return GetChild<HiddenCroppedImage>("AnotherImage"); }
			set { SetChild(value, "AnotherImage"); }
		}

        [ImageEditor("Cropped Image With More Crops", 130)]
        [ImageCrop]
        [ImageCrop("Thumbnail", 300d / 200d, MinWidth = 300, MinHeight = 200)]
        public virtual HiddenCroppedImage AnotherImage2
        {
            get { return GetChild<HiddenCroppedImage>("AnotherImage2"); }
            set { SetChild(value, "AnotherImage2"); }
        }

        [ChildEditor("File", 140)]
        public virtual File File
        {
            get { return GetChild<File>("File"); }
            set { SetChild(value, "File"); }
        }

        [ChildrenEditor("Test Child Editors", 150, TypeFilter = typeof(MyLittleType), ContainerName = "NewContainer")]
		public virtual IEnumerable<MyLittleType> ListFilters
		{
			get { return GetChildren<MyLittleType>(); }
		}

        public override bool AllowParamsOnIndex
        {
            get
            {
                return true;
            }
        }

	}
}
