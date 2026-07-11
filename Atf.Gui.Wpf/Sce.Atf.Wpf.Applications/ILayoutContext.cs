using System.Windows;

namespace Sce.Atf.Wpf.Applications;

public interface ILayoutContext
{
	BoundsSpecified GetBounds(object item, out Rect bounds);

	BoundsSpecified CanSetBounds(object item);

	void SetBounds(object item, Rect oldBounds, Rect newBounds, BoundsSpecified specified);
}
