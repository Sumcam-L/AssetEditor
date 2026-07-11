using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace WeifenLuo.WinFormsUI.ThemeVS2012;

public class VS2012WindowSplitterControl : SplitterBase
{
	private readonly SolidBrush _horizontalBrush;

	private readonly SolidBrush _backgroundBrush;

	private PathGradientBrush _foregroundBrush;

	private readonly Color[] _verticalSurroundColors;

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

	public VS2012WindowSplitterControl(ISplitterHost host)
	{
		_host = host;
		_horizontalBrush = host.DockPanel.Theme.PaintingService.GetBrush(host.DockPanel.Theme.ColorPalette.TabSelectedInactive.Background);
		_backgroundBrush = host.DockPanel.Theme.PaintingService.GetBrush(host.DockPanel.Theme.ColorPalette.MainWindowActive.Background);
		_verticalSurroundColors = new Color[1] { host.DockPanel.Theme.ColorPalette.MainWindowActive.Background };
	}

	protected override void OnSizeChanged(EventArgs e)
	{
		var sw = System.Diagnostics.Stopwatch.StartNew();
		base.OnSizeChanged(e);
		Rectangle clientRectangle = base.ClientRectangle;
		if (clientRectangle.Width <= 0 || clientRectangle.Height <= 0 || (Dock != DockStyle.Left && Dock != DockStyle.Right))
		{
			return;
		}
		_foregroundBrush?.Dispose();
		using GraphicsPath graphicsPath = new GraphicsPath();
		graphicsPath.AddRectangle(clientRectangle);
		_foregroundBrush = new PathGradientBrush(graphicsPath)
		{
			CenterColor = _horizontalBrush.Color,
			SurroundColors = _verticalSurroundColors
		};
		sw.Stop();
		if (sw.ElapsedMilliseconds > 1)
			System.Diagnostics.Trace.WriteLine($"[Paint] VS2012WindowSplitterControl.OnSizeChanged: {sw.ElapsedMilliseconds}ms");
	}

	protected override void Dispose(bool disposing)
	{
		if (!base.IsDisposed && disposing)
		{
			_foregroundBrush?.Dispose();
		}
		base.Dispose(disposing);
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
		if (_host.IsDockWindow)
		{
			switch (Dock)
			{
			case DockStyle.Left:
			case DockStyle.Right:
				e.Graphics.FillRectangle(_backgroundBrush, clientRectangle);
				e.Graphics.FillRectangle(_foregroundBrush, clientRectangle.X + SplitterSize / 2 - 1, clientRectangle.Y, SplitterSize / 3, clientRectangle.Height);
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
				e.Graphics.FillRectangle(_backgroundBrush, clientRectangle);
				break;
			case DockState.DockTopAutoHide:
			case DockState.DockBottomAutoHide:
				e.Graphics.FillRectangle(_horizontalBrush, clientRectangle);
				break;
			}
		}
		sw.Stop();
		if (sw.ElapsedMilliseconds > 1)
			System.Diagnostics.Trace.WriteLine($"[Paint] VS2012WindowSplitterControl.OnPaint: {sw.ElapsedMilliseconds}ms");
	}
}
