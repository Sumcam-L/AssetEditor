using System;

namespace Firaxis.Controls;

public class ScrollableItemEventArgs : EventArgs
{
	public IScrollableItem Item { get; private set; }

	public int X { get; set; }

	public int Y { get; set; }

	public ScrollableItemEventArgs(IScrollableItem item, int x, int y)
	{
		Item = item;
		X = x;
		Y = y;
	}
}
