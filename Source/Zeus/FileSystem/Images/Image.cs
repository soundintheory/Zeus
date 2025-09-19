using System.IO;
using SoundInTheory.DynamicImage.Caching;
using SoundInTheory.DynamicImage.Layers;
using Zeus.BaseLibrary.ExtensionMethods;
using Zeus.BaseLibrary.ExtensionMethods.IO;
using Zeus.BaseLibrary.Web;
using Zeus.Design.Editors;
using SoundInTheory.DynamicImage;
using SoundInTheory.DynamicImage.Fluent;
using SoundInTheory.DynamicImage.Filters;
using System.Drawing;
using System;

namespace Zeus.FileSystem.Images
{
	[ContentType]
	public class Image : File
	{
		public Image()
		{
			base.Visible = false;
		}

        public int SourceWidth
        {
            get { return GetDetail("SourceWidth", default(int)); }
            set { SetDetail("SourceWidth", value); }
        }

        public int SourceHeight
        {
            get { return GetDetail("SourceHeight", default(int)); }
            set { SetDetail("SourceHeight", value); }
        }

        [ImageUploadEditor("Image", 100)]
		public override byte[] Data
		{
			get { return base.Data; }
			set { base.Data = value; }
		}

		public static Image FromStream(Stream stream, string filename)
		{
			byte[] fileBytes = stream.ReadAllBytes();
			var image = new Image
			{
				ContentType = fileBytes.GetMimeType(),
				Data = fileBytes,
				Name = Utility.GetSafeName(filename),
				Size = stream.Length,
                FileName = filename
			};
            image.EnsureSourceDimensions();
            return image;
		}

        public static T FromStream<T>(Stream stream, string filename) where T : Image, new()
        {
            byte[] fileBytes = stream.ReadAllBytes();
            var image = new T
            {
                ContentType = fileBytes.GetMimeType(),
                Data = fileBytes,
                Name = Utility.GetSafeName(filename),
                Size = stream.Length,
                FileName = filename
            };
            image.EnsureSourceDimensions();
            return image;
        }

        public virtual string GetUrl(int width, int height, bool fill)
		{
            return GetUrl(width, height, fill, DynamicImageFormat.Jpeg);
		}

		public virtual string GetUrl(int width, int height)
		{
            return GetUrl(width, height, true, DynamicImageFormat.Jpeg);
		}

        public virtual string GetUrl(int width, int height, bool fill, DynamicImageFormat format)
        {
            return Context.Current.Cache.GetOrAdd(
                ID,
                $"ZeusImage.{ID}.{width}.{height}.{fill}",
                () => GetUrlInternal(width, height, fill, format)
            );
        }

        protected virtual string GetUrlInternal(int width, int height, bool fill, DynamicImageFormat format)
        {
            var image = new Composition
            {
                ImageFormat = format
            };

            var imageLayer = new ImageLayer
            {
                Source = new ZeusImageSource(this)
            };

            var resizeFilter = new ResizeFilter
            {
                Mode = fill ? ResizeMode.UniformFill : ResizeMode.Uniform,
                Width = Unit.Pixel(width),
                Height = Unit.Pixel(height)
            };

            imageLayer.Filters.Add(resizeFilter);
            image.Layers.Add(imageLayer);

            var url = ImageUrlGenerator.GetImageUrl(image);

            return url;
        }

        public bool EnsureSourceDimensions()
        {
            if (!IsEmpty() && (SourceWidth == 0 || SourceHeight == 0))
            {
                System.Drawing.Image image = System.Drawing.Image.FromStream(new MemoryStream(Data), false, false);
                SourceWidth = image.Width;
                SourceHeight = image.Height;

                return true;
            }

            return false;
        }
	}
}