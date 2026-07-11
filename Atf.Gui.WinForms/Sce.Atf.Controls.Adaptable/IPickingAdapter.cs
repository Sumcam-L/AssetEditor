using System.Collections.Generic;
using System.Drawing;

namespace Sce.Atf.Controls.Adaptable;

public interface IPickingAdapter
{
	DiagramHitRecord Pick(Point p);

	IEnumerable<object> Pick(Region region);

	Rectangle GetBounds(IEnumerable<object> items);
}
