using System;
using System.Drawing.Imaging;

namespace Zeus.BaseLibrary.Drawing.Imaging
{
	public static class ImageCodecInfoHelper
	{
		public static ImageCodecInfo GetEncoder(ImageFormat format)
		{
			var codecs = ImageCodecInfo.GetImageDecoders();
			foreach (var codec in codecs)
			{
				if (codec.FormatID == format.Guid)
				{
					return codec;
				}
			}

			return null;
		}
	}
}