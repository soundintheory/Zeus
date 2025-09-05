using System.Web.Mvc;
using SoundInTheory.DynamicImage.Fluent;
using Zeus.FileSystem.Images;
using SoundInTheory.DynamicImage;

namespace Zeus.Templates.Mvc.Html
{
	public static class ImageExtensions
	{
        /// <summary>
        /// Image tag methods
        /// </summary>

        public static ImageTagBuilder ImageTag(this HtmlHelper helper, Image image)
        {
            return ImageTag(helper, image, 0, 0);
        }

        public static ImageTagBuilder ImageTag(this HtmlHelper helper, Image image, int width, int height)
        {
            return ImageTag(helper, image, width, height, true);
        }

        public static ImageTagBuilder ImageTag(this HtmlHelper helper, Image image, int width, int height, bool fill)
        {
            return ImageTag(helper, image, width, height, fill, string.Empty);
        }

        public static ImageTagBuilder ImageTag(this HtmlHelper helper, Image image, int width, int height, bool fill, DynamicImageFormat format)
        {
            return ImageTag(helper, image, width, height, fill, string.Empty, format);
        }

        public static ImageTagBuilder ImageTag(this HtmlHelper helper, Image image, int width, int height, bool fill, string defaultImage)
        {
            return ImageTag(helper, image, width, height, fill, defaultImage, DynamicImageFormat.Jpeg);
        }

        public static ImageTagBuilder ImageTag(this HtmlHelper helper, Image image, int width, int height, bool fill, string defaultImage, DynamicImageFormat format)
        {
            string url = ImageUrl(helper, image, width, height, fill, defaultImage, format);

            return ImageTag(image, url);
        }

        public static ImageTagBuilder ImageTag(this HtmlHelper helper, CroppedImage image, string cropId)
        {
            return ImageTag(helper, image, cropId, 0, 0);
        }

        public static ImageTagBuilder ImageTag(this HtmlHelper helper, CroppedImage image, string cropId, int width, int height)
        {
            return ImageTag(helper, image, cropId, width, height, true);
        }

        public static ImageTagBuilder ImageTag(this HtmlHelper helper, CroppedImage image, string cropId, int width, int height, bool fill)
        {
            return ImageTag(helper, image, cropId, width, height, fill, string.Empty);
        }

        public static ImageTagBuilder ImageTag(this HtmlHelper helper, CroppedImage image, string cropId, int width, int height, bool fill, DynamicImageFormat format)
        {
            return ImageTag(helper, image, cropId, width, height, fill, string.Empty, format);
        }

        public static ImageTagBuilder ImageTag(this HtmlHelper helper, CroppedImage image, string cropId, int width, int height, bool fill, string defaultImage)
        {
            return ImageTag(helper, image, cropId, width, height, fill, defaultImage, DynamicImageFormat.Jpeg);
        }

        public static ImageTagBuilder ImageTag(this HtmlHelper helper, CroppedImage image, string cropId, int width, int height, bool fill, string defaultImage, DynamicImageFormat format)
        {
            string url = ImageUrl(helper, image, cropId, width, height, fill, defaultImage, format);

            return ImageTag(image, url);
        }

        private static ImageTagBuilder ImageTag(Image image, string imageUrl)
        {
            var imageTag = new ImageTagBuilder();

            imageTag.MergeAttribute("src", imageUrl, true);
            imageTag.MergeAttribute("alt", image != null ? image.Caption : string.Empty, true);
            imageTag.MergeAttribute("border", "0", true);
            
            return imageTag;
        }

        /// <summary>
        /// Image URL methods
        /// </summary>

        public static string ImageUrl(this HtmlHelper helper, Image image)
        {
            return ImageUrl(helper, image, 0, 0);
        }

        public static string ImageUrl(this HtmlHelper helper, Image image, int width, int height)
		{
            return ImageUrl(helper, image, width, height, true);
		}

        public static string ImageUrl(this HtmlHelper helper, Image image, int width, int height, bool fill)
        {
            return ImageUrl(helper, image, width, height, fill, string.Empty, DynamicImageFormat.Jpeg);
        }

        public static string ImageUrl(this HtmlHelper helper, Image image, int width, int height, bool fill, DynamicImageFormat format)
        {
            return ImageUrl(helper, image, width, height, fill, string.Empty, format);
        }

        public static string ImageUrl(this HtmlHelper helper, Image image, int width, int height, bool fill, string defaultImage, DynamicImageFormat format)
        {
            // only generate url if image exists
            if (image != null)
            {
                // special code for image without resizing
                if (width == 0 && height == 0)
                {
                    return new CompositionBuilder()
                        .WithLayer(LayerBuilder.Image.SourceImage(image))
                        .Url;
                }
                else
                {
                    if (image is CroppedImage croppedImage)
                    {
                        return croppedImage.GetCropUrl(width, height, fill, format);
                    }

                    return image.GetUrl(width, height, fill, format);
                }
            }

            return defaultImage;
        }

        public static string ImageUrl(this HtmlHelper helper, CroppedImage image, string cropId)
        {
            return ImageUrl(helper, image, cropId, 0, 0);
        }

        public static string ImageUrl(this HtmlHelper helper, CroppedImage image, string cropId, int width, int height)
        {
            return ImageUrl(helper, image, cropId, width, height, true);
        }

        public static string ImageUrl(this HtmlHelper helper, CroppedImage image, string cropId, int width, int height, bool fill)
        {
            return ImageUrl(helper, image, cropId, width, height, fill, string.Empty, DynamicImageFormat.Jpeg);
        }

        public static string ImageUrl(this HtmlHelper helper, CroppedImage image, string cropId, int width, int height, bool fill, DynamicImageFormat format)
        {
            return ImageUrl(helper, image, cropId, width, height, fill, string.Empty, format);
        }

        /// <summary>
        /// Cropped image URL methods
        /// </summary>

        public static string ImageUrl(this HtmlHelper helper, CroppedImage image, string cropId, int width, int height, bool fill, string defaultImage, DynamicImageFormat format)
        {
            // only generate url if image exists
            if (image != null)
            {
                return image.GetCropUrl(cropId, width, height, fill, format);
            }

            return defaultImage;
        }

    }
}