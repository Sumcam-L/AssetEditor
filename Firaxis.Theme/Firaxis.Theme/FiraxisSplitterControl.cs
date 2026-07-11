using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace Firaxis.Theme;

internal class FiraxisSplitterControl : DockPane.SplitterControlBase
{
	private SolidBrush HorizontalBrush => base.DockPane.DockPanel.Theme.PaintingService.GetBrush(base.DockPane.DockPanel.Theme.ColorPalette.TabSelectedInactive.Background);

	private SolidBrush BackgroundBrush => base.DockPane.DockPanel.Theme.PaintingService.GetBrush(base.DockPane.DockPanel.Theme.ColorPalette.MainWindowActive.Background);

	private PathGradientBrush ForegroundBrush { get; set; }

	private int SplitterSize { get; }

	public FiraxisSplitterControl(DockPane pane)
		: base(pane)
	{
		SplitterSize = pane.DockPanel.Theme.Measures.SplitterSize;
	}

	protected override void OnSizeChanged(EventArgs e)
	{
		base.OnSizeChanged(e);
		Rectangle clientRectangle = base.ClientRectangle;
		if (clientRectangle.Width > 0 && clientRectangle.Height > 0 && (base.Alignment == DockAlignment.Left || base.Alignment == DockAlignment.Right))
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
			SurroundColors = new Color[1] { base.DockPane.DockPanel.Theme.ColorPalette.MainWindowActive.Background }
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

	protected override void OnPaint(PaintEventArgs e)
	{
		base.OnPaint(e);
		Rectangle clientRectangle = base.ClientRectangle;
		if (clientRectangle.Width <= 0 || clientRectangle.Height <= 0)
		{
			return;
		}
		switch (base.Alignment)
		{
		case DockAlignment.Left:
		case DockAlignment.Right:
			if (ForegroundBrush == null)
			{
				MakeForegroundBrush(clientRectangle);
			}
			e.Graphics.FillRectangle(BackgroundBrush, clientRectangle);
			e.Graphics.FillRectangle(ForegroundBrush, clientRectangle.X + SplitterSize / 2 - 1, clientRectangle.Y, SplitterSize / 3, clientRectangle.Height);
			break;
		case DockAlignment.Top:
		case DockAlignment.Bottom:
			e.Graphics.FillRectangle(HorizontalBrush, clientRectangle);
			break;
		}
	}
}
