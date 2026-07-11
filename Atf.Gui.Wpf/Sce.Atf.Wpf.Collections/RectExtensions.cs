using System.Windows;

namespace Sce.Atf.Wpf.Collections;

internal static class RectExtensions
{
	public static Point GetCenter(this Rect rect)
	{
		return new Point(rect.X + rect.Width / 2.0, rect.Y + rect.Height / 2.0);
	}

	public static bool IsDefined(this Rect rect)
	{
		return rect.Width >= 0.0 && rect.Height >= 0.0 && rect.Top < double.PositiveInfinity && rect.Left < double.PositiveInfinity && (rect.Top > double.NegativeInfinity || rect.Height == double.PositiveInfinity) && (rect.Left > double.NegativeInfinity || rect.Width == double.PositiveInfinity);
	}

	public static bool Intersects(this Rect self, Rect rect)
	{
		return self.IsEmpty || rect.IsEmpty || ((self.Width == double.PositiveInfinity || self.Right >= rect.Left) && (rect.Width == double.PositiveInfinity || rect.Right >= self.Left) && (self.Height == double.PositiveInfinity || self.Bottom >= rect.Top) && (rect.Height == double.PositiveInfinity || rect.Bottom >= self.Top));
	}
}
