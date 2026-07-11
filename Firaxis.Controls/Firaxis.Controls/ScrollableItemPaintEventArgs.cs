using System;
using System.Drawing;

namespace Firaxis.Controls;

public class ScrollableItemPaintEventArgs : EventArgs
{
	public bool Interacting { get; set; }

	public ScrollableItemStyle Style { get; set; }

	public ScrollableItemState State { get; set; }

	public Rectangle Bounds { get; set; }

	public Graphics Graphics { get; set; }

	public int Level { get; set; }

	public ScrollableItemPaintEventArgs()
		: this(null, Rectangle.Empty, ScrollableItemState.Normal, ScrollableItemStyle.Normal)
	{
	}

	public ScrollableItemPaintEventArgs(Graphics graphics, Rectangle bounds, ScrollableItemState state)
		: this(graphics, bounds, state, ScrollableItemStyle.Normal)
	{
	}

	public ScrollableItemPaintEventArgs(Graphics graphics, Rectangle bounds, ScrollableItemState state, ScrollableItemStyle style)
	{
		Graphics = graphics;
		Bounds = bounds;
		State = state;
		Style = style;
	}
}
