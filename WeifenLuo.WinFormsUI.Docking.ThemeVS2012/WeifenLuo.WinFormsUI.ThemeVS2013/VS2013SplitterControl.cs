using System.Drawing;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace WeifenLuo.WinFormsUI.ThemeVS2013;

internal class VS2013SplitterControl : DockPane.SplitterControlBase
{
	private readonly SolidBrush _horizontalBrush;

	private int SplitterSize { get; }

	public VS2013SplitterControl(DockPane pane)
		: base(pane)
	{
		_horizontalBrush = pane.DockPanel.Theme.PaintingService.GetBrush(pane.DockPanel.Theme.ColorPalette.MainWindowActive.Background);
		SplitterSize = pane.DockPanel.Theme.Measures.SplitterSize;
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		base.OnPaint(e);
		Rectangle clientRectangle = base.ClientRectangle;
		if (clientRectangle.Width > 0 && clientRectangle.Height > 0)
		{
			switch (base.Alignment)
			{
			case DockAlignment.Left:
			case DockAlignment.Right:
				e.Graphics.FillRectangle(_horizontalBrush, clientRectangle);
				break;
			case DockAlignment.Top:
			case DockAlignment.Bottom:
				e.Graphics.FillRectangle(_horizontalBrush, clientRectangle);
				break;
			}
		}
	}
}
