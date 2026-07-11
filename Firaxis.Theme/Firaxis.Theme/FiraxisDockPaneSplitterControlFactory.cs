using WeifenLuo.WinFormsUI.Docking;

namespace Firaxis.Theme;

internal class FiraxisDockPaneSplitterControlFactory : DockPanelExtender.IDockPaneSplitterControlFactory
{
	public DockPane.SplitterControlBase CreateSplitterControl(DockPane pane)
	{
		return new FiraxisSplitterControl(pane);
	}
}
