using System.ComponentModel;
using System.Drawing;
using WeifenLuo.WinFormsUI.Docking;

namespace Firaxis.Theme;

[ToolboxItem(false)]
internal class FiraxisDockWindow : DockWindow
{
	public override Rectangle DisplayingRectangle
	{
		get
		{
			Rectangle clientRectangle = base.ClientRectangle;
			if (base.DockState == DockState.DockLeft)
			{
				clientRectangle.Width -= base.DockPanel.Theme.Measures.SplitterSize;
			}
			else if (base.DockState == DockState.DockRight)
			{
				clientRectangle.X += base.DockPanel.Theme.Measures.SplitterSize;
				clientRectangle.Width -= base.DockPanel.Theme.Measures.SplitterSize;
			}
			else if (base.DockState == DockState.DockTop)
			{
				clientRectangle.Height -= base.DockPanel.Theme.Measures.SplitterSize;
			}
			else if (base.DockState == DockState.DockBottom)
			{
				clientRectangle.Y += base.DockPanel.Theme.Measures.SplitterSize;
				clientRectangle.Height -= base.DockPanel.Theme.Measures.SplitterSize;
			}
			return clientRectangle;
		}
	}

	public FiraxisDockWindow(DockPanel dockPanel, DockState dockState)
		: base(dockPanel, dockState)
	{
	}
}
