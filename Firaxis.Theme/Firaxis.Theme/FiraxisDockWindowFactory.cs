using WeifenLuo.WinFormsUI.Docking;

namespace Firaxis.Theme;

internal class FiraxisDockWindowFactory : DockPanelExtender.IDockWindowFactory
{
	public DockWindow CreateDockWindow(DockPanel dockPanel, DockState dockState)
	{
		return new FiraxisDockWindow(dockPanel, dockState);
	}
}
