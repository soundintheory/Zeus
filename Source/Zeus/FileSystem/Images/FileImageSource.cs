using System;
using SoundInTheory.DynamicImage.Sources;

namespace Zeus.FileSystem.Images
{
    public class ZeusFileImageSource : FileImageSource
    {
        public DateTime Updated
        {
            get { return (DateTime)(this["Updated"] ?? default(DateTime)); }
            set { this["Updated"] = value; }
        }
    }
}
