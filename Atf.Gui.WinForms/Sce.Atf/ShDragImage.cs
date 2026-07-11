using System;
using System.Drawing;

namespace Sce.Atf;

internal struct ShDragImage
{
	public Size sizeDragImage;

	public Point ptOffset;

	public IntPtr hbmpDragImage;

	public int crColorKey;
}
