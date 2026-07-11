using WeifenLuo.WinFormsUI.Docking;

namespace Firaxis.Theme;

internal class FiraxisDockPaneCaptionFactory : DockPanelExtender.IDockPaneCaptionFactory
{
	public DockPaneCaptionBase CreateDockPaneCaption(DockPane pane)
	{
		return new FiraxisDockPaneCaption(pane);
	}
}
