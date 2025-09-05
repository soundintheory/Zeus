using System;
using Zeus.Web.UI.WebControls;
using System.Web.UI;
using Zeus.FileSystem.Images;
using Zeus.FileSystem;
using Zeus.ContentTypes;
using System.IO;
using Zeus.Web.UI;

namespace Zeus.Design.Editors
{
	[AttributeUsage(AttributeTargets.Property)]
	public class CroppedImageUploadEditorAttribute : ImageUploadEditorAttribute
	{
		/// <summary>Initializes a new instance of the ImageUploadEditorAttribute class.</summary>
		/// <param name="title">The label displayed to editors</param>
		/// <param name="sortOrder">The order of this editor</param>
        public CroppedImageUploadEditorAttribute(string title, int sortOrder)
			: base(title, sortOrder)
		{

		}

		protected override BaseFileUpload CreateEditor(Control container)
		{
			var imageView = container.FindParent<ImageEditView>();
			var uploader = new DropzoneImageUpload
			{
				MinWidth = MinWidth,
				MinHeight = MinHeight
			};

			// If we're in a child image editor, take the crop settings from the property definition
			if (imageView != null)
			{
				uploader.MinWidth = imageView.MinWidth;
				uploader.MinHeight = imageView.MinHeight;
				uploader.AllowCropping = imageView.AllowCropping;
				uploader.Crops = imageView.Crops;
            }

            return uploader;
		}

        protected override void UpdateEditorInternal(IEditableObject item, Control editor)
        {
            base.UpdateEditorInternal(item, editor);

			if (item is CroppedImage croppedImage && editor is DropzoneImageUpload imageUpload)
			{
				croppedImage.EnsureSourceDimensions();

				imageUpload.CurrentCropData = croppedImage.GetAllCropData();
				imageUpload.FullSizeUrl = croppedImage.GetUrl(800, 600, false);
				imageUpload.SourceWidth = croppedImage.SourceWidth;
				imageUpload.SourceHeight = croppedImage.SourceHeight;
			}
        }

        public override bool UpdateItem(IEditableObject item, Control editor)
        {
			var updated = false;

            // Update the crop data if it has been changed
            if (item is CroppedImage image && editor is DropzoneImageUpload imageUpload && !imageUpload.HasDeletedFile && !string.IsNullOrEmpty(imageUpload.UpdatedCropData))
            {
                image.RawCropData = imageUpload.UpdatedCropData;
				updated = true;
            }

            var result = base.UpdateItem(item, editor);

			return result || updated;
        }
    }
}