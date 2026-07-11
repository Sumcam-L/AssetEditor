using WeifenLuo.WinFormsUI.Docking;

namespace Firaxis.Theme;

internal class FiraxisWindowSplitterControlFactory : DockPanelExtender.IWindowSplitterControlFactory
{
	public SplitterBase CreateSplitterControl(ISplitterHost host)
	{
		return new FiraxisWindowSplitterControl(host);
	}
}
