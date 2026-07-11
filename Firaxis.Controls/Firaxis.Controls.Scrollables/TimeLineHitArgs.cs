using System;
using System.Drawing;

namespace Firaxis.Controls.Scrollables;

public class TimeLineHitArgs : EventArgs
{
	public Rectangle Bounds { get; set; }

	public TimeLineControl TimeLine { get; private set; }

	public TimeLineHitArgs(TimeLineControl timeLine, Rectangle bounds)
	{
		Bounds = bounds;
		TimeLine = timeLine;
	}
}
