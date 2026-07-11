using WeifenLuo.WinFormsUI.Docking;

namespace Firaxis.Theme;

internal class FiraxisAutoHideWindowFactory : DockPanelExtender.IAutoHideWindowFactory
{
	public DockPanel.AutoHideWindowControl CreateAutoHideWindow(DockPanel panel)
	{
		return new FiraxisAutoHideWindowControl(panel);
	}
}
