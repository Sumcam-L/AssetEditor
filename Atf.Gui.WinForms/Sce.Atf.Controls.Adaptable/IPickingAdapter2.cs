using System.Collections.Generic;
using System.Drawing;

namespace Sce.Atf.Controls.Adaptable;

public interface IPickingAdapter2
{
	DiagramHitRecord Pick(Point p);

	IEnumerable<object> Pick(Rectangle bounds);

	Rectangle GetBounds(IEnumerable<object> items);
}
