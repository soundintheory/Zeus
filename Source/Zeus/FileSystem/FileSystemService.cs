using System;
using Zeus.ContentTypes;

namespace Zeus.FileSystem
{
	public class FileSystemService : IFileSystemService
	{
		#region Fields

		private readonly IContentTypeManager _contentTypeManager;

		#endregion

		#region Constructor

		public FileSystemService(IContentTypeManager contentTypeManager)
		{
			_contentTypeManager = contentTypeManager;
		}

		#endregion

		public Folder EnsureFolder(Folder parentFolder, string folderName)
		{
			var currentFolder = parentFolder;

			var folderNameParts = folderName.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
			foreach (var folderNamePart in folderNameParts)
			{
				var existingFolder = currentFolder.GetChild(folderNamePart) as Folder;
				if (existingFolder != null)
				{
					currentFolder = existingFolder;
					continue;
				}

				var newFolder = _contentTypeManager.CreateInstance<Folder>(currentFolder);
				newFolder.Name = folderNamePart;
				newFolder.AddTo(currentFolder);

				currentFolder = newFolder;
			}

			return currentFolder;
		}
	}
}