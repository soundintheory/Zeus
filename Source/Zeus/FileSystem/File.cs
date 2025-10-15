using Ext.Net;
using Zeus.ContentTypes;
using Zeus.Design.Editors;
using Zeus.Integrity;

namespace Zeus.FileSystem
{
	[ContentType(Description = "A node that represents a file.")]
	[RestrictParents(typeof(Folder))]
	public class File : FileSystemNode
	{
		protected override Icon Icon
		{
			get
			{
				string fileExtension = (!string.IsNullOrEmpty(FileExtension)) ? FileExtension.ToLower() : null;
				switch (fileExtension)
				{
					case ".pdf":
						return Icon.PageWhiteAcrobat;
					case ".zip":
						return Icon.PageWhiteCompressed;
					case ".xls":
					case ".xlsx":
						return Icon.PageWhiteExcel;
					case ".jpg":
					case ".jpeg":
					case ".gif":
					case ".png":
					case ".bmp":
						return Icon.PageWhitePicture;
					case ".ppt":
					case ".pptx":
						return Icon.PageWhitePowerpoint;
					case ".doc":
					case ".docx":
						return Icon.PageWhiteWord;
					default:
						return Icon.PageWhite;
				}
			}
		}

		public string FileExtension
		{
			get { return System.IO.Path.GetExtension(FileName); }
		}

		public override bool IsPage
		{
			get { return true; }
		}

		[FileUploadEditor("File", 100)]
		public virtual byte[] Data
		{
			get { return GetDetail<byte[]>("Data", null); }
			set { SetDetail("Data", value); }
		}

		public string ContentType
		{
			get { return GetDetail<string>("ContentType", null); }
			set { SetDetail("ContentType", value); }
		}

        public int Size
        {
            get { return GetDetail<int>("_Size", default); }
            set { SetDetail("_Size", value); }
        }

        [ContentProperty("Caption", 200)]
		public virtual string Caption
		{
			get { return GetDetail("Caption", string.Empty); }
			set { SetDetail("Caption", value); }
		}

		public string FileName
		{
			get { return GetDetail("FileName", DefaultFileName); }
			set { SetDetail("FileName", value); }
		}
		
		public virtual string DefaultFileName => IsEmpty() ? string.Empty : Name + DefaultFileExtension;

		public virtual string DefaultFileExtension
		{
			get
			{
				switch (ContentType)
				{
					case "image/png":
						return ".png";
                    case "image/gif":
                        return ".gif";
                    case "image/jpeg":
					case "image/jpg":
						return ".jpg";
					default:
						return string.Empty;
				}
			}
		}

		public override bool IsEmpty() => Size == 0;

        public override bool IsVisibleInTree => !IsEmpty() && base.IsVisibleInTree;
	}
}
