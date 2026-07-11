using System.Drawing;
using System.Windows.Forms;

namespace WeifenLuo.WinFormsUI.Docking;

public interface IDockDragSource : IDragSource
{
	Rectangle BeginDrag(Point ptMouse);

	void EndDrag();

	bool IsDockStateValid(DockState dockState);

	bool CanDockTo(DockPane pane);

	void FloatAt(Rectangle floatWindowBounds);

	void DockTo(DockPane pane, DockStyle dockStyle, int contentIndex);

	void DockTo(DockPanel panel, DockStyle dockStyle);
}
