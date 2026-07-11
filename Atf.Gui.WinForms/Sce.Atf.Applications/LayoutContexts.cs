using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Sce.Atf.Applications;

public static class LayoutContexts
{
	public static BoundsSpecified GetBounds(this ILayoutContext layoutContext, IEnumerable<object> items, out Rectangle bounds)
	{
		BoundsSpecified boundsSpecified = BoundsSpecified.None;
		Rectangle rectangle = default(Rectangle);
		bool flag = true;
		foreach (object item in items)
		{
			Rectangle bounds3;
			BoundsSpecified bounds2 = layoutContext.GetBounds(item, out bounds3);
			if (bounds2 == BoundsSpecified.All)
			{
				if (flag)
				{
					flag = false;
					rectangle = bounds3;
					boundsSpecified = bounds2;
				}
				else
				{
					rectangle = Rectangle.Union(rectangle, bounds3);
					boundsSpecified &= bounds2;
				}
			}
		}
		bounds = rectangle;
		return boundsSpecified;
	}

	public static void Center(this ILayoutContext layoutContext, object item, Point center)
	{
		layoutContext.GetBounds(item, out var bounds);
		Point location = new Point(center.X - bounds.Width / 2, center.Y - bounds.Height / 2);
		layoutContext.SetBounds(item, new Rectangle(location, bounds.Size), BoundsSpecified.Location);
	}

	public static void Center(this ILayoutContext layoutContext, IEnumerable<object> items, Point center)
	{
		GetBounds(layoutContext, items, out var bounds);
		Point point = new Point(center.X - (bounds.Left + bounds.Width / 2), center.Y - (bounds.Top + bounds.Height / 2));
		foreach (object item in items)
		{
			layoutContext.GetBounds(item, out var bounds2);
			Point location = new Point(bounds2.Left + point.X, bounds2.Top + point.Y);
			layoutContext.SetBounds(item, new Rectangle(location, bounds2.Size), BoundsSpecified.Location);
		}
	}
}
