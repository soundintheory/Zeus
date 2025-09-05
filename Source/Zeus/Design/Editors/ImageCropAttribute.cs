using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zeus.FileSystem.Images;

namespace Zeus.Design.Editors
{
    public class ImageCropAttribute : AbstractEditorConfigAttribute
    {
        public ImageCropAttribute() : this(CroppedImage.DefaultCropId, 0)
        {
        }

        public ImageCropAttribute(double aspectRatio) : this(CroppedImage.DefaultCropId, aspectRatio)
        {
        }

        public ImageCropAttribute(string id) : this(id, 0)
        {
        }

        public ImageCropAttribute(string id, double aspectRatio)
        {
            Id = id;
            AspectRatio = aspectRatio;
        }

        /// <summary>
        /// The key / identifier for the crop
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The human readable title of the crop (in admin)
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Description of the crop (optional)
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Fix the aspect ratio of the crop
        /// </summary>
        public double AspectRatio { get; set; }

        /// <summary>
        /// The minimum width of the crop area in pixels
        /// </summary>
        public int MinWidth { get; set; }

        /// <summary>
        /// The minimum height of the crop area in pixels
        /// </summary>
        public int MinHeight { get; set; }
    }
}
