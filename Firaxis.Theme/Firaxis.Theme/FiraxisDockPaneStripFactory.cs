using WeifenLuo.WinFormsUI.Docking;

namespace Firaxis.Theme;

internal class FiraxisDockPaneStripFactory : DockPanelExtender.IDockPaneStripFactory
{
	public DockPaneStripBase CreateDockPaneStrip(DockPane pane)
	{
		FiraxisDockPaneStrip firaxisDockPaneStrip = new FiraxisDockPaneStrip(pane);
		firaxisDockPaneStrip.AlwaysShowDocOverflow = true;
		return firaxisDockPaneStrip;
	}
}
