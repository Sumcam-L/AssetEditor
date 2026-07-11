using System.Collections.Generic;
using System.Windows;

namespace Sce.Atf.Wpf.Applications;

public static class LayoutContexts
{
	public static BoundsSpecified GetBounds(this ILayoutContext layoutContext, IEnumerable<object> items, out Rect bounds)
	{
		BoundsSpecified boundsSpecified = BoundsSpecified.None;
		Rect rect = Rect.Empty;
		foreach (object item in items)
		{
			Rect bounds3;
			BoundsSpecified bounds2 = layoutContext.GetBounds(item, out bounds3);
			if (bounds2 == BoundsSpecified.All)
			{
				if (rect.IsEmpty)
				{
					rect = bounds3;
					boundsSpecified = bounds2;
				}
				else
				{
					rect = Rect.Union(rect, bounds3);
					boundsSpecified &= bounds2;
				}
			}
		}
		bounds = rect;
		return boundsSpecified;
	}

	public static void Center(this ILayoutContext layoutContext, object item, Point center)
	{
		layoutContext.GetBounds(item, out var bounds);
		layoutContext.SetBounds(newBounds: new Rect(new Point(center.X - bounds.Width / 2.0, center.Y - bounds.Height / 2.0), bounds.Size), item: item, oldBounds: bounds, specified: BoundsSpecified.Location);
	}

	public static void Center(this ILayoutContext layoutContext, IEnumerable<object> items, Point center)
	{
		GetBounds(layoutContext, items, out var bounds);
		Point point = new Point(center.X - (bounds.Left + bounds.Width / 2.0), center.Y - (bounds.Top + bounds.Height / 2.0));
		foreach (object item in items)
		{
			layoutContext.GetBounds(item, out var bounds2);
			layoutContext.SetBounds(newBounds: new Rect(new Point(bounds2.Left + point.X, bounds2.Top + point.Y), bounds2.Size), item: item, oldBounds: bounds2, specified: BoundsSpecified.Location);
		}
	}
}
