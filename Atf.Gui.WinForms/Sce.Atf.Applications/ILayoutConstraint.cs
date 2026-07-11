using System.Drawing;
using System.Windows.Forms;

namespace Sce.Atf.Applications;

public interface ILayoutConstraint
{
	string Name { get; }

	bool Enabled { get; set; }

	Rectangle Constrain(Rectangle bounds, BoundsSpecified specified);
}
