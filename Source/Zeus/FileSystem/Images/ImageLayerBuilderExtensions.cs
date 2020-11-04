using SoundInTheory.DynamicImage.Fluent;

namespace Zeus.FileSystem.Images
{
	public static class ImageLayerBuilderExtensions
	{
		public static ImageLayerBuilder SourceImage(this ImageLayerBuilder builder, Image image)
		{
			builder.Source = new ZeusImageSource(image);
			return builder;
		}
	}
}