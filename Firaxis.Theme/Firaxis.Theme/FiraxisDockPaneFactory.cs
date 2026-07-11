using System.Drawing;
using WeifenLuo.WinFormsUI.Docking;

namespace Firaxis.Theme;

public class FiraxisDockPaneFactory : DockPanelExtender.IDockPaneFactory
{
	public DockPane CreateDockPane(IDockContent content, DockState visibleState, bool show)
	{
		return new FiraxisDockPane(content, visibleState, show);
	}

	public DockPane CreateDockPane(IDockContent content, FloatWindow floatWindow, bool show)
	{
		return new FiraxisDockPane(content, floatWindow, show);
	}

	public DockPane CreateDockPane(IDockContent content, DockPane previousPane, DockAlignment alignment, double proportion, bool show)
	{
		return new FiraxisDockPane(content, previousPane, alignment, proportion, show);
	}

	public DockPane CreateDockPane(IDockContent content, Rectangle floatWindowBounds, bool show)
	{
		return new FiraxisDockPane(content, floatWindowBounds, show);
	}
}
