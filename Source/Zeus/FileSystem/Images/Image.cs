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
using Zeus.Web.Caching;

namespace Zeus.FileSystem.Images
{
	[ContentType]
	public class Image : File
	{
        private static MemoryCacheStore<string, ImageUrl> _urlCache = new MemoryCacheStore<string, ImageUrl>();

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

        public static Image FromStream(Stream stream, string filename) => FromStream<Image>(stream, filename);

        public static T FromStream<T>(Stream stream, string filename) where T : Image, new()
        {
			byte[] fileBytes = stream.ReadAllBytes();
			return new T
			{
				ContentType = fileBytes.GetMimeType(),
				Data = fileBytes,
				Name = filename,
				Size = Convert.ToInt32(stream.Length)
			};
		}

        public string GetUrl(int width, int height, bool fill, DynamicImageFormat format)
		{
            string cacheKey = "ZeusImage_" + this.ID + "_" + width + "_" + height + "_" + fill.ToString();

            if (_urlCache.TryGet(cacheKey, out var cacheEntry) && cacheEntry.LastUpdated >= this.Updated)
            {
                return cacheEntry.Url;
            }

            return _urlCache.AddOrUpdate(
                cacheKey,
                (key) => GetUrlInternal(width, height, fill, format),
                (key, currentValue) =>
                {
                    if (currentValue.LastUpdated >= this.Updated)
                    {
                        return currentValue;
                    }
                    return GetUrlInternal(width, height, fill, format);
                }
            ).Url;
		}

        public string GetUrl(int width, int height, bool fill)
		{
            return GetUrl(width, height, fill, DynamicImageFormat.Jpeg);
		}

		public string GetUrl(int width, int height)
		{
            return GetUrl(width, height, true, DynamicImageFormat.Jpeg);
		}

        protected ImageUrl GetUrlInternal(int width, int height, bool fill, DynamicImageFormat format)
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

            string url = ImageUrlGenerator.GetImageUrl(image);

            return new ImageUrl { Url = url, LastUpdated = Updated };
        }

        public class ImageUrl
        {
            public string Url { get; set; }

            public DateTime LastUpdated { get; set; }
        }
	}
}