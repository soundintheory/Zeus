﻿using System;
using System.IO;
using System.Web.UI;
using Zeus.BaseLibrary.ExtensionMethods;
using Zeus.BaseLibrary.ExtensionMethods.IO;
using Zeus.BaseLibrary.Web;
using Zeus.ContentProperties;
using Zeus.FileSystem;
using Zeus.Web.Handlers;
using Zeus.Web.UI.WebControls;
using File=Zeus.FileSystem.File;

namespace Zeus.Design.Editors
{
	public class MultiFileUploadEditorAttribute : BaseDetailCollectionEditorAttribute
	{
		public MultiFileUploadEditorAttribute(string title, int sortOrder)
			: base(title, sortOrder)
		{
			
		}

		protected override void CreateOrUpdateDetailCollectionItem(ContentItem contentItem, PropertyData existingDetail, Control editor, out object newDetail)
		{
			var fileEditor = (BaseFileUpload)editor;
			var existingFileProperty = existingDetail as LinkProperty;
			if (fileEditor.HasNewOrChangedFile)
			{
				// Add new file.
				File newFile = null;
				if (existingFileProperty != null)
				{
					newFile = existingFileProperty.LinkedItem as File;
				}

				if (newFile == null)
				{
					newFile = CreateNewItem();
					newFile.Name = Name + Guid.NewGuid();
					newFile.AddTo(contentItem);
				}

				// Populate FileData object.
				newFile.FileName = fileEditor.FileName;
				var uploadFolder = BaseFileUploadHandler.GetUploadFolder(fileEditor.Identifier);
				var uploadedFile = Path.Combine(uploadFolder, fileEditor.FileName);
				using (var fs = new FileStream(uploadedFile, FileMode.Open))
				{
					newFile.Data = fs.ReadAllBytes();
                    newFile.ContentType = newFile.Data.GetMimeType();
					newFile.Size = fs.Length;
				}

				// Delete temp folder.
				System.IO.File.Delete(uploadedFile);
				Directory.Delete(uploadFolder);

				newDetail = newFile;

				if (existingFileProperty != null)
				{
					HandleUpdatedFile(newFile);
				}
			}
			else
			{
				newDetail = null;
			}
		}

		protected virtual void HandleUpdatedFile(File file)
		{
			
		}

		protected virtual File CreateNewItem()
		{
			return new File();
		}

		protected override BaseDetailCollectionEditor CreateEditor()
		{
			return new MultiFileUploadEditor();
		}
	}
}