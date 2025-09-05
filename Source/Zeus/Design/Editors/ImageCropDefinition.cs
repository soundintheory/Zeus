using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zeus.Design.Editors
{
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public class ImageCropDefinition
    {
        public ImageCropDefinition(ImageCropAttribute attribute)
        {
            Id = attribute.Id;
            Title = attribute.Title;
            Description = attribute.Description;
            AspectRatio = attribute.AspectRatio;
            MinWidth = attribute.MinWidth;
            MinHeight = attribute.MinHeight;
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
