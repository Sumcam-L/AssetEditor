using WeifenLuo.WinFormsUI.Docking;

namespace Firaxis.Theme;

internal class FiraxisAutoHideStripFactory : DockPanelExtender.IAutoHideStripFactory
{
	public AutoHideStripBase CreateAutoHideStrip(DockPanel panel)
	{
		return new FiraxisAutoHideStrip(panel);
	}
}
