using System.Drawing;

namespace WeifenLuo.WinFormsUI.Docking;

public static class LayoutUtils
{
	public static bool IsZeroWidthOrHeight(Rectangle rectangle)
	{
		if (rectangle.Width != 0)
		{
			return rectangle.Height == 0;
		}
		return true;
	}
}
