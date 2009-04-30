﻿using System.Web;
using Isis.Web.UI;
using Zeus.FileSystem.Details;
using Zeus.Integrity;

namespace Zeus.FileSystem
{
	[ContentType(Description = "A node that represents a file.")]
	[RestrictParents(typeof(IFileSystemContainer), typeof(Folder))]
	public class File : FileSystemNode
	{
		public override string IconUrl
		{
			get
			{
				string resourceName = "Zeus.Web.Resources.Icons.";
				string fileExtension = (!string.IsNullOrEmpty(FileExtension)) ? FileExtension.ToLower() : null;
				switch (fileExtension)
				{
					case ".pdf":
						resourceName += "page_white_acrobat.png";
						break;
					case ".zip":
						resourceName += "page_white_compressed.png";
						break;
					case ".xls":
					case ".xlsx":
						resourceName += "page_white_excel.png";
						break;
					case ".jpg":
					case ".jpeg":
					case ".gif":
					case ".png":
					case ".bmp":
						resourceName += "page_white_picture.png";
						break;
					case ".ppt":
					case ".pptx":
						resourceName += "page_white_powerpoint.png";
						break;
					case ".doc":
					case ".docx":
						resourceName += "page_white_word.png";
						break;
					default:
						resourceName += "page_white.png";
						break;
				}
				return WebResourceUtility.GetUrl(typeof(File), resourceName);
			}
		}

		public override string Url
		{
			get { return "~/File.axd?Path=" + HttpUtility.UrlEncode(Path); }
		}

		[UploadEditor(IsLocallyUnique = true)]
		public override string Name
		{
			get { return base.Name; }
			set { base.Name = value; }
		}

		public string FileExtension
		{
			get { return System.IO.Path.GetExtension(Name); }
		}

		public byte[] Data
		{
			get { return GetDetail<byte[]>("Data", null); }
			set { SetDetail("Data", value); }
		}

		public string ContentType
		{
			get { return GetDetail<string>("ContentType", null); }
			set { SetDetail("ContentType", value); }
		}

		public long? Size
		{
			get { return GetDetail<long?>("Size", null); }
			set { SetDetail("Size", value); }
		}
	}
}
