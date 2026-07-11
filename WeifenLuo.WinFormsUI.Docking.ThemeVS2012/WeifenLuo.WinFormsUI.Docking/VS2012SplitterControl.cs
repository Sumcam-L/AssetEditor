using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace WeifenLuo.WinFormsUI.Docking;

internal class VS2012SplitterControl : DockPane.SplitterControlBase
{
	private readonly SolidBrush _horizontalBrush;

	private readonly SolidBrush _backgroundBrush;

	private PathGradientBrush _foregroundBrush;

	private readonly Color[] _verticalSurroundColors;

	private int SplitterSize { get; }

	public VS2012SplitterControl(DockPane pane)
		: base(pane)
	{
		_horizontalBrush = pane.DockPanel.Theme.PaintingService.GetBrush(pane.DockPanel.Theme.ColorPalette.TabSelectedInactive.Background);
		_backgroundBrush = pane.DockPanel.Theme.PaintingService.GetBrush(pane.DockPanel.Theme.ColorPalette.MainWindowActive.Background);
		_verticalSurroundColors = new Color[1] { pane.DockPanel.Theme.ColorPalette.MainWindowActive.Background };
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
		_foregroundBrush?.Dispose();
		using GraphicsPath graphicsPath = new GraphicsPath();
		graphicsPath.AddRectangle(rect);
		_foregroundBrush = new PathGradientBrush(graphicsPath)
		{
			CenterColor = _horizontalBrush.Color,
			SurroundColors = _verticalSurroundColors
		};
	}

	protected override void Dispose(bool disposing)
	{
		if (!base.IsDisposed && disposing)
		{
			_foregroundBrush?.Dispose();
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
			if (_foregroundBrush == null)
			{
				MakeForegroundBrush(clientRectangle);
			}
			e.Graphics.FillRectangle(_backgroundBrush, clientRectangle);
			e.Graphics.FillRectangle(_foregroundBrush, clientRectangle.X + SplitterSize / 2 - 1, clientRectangle.Y, SplitterSize / 3, clientRectangle.Height);
			break;
		case DockAlignment.Top:
		case DockAlignment.Bottom:
			e.Graphics.FillRectangle(_horizontalBrush, clientRectangle);
			break;
		}
	}
}
