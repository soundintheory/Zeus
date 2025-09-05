using SoundInTheory.DynamicImage.Filters;
using SoundInTheory.DynamicImage.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Zeus.FileSystem.Images
{
    public class ZeusCropFilter : ImageReplacementFilter
    {
        #region Properties

        /// <summary>
        /// Gets or sets the X-coordinate of the rectangular section to crop. Defaults to 0.
        /// </summary>
        public int X
        {
            get => (int)(this["X"] ?? 0);
            set => this["X"] = value;
        }

        /// <summary>
        /// Gets or sets the Y-coordinate of the rectangular section to crop. Defaults to 0.
        /// </summary>
        public int Y
        {
            get => (int)(this["Y"] ?? 0);
            set => this["Y"] = value;
        }

        /// <summary>
        /// Gets or sets the width of the rectangular section to crop. Defaults to 200.
        /// </summary>
        public int Width
        {
            get { return (int)(this["Width"] ?? 200); }
            set
            {
                if (value < 1)
                    throw new ArgumentException("The width of the rectangular section must be greater than zero.", "value");
                this["Width"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the height of the rectangular section to crop. Defaults to 200.
        /// </summary>
        public int Height
        {
            get { return (int)(this["Height"] ?? 200); }
            set
            {
                if (value < 1)
                    throw new ArgumentException("The height of the rectangular section must be greater than zero.", "value");
                this["Height"] = value;
            }
        }

        #endregion

        #region Methods

        protected override bool GetDestinationDimensions(FastBitmap source, out int width, out int height)
        {
            width = Width;
            height = Height;
            return true;
        }

        protected override void ApplyFilter(FastBitmap source, DrawingContext dc, int width, int height)
        {
            var cropRect = System.Windows.Rect.Intersect(new System.Windows.Rect(X, Y, Width, Height), new System.Windows.Rect(0, 0, source.Width, source.Height));

            if (cropRect.Width >= 1d && cropRect.Height >= 1d)
            {
                BitmapSource bitmapSource = new CroppedBitmap(source.InnerBitmap, new System.Windows.Int32Rect((int)cropRect.X, (int)cropRect.Y, (int)cropRect.Width, (int)cropRect.Height));
                dc.DrawImage(bitmapSource, new System.Windows.Rect(X < 0 ? -X : 0, Y < 0 ? -Y : 0, cropRect.Width, cropRect.Height));
            }
        }

        #endregion
    }
}
