using SoundInTheory.DynamicImage.Filters;
using System;
using System.IO;
using System.IO.Pipes;
using System.Web.UI;
using Zeus.BaseLibrary.ExtensionMethods;
using Zeus.BaseLibrary.ExtensionMethods.IO;
using Zeus.ContentTypes;
using Zeus.FileSystem;
using Zeus.FileSystem.Images;
using Zeus.Web.Handlers;
using Zeus.Web.UI;
using Zeus.Web.UI.WebControls;
using File = Zeus.FileSystem.File;

namespace Zeus.Design.Editors
{
	[AttributeUsage(AttributeTargets.Property)]
	public class ImageUploadEditorAttribute : FileUploadEditorAttribute
	{
		public int MinWidth { get; set; }

		public int MinHeight { get; set; }

		/// <summary>Initializes a new instance of the ImageUploadEditorAttribute class.</summary>
		/// <param name="title">The label displayed to editors</param>
		/// <param name="sortOrder">The order of this editor</param>
		public ImageUploadEditorAttribute(string title, int sortOrder)
			: base(title, sortOrder)
		{
		}

		protected override BaseFileUpload CreateEditor(Control container)
		{
			return new DropzoneImageUpload
			{
				MinWidth = MinWidth,
				MinHeight = MinHeight
			};
		}

        public override Control AddTo(Control container)
        {
            var imageView = container.FindParent<ImageEditView>();

            if (imageView != null)
            {
				if (!imageView.UseFieldset && !string.IsNullOrEmpty(imageView.Title))
				{
                    Title = imageView.Title;
                }
                
				if (!string.IsNullOrEmpty(imageView.Description))
				{
					Description = imageView.Description;
				}
            }

            return base.AddTo(container);
        }

        public override bool UpdateItem(IEditableObject item, Control editor)
        {
			var updated = false;

			// Even if the file has stayed the same, make sure the object has source dimensions
            if (item is Image image && editor is BaseFileUpload fileUpload && !fileUpload.HasDeletedFile)
            {
				updated = image.EnsureSourceDimensions();
            }

            var result = base.UpdateItem(item, editor);

			return result || updated;
        }

        protected override void UpdateFileItem(FileStream fs, File item, BaseFileUpload editor)
        {
			if (item is Image imageItem)
			{
                using (var image = System.Drawing.Image.FromStream(fs, false, false))
                {
                    imageItem.SourceWidth = image.Width;
                    imageItem.SourceHeight = image.Height;
                }
            }

			fs.Seek(0, SeekOrigin.Begin);
			base.UpdateFileItem(fs, item, editor);
        }
    }
}