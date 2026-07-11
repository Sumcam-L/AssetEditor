using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace WeifenLuo.WinFormsUI.Docking;

[ToolboxItem(false)]
internal class DefaultDockWindow : DockWindow
{
	internal DefaultDockWindow(DockPanel dockPanel, DockState dockState)
		: base(dockPanel, dockState)
	{
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		if (base.DockState == DockState.Document)
		{
			e.Graphics.DrawRectangle(SystemPens.ControlDark, base.ClientRectangle.X, base.ClientRectangle.Y, base.ClientRectangle.Width - 1, base.ClientRectangle.Height - 1);
		}
		base.OnPaint(e);
	}
}
