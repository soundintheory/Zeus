using System;
using System.Drawing;

namespace Zeus.BaseLibrary.Drawing
{
	public static class RectangleHelper
	{
		public static Rectangle FromPoints(Point[] points)
		{
			var result = new Rectangle(points[0], Size.Empty);
			for (int i = 1, length = points.Length; i < length; ++i)
				result = Rectangle.Union(result, new Rectangle(points[i], Size.Empty));
			return result;
		}

		public static RectangleF FromPoints(PointF[] points)
		{
			var result = new RectangleF(points[0], Size.Empty);
			for (int i = 1, length = points.Length; i < length; ++i)
				result = RectangleF.Union(result, new RectangleF(points[i], Size.Empty));
			return result;
		}
	}
}