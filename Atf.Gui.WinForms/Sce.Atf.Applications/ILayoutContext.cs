using System.Drawing;
using System.Windows.Forms;

namespace Sce.Atf.Applications;

public interface ILayoutContext
{
	BoundsSpecified GetBounds(object item, out Rectangle bounds);

	BoundsSpecified CanSetBounds(object item);

	void SetBounds(object item, Rectangle bounds, BoundsSpecified specified);
}
