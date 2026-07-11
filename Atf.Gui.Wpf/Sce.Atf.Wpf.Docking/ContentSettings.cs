using System.Windows;

namespace Sce.Atf.Wpf.Docking;

internal class ContentSettings
{
	public DockTo DefaultDock { get; private set; }

	public Size Size { get; set; }

	public DockState DockState { get; set; }

	public Point Location { get; set; }

	public ContentSettings(DockTo defaultDock)
	{
		DefaultDock = defaultDock;
		Size = new Size(0.0, 0.0);
		DockState = DockState.Docked;
	}
}
