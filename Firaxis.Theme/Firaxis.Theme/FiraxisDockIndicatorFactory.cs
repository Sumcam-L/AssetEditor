using WeifenLuo.WinFormsUI.Docking;

namespace Firaxis.Theme;

internal class FiraxisDockIndicatorFactory : DockPanelExtender.IDockIndicatorFactory
{
	public DockPanel.DockDragHandler.DockIndicator CreateDockIndicator(DockPanel.DockDragHandler dockDragHandler)
	{
		return new DockPanel.DockDragHandler.DockIndicator(dockDragHandler)
		{
			Opacity = 0.7
		};
	}
}
