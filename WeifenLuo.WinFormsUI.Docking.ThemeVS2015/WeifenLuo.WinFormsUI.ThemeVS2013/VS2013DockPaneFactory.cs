using System.Drawing;
using WeifenLuo.WinFormsUI.Docking;

namespace WeifenLuo.WinFormsUI.ThemeVS2013;

public class VS2013DockPaneFactory : DockPanelExtender.IDockPaneFactory
{
	public DockPane CreateDockPane(IDockContent content, DockState visibleState, bool show)
	{
		return new VS2013DockPane(content, visibleState, show);
	}

	public DockPane CreateDockPane(IDockContent content, FloatWindow floatWindow, bool show)
	{
		return new VS2013DockPane(content, floatWindow, show);
	}

	public DockPane CreateDockPane(IDockContent content, DockPane previousPane, DockAlignment alignment, double proportion, bool show)
	{
		return new VS2013DockPane(content, previousPane, alignment, proportion, show);
	}

	public DockPane CreateDockPane(IDockContent content, Rectangle floatWindowBounds, bool show)
	{
		return new VS2013DockPane(content, floatWindowBounds, show);
	}
}
