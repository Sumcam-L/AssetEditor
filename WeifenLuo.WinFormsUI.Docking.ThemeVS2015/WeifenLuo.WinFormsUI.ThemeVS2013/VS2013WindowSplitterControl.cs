using System.Drawing;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace WeifenLuo.WinFormsUI.ThemeVS2013;

public class VS2013WindowSplitterControl : SplitterBase
{
	private SolidBrush _horizontalBrush;

	private readonly ISplitterHost _host;

	protected override int SplitterSize
	{
		get
		{
			if (!_host.IsDockWindow)
			{
				return _host.DockPanel.Theme.Measures.AutoHideSplitterSize;
			}
			return _host.DockPanel.Theme.Measures.SplitterSize;
		}
	}

	public VS2013WindowSplitterControl(ISplitterHost host)
	{
		_host = host;
	}

	protected override void StartDrag()
	{
		_host.DockPanel.BeginDrag(_host, _host.DragControl.RectangleToScreen(base.Bounds));
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		var sw = System.Diagnostics.Stopwatch.StartNew();
		base.OnPaint(e);
		Rectangle clientRectangle = base.ClientRectangle;
		if (clientRectangle.Width <= 0 || clientRectangle.Height <= 0)
		{
			return;
		}
		_horizontalBrush = _host.DockPanel.Theme.PaintingService.GetBrush(_host.DockPanel.Theme.ColorPalette.MainWindowActive.Background);
		if (_host.IsDockWindow)
		{
			switch (Dock)
			{
			case DockStyle.Left:
			case DockStyle.Right:
				e.Graphics.FillRectangle(_horizontalBrush, clientRectangle);
				break;
			case DockStyle.Top:
			case DockStyle.Bottom:
				e.Graphics.FillRectangle(_horizontalBrush, clientRectangle);
				break;
			}
		}
		else
		{
			switch (_host.DockState)
			{
			case DockState.DockLeftAutoHide:
			case DockState.DockRightAutoHide:
				e.Graphics.FillRectangle(_horizontalBrush, clientRectangle);
				break;
			case DockState.DockTopAutoHide:
			case DockState.DockBottomAutoHide:
				e.Graphics.FillRectangle(_horizontalBrush, clientRectangle);
				break;
			}
		}
		sw.Stop();
		if (sw.ElapsedMilliseconds > 1)
			System.Diagnostics.Trace.WriteLine($"[Paint] VS2013WindowSplitterControl.OnPaint: {sw.ElapsedMilliseconds}ms");
	}
}
