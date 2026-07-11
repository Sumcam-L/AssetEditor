using System;
using System.Windows;

namespace Sce.Atf.Wpf.Controls.Adaptable;

public interface ICanvasAdapter
{
	Rect Bounds { get; set; }

	Rect WindowBounds { get; set; }

	event EventHandler BoundsChanged;

	event EventHandler WindowBoundsChanged;
}
