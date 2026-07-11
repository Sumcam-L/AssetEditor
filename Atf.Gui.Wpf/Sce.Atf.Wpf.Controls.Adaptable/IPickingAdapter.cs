using System.Collections.Generic;
using System.Windows;
using Sce.Atf.Controls.Adaptable;

namespace Sce.Atf.Wpf.Controls.Adaptable;

public interface IPickingAdapter
{
	DiagramHitRecord Pick(Point p);

	IEnumerable<object> Pick(Rect region);

	Rect GetBounds(IEnumerable<object> items);
}
