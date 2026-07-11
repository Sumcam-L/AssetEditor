using System;
using System.Drawing;

namespace Sce.Atf.Controls.Adaptable;

public interface ICanvasAdapter
{
	Rectangle Bounds { get; set; }

	Rectangle WindowBounds { get; set; }

	event EventHandler BoundsChanged;

	event EventHandler WindowBoundsChanged;
}
