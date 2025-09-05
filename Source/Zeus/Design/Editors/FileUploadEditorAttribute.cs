using System;
using System.IO;
using System.Web;
using System.Web.UI;
using Zeus.BaseLibrary.ExtensionMethods.IO;
using Zeus.BaseLibrary.Web;
using Zeus.ContentTypes;
using Zeus.Web.Handlers;
using Zeus.Web.UI.WebControls;
using Zeus.BaseLibrary.ExtensionMethods;
using File = Zeus.FileSystem.File;
using Zeus.FileSystem.Images;

namespace Zeus.Design.Editors
{
	[AttributeUsage(AttributeTargets.Property)]
	public class FileUploadEditorAttribute : AbstractEditorAttribute
	{
		/// <summary>Initializes a new instance of the EditableTextBoxAttribute class.</summary>
		/// <param name="title">The label displayed to editors</param>
		/// <param name="sortOrder">The order of this editor</param>
		public FileUploadEditorAttribute(string title, int sortOrder)
			: base(title, sortOrder)
		{

		}

		#region Properties

		public string TypeFilterDescription { get; set; }
		public string[] TypeFilter { get; set; }
		public int MaximumFileSize { get; set; }

		#endregion

		public static string GetUploadedFilePath(BaseFileUpload fileUpload)
		{
			string uploadFolder = BaseFileUploadHandler.GetUploadFolder(fileUpload.Identifier);
			return Path.Combine(uploadFolder, HttpUtility.UrlDecode(fileUpload.FileName));
		}

		public override bool UpdateItem(IEditableObject item, Control editor)
		{
            var fileUpload = (BaseFileUpload) editor;
			File file = (File) item;

			bool result = false;
			if (fileUpload.HasDeletedFile)
			{
				file.Data = null;
				result = true;
			}
			else if (fileUpload.HasNewOrChangedFile)
			{
                // Populate File object.
				file.FileName = fileUpload.FileName;
				string uploadedFile = GetUploadedFilePath(fileUpload);
				using (FileStream fs = new FileStream(uploadedFile, FileMode.Open, FileAccess.Read, FileShare.Read))
				{
                    UpdateFileItem(fs, file, fileUpload);
				}

				// Later, we will change the name, if this is a child property.
                file.Title = fileUpload.FileName;
                file.Name = fileUpload.FileName.ToLower();

				// Delete temp folder.
				System.IO.File.Delete(uploadedFile);
				Directory.Delete(BaseFileUploadHandler.GetUploadFolder(fileUpload.Identifier), true);

				result = true;
			}

			return result;
		}

		protected virtual void UpdateFileItem(FileStream fs, File item, BaseFileUpload editor)
		{
            item.Data = fs.ReadAllBytes();
            item.ContentType = item.Data.GetMimeType();
            item.Size = fs.Length;
        }

		/// <summary>Creates a text box editor.</summary>
		/// <param name="container">The container control the tetx box will be placed in.</param>
		/// <returns>A text box control.</returns>
		protected override Control AddEditor(Control container)
		{
			var fileUpload = CreateEditor(container);
			fileUpload.ID = Name;
			if (!string.IsNullOrEmpty(TypeFilterDescription))
				fileUpload.TypeFilterDescription = TypeFilterDescription;
			if (TypeFilter != null)
				fileUpload.TypeFilter = TypeFilter;
			if (MaximumFileSize > 0)
				fileUpload.MaximumFileSize = MaximumFileSize;
			container.Controls.Add(fileUpload);

			return fileUpload;
		}

		protected override void DisableEditor(Control editor)
		{
			((BaseFileUpload)editor).Enabled = false;
		}
        
		protected override void UpdateEditorInternal(IEditableObject item, Control editor)
		{
			var fileUpload = (BaseFileUpload)editor;
			File file = (File)item;
            if (!file.IsEmpty())
            {
                CurrentID = file.ID;
                fileUpload.CurrentFileName = file.FileName;

                if (file is Image image)
                {
                    fileUpload.PreviewUrl = image.GetUrl(BaseFileUpload.THUMBNAIL_WIDTH, BaseFileUpload.THUMBNAIL_HEIGHT, false);
                }
            }
		}

		protected virtual BaseFileUpload CreateEditor(Control container)
		{
			return new DropzoneUpload();
		}
	}
}