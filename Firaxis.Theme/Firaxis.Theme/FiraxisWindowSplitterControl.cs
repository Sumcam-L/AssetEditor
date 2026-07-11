using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace Firaxis.Theme;

public class FiraxisWindowSplitterControl : SplitterBase
{
	private readonly ISplitterHost _host;

	private SolidBrush HorizontalBrush => _host.DockPanel.Theme.PaintingService.GetBrush(_host.DockPanel.Theme.ColorPalette.TabSelectedInactive.Background);

	private SolidBrush BackgroundBrush => _host.DockPanel.Theme.PaintingService.GetBrush(_host.DockPanel.Theme.ColorPalette.MainWindowActive.Background);

	private PathGradientBrush ForegroundBrush { get; set; }

	protected override int SplitterSize => _host.IsDockWindow ? _host.DockPanel.Theme.Measures.SplitterSize : _host.DockPanel.Theme.Measures.AutoHideSplitterSize;

	public FiraxisWindowSplitterControl(ISplitterHost host)
	{
		_host = host;
	}

	protected override void OnSizeChanged(EventArgs e)
	{
		base.OnSizeChanged(e);
		Rectangle clientRectangle = base.ClientRectangle;
		if (clientRectangle.Width > 0 && clientRectangle.Height > 0 && (Dock == DockStyle.Left || Dock == DockStyle.Right))
		{
			MakeForegroundBrush(clientRectangle);
		}
	}

	private void MakeForegroundBrush(Rectangle rect)
	{
		ForegroundBrush?.Dispose();
		using GraphicsPath graphicsPath = new GraphicsPath();
		graphicsPath.AddRectangle(rect);
		ForegroundBrush = new PathGradientBrush(graphicsPath)
		{
			CenterColor = HorizontalBrush.Color,
			SurroundColors = new Color[1] { _host.DockPanel.Theme.ColorPalette.MainWindowActive.Background }
		};
	}

	protected override void Dispose(bool disposing)
	{
		if (!base.IsDisposed && disposing)
		{
			ForegroundBrush?.Dispose();
		}
		base.Dispose(disposing);
	}

	protected override void StartDrag()
	{
		_host.DockPanel.BeginDrag(_host, _host.DragControl.RectangleToScreen(base.Bounds));
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		base.OnPaint(e);
		Rectangle clientRectangle = base.ClientRectangle;
		if (clientRectangle.Width <= 0 || clientRectangle.Height <= 0)
		{
			return;
		}
		if (_host.IsDockWindow)
		{
			if (ForegroundBrush == null)
			{
				MakeForegroundBrush(clientRectangle);
			}
			switch (Dock)
			{
			case DockStyle.Left:
			case DockStyle.Right:
			{
				int num2 = Math.Min(SplitterSize, clientRectangle.Width);
				e.Graphics.FillRectangle(BackgroundBrush, clientRectangle);
				e.Graphics.FillRectangle(ForegroundBrush, clientRectangle.X + num2 / 2 - 1, clientRectangle.Y, num2 / 3, clientRectangle.Height);
				break;
			}
			case DockStyle.Top:
			case DockStyle.Bottom:
			{
				int num = Math.Min(SplitterSize, clientRectangle.Height);
				e.Graphics.FillRectangle(BackgroundBrush, clientRectangle);
				e.Graphics.FillRectangle(ForegroundBrush, clientRectangle.X, clientRectangle.Y + num / 2 - 1, clientRectangle.Width, num / 3);
				break;
			}
			}
		}
		else
		{
			switch (_host.DockState)
			{
			case DockState.DockLeftAutoHide:
			case DockState.DockRightAutoHide:
				clientRectangle.Width = Math.Min(SplitterSize, clientRectangle.Width);
				e.Graphics.FillRectangle(BackgroundBrush, clientRectangle);
				break;
			case DockState.DockTopAutoHide:
			case DockState.DockBottomAutoHide:
				clientRectangle.Height = Math.Min(SplitterSize, clientRectangle.Height);
				e.Graphics.FillRectangle(HorizontalBrush, clientRectangle);
				break;
			}
		}
	}
}
