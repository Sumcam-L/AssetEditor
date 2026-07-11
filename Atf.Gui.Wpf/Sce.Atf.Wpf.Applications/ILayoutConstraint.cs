using System.Windows;

namespace Sce.Atf.Wpf.Applications;

public interface ILayoutConstraint
{
	string Name { get; }

	bool Enabled { get; set; }

	Rect Constrain(Rect bounds, BoundsSpecified specified);
}
