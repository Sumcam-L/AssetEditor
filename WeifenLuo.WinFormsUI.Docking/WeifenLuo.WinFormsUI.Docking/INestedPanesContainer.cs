using System.Drawing;

namespace WeifenLuo.WinFormsUI.Docking;

public interface INestedPanesContainer
{
	DockState DockState { get; }

	Rectangle DisplayingRectangle { get; }

	NestedPaneCollection NestedPanes { get; }

	VisibleNestedPaneCollection VisibleNestedPanes { get; }

	bool IsFloat { get; }
}
