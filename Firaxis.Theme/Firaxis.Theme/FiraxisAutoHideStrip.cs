using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace Firaxis.Theme;

internal class FiraxisAutoHideStrip : AutoHideStripBase
{
	private class TabFiraxis : Tab
	{
		public int TabX { get; set; }

		public int TabWidth { get; set; }

		public bool IsMouseOver { get; set; }

		internal TabFiraxis(IDockContent content)
			: base(content)
		{
		}
	}

	private const int TextGapLeft = 0;

	private const int TextGapRight = 0;

	private const int TextGapBottom = 3;

	private const int TabGapTop = 3;

	private const int TabGapBottom = 8;

	private const int TabGapLeft = 0;

	private const int TabGapBetween = 12;

	private static StringFormat _stringFormatTabHorizontal;

	private static StringFormat _stringFormatTabVertical;

	private static Matrix _matrixIdentity = new Matrix();

	private static DockState[] _dockStates;

	private static GraphicsPath _graphicsPath;

	private TabFiraxis lastSelectedTab = null;

	public Font TextFont => base.DockPanel.Theme.Skin.AutoHideStripSkin.TextFont;

	private StringFormat StringFormatTabHorizontal
	{
		get
		{
			if (_stringFormatTabHorizontal == null)
			{
				_stringFormatTabHorizontal = new StringFormat();
				_stringFormatTabHorizontal.Alignment = StringAlignment.Near;
				_stringFormatTabHorizontal.LineAlignment = StringAlignment.Center;
				_stringFormatTabHorizontal.FormatFlags = StringFormatFlags.NoWrap;
				_stringFormatTabHorizontal.Trimming = StringTrimming.None;
			}
			if (RightToLeft == RightToLeft.Yes)
			{
				_stringFormatTabHorizontal.FormatFlags |= StringFormatFlags.DirectionRightToLeft;
			}
			else
			{
				_stringFormatTabHorizontal.FormatFlags &= ~StringFormatFlags.DirectionRightToLeft;
			}
			return _stringFormatTabHorizontal;
		}
	}

	private StringFormat StringFormatTabVertical
	{
		get
		{
			if (_stringFormatTabVertical == null)
			{
				_stringFormatTabVertical = new StringFormat();
				_stringFormatTabVertical.Alignment = StringAlignment.Near;
				_stringFormatTabVertical.LineAlignment = StringAlignment.Center;
				_stringFormatTabVertical.FormatFlags = StringFormatFlags.DirectionVertical | StringFormatFlags.NoWrap;
				_stringFormatTabVertical.Trimming = StringTrimming.None;
			}
			if (RightToLeft == RightToLeft.Yes)
			{
				_stringFormatTabVertical.FormatFlags |= StringFormatFlags.DirectionRightToLeft;
			}
			else
			{
				_stringFormatTabVertical.FormatFlags &= ~StringFormatFlags.DirectionRightToLeft;
			}
			return _stringFormatTabVertical;
		}
	}

	private static Matrix MatrixIdentity => _matrixIdentity;

	private static DockState[] DockStates
	{
		get
		{
			if (_dockStates == null)
			{
				_dockStates = new DockState[4];
				_dockStates[0] = DockState.DockLeftAutoHide;
				_dockStates[1] = DockState.DockRightAutoHide;
				_dockStates[2] = DockState.DockTopAutoHide;
				_dockStates[3] = DockState.DockBottomAutoHide;
			}
			return _dockStates;
		}
	}

	internal static GraphicsPath GraphicsPath
	{
		get
		{
			if (_graphicsPath == null)
			{
				_graphicsPath = new GraphicsPath();
			}
			return _graphicsPath;
		}
	}

	public FiraxisAutoHideStrip(DockPanel panel)
		: base(panel)
	{
		SetStyle(ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, value: true);
		BackColor = base.DockPanel.Theme.ColorPalette.MainWindowActive.Background;
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		base.OnPaint(e);
		Graphics graphics = e.Graphics;
		DrawTabStrip(graphics);
	}

	protected override void OnLayout(LayoutEventArgs levent)
	{
		CalculateTabs();
		base.OnLayout(levent);
	}

	private void DrawTabStrip(Graphics g)
	{
		DrawTabStrip(g, DockState.DockTopAutoHide);
		DrawTabStrip(g, DockState.DockBottomAutoHide);
		DrawTabStrip(g, DockState.DockLeftAutoHide);
		DrawTabStrip(g, DockState.DockRightAutoHide);
	}

	private void DrawTabStrip(Graphics g, DockState dockState)
	{
		Rectangle logicalTabStripRectangle = GetLogicalTabStripRectangle(dockState);
		if (logicalTabStripRectangle.IsEmpty)
		{
			return;
		}
		Matrix transform = g.Transform;
		if (dockState == DockState.DockLeftAutoHide || dockState == DockState.DockRightAutoHide)
		{
			Matrix matrix = new Matrix();
			matrix.RotateAt(90f, new PointF((float)logicalTabStripRectangle.X + (float)logicalTabStripRectangle.Height / 2f, (float)logicalTabStripRectangle.Y + (float)logicalTabStripRectangle.Height / 2f));
			g.Transform = matrix;
		}
		foreach (Pane item in (IEnumerable<Pane>)GetPanes(dockState))
		{
			foreach (TabFiraxis item2 in (IEnumerable<Tab>)item.AutoHideTabs)
			{
				DrawTab(g, item2);
			}
		}
		g.Transform = transform;
	}

	private void CalculateTabs()
	{
		CalculateTabs(DockState.DockTopAutoHide);
		CalculateTabs(DockState.DockBottomAutoHide);
		CalculateTabs(DockState.DockLeftAutoHide);
		CalculateTabs(DockState.DockRightAutoHide);
	}

	private void CalculateTabs(DockState dockState)
	{
		int num = GetLogicalTabStripRectangle(dockState).X;
		foreach (Pane item in (IEnumerable<Pane>)GetPanes(dockState))
		{
			foreach (TabFiraxis item2 in (IEnumerable<Tab>)item.AutoHideTabs)
			{
				int num2 = TextRenderer.MeasureText(item2.Content.DockHandler.TabText, TextFont).Width;
				item2.TabX = num;
				item2.TabWidth = num2;
				num += num2 + 12;
			}
		}
	}

	private Rectangle RtlTransform(Rectangle rect, DockState dockState)
	{
		if (dockState == DockState.DockLeftAutoHide || dockState == DockState.DockRightAutoHide)
		{
			return rect;
		}
		return DrawHelper.RtlTransform(this, rect);
	}

	private GraphicsPath GetTabOutline(TabFiraxis tab, bool rtlTransform)
	{
		DockState dockState = tab.Content.DockHandler.DockState;
		Rectangle rect = GetTabRectangle(tab);
		if (rtlTransform)
		{
			rect = RtlTransform(rect, dockState);
		}
		if (GraphicsPath != null)
		{
			GraphicsPath.Reset();
			GraphicsPath.AddRectangle(rect);
		}
		return GraphicsPath;
	}

	private void DrawTab(Graphics g, TabFiraxis tab)
	{
		Rectangle tabRectangle = GetTabRectangle(tab);
		if (!tabRectangle.IsEmpty)
		{
			DockState dockState = tab.Content.DockHandler.DockState;
			IDockContent content = tab.Content;
			Matrix transform = g.Transform;
			g.Transform = MatrixIdentity;
			Color border;
			Color background;
			Color color;
			if (tab.IsMouseOver)
			{
				border = base.DockPanel.Theme.ColorPalette.AutoHideStripHovered.Border;
				background = base.DockPanel.Theme.ColorPalette.AutoHideStripHovered.Background;
				color = base.DockPanel.Theme.ColorPalette.AutoHideStripHovered.Text;
			}
			else
			{
				border = base.DockPanel.Theme.ColorPalette.AutoHideStripDefault.Border;
				background = base.DockPanel.Theme.ColorPalette.AutoHideStripDefault.Background;
				color = base.DockPanel.Theme.ColorPalette.AutoHideStripDefault.Text;
			}
			g.FillRectangle(base.DockPanel.Theme.PaintingService.GetBrush(background), tabRectangle);
			Rectangle borderRectangle = GetBorderRectangle(tabRectangle, dockState, TextRenderer.MeasureText(tab.Content.DockHandler.TabText, TextFont).Width);
			g.FillRectangle(base.DockPanel.Theme.PaintingService.GetBrush(border), borderRectangle);
			Rectangle textRectangle = GetTextRectangle(tabRectangle, dockState);
			if (dockState == DockState.DockLeftAutoHide || dockState == DockState.DockRightAutoHide)
			{
				g.DrawString(content.DockHandler.TabText, TextFont, base.DockPanel.Theme.PaintingService.GetBrush(color), textRectangle, StringFormatTabVertical);
			}
			else
			{
				g.DrawString(content.DockHandler.TabText, TextFont, base.DockPanel.Theme.PaintingService.GetBrush(color), textRectangle, StringFormatTabHorizontal);
			}
			g.Transform = transform;
		}
	}

	private Rectangle GetBorderRectangle(Rectangle tab, DockState state, int width)
	{
		Rectangle result = new Rectangle(tab.Location, tab.Size);
		switch (state)
		{
		case DockState.DockLeftAutoHide:
			result.Height = width;
			result.Width = base.DockPanel.Theme.Measures.AutoHideTabLineWidth;
			result.Y = result.Y;
			return result;
		case DockState.DockRightAutoHide:
			result.Height = width;
			result.Width = base.DockPanel.Theme.Measures.AutoHideTabLineWidth;
			result.X += tab.Width - result.Width;
			result.Y = result.Y;
			return result;
		case DockState.DockBottomAutoHide:
			result.Width = width;
			result.Height = base.DockPanel.Theme.Measures.AutoHideTabLineWidth;
			result.X = result.X;
			result.Y += tab.Height - result.Height;
			return result;
		case DockState.DockTopAutoHide:
			result.Width = width;
			result.Height = base.DockPanel.Theme.Measures.AutoHideTabLineWidth;
			result.X = result.X;
			return result;
		default:
			return Rectangle.Empty;
		}
	}

	public Rectangle GetLogicalTabStripRectangle(DockState state)
	{
		Rectangle tabStripRectangle = GetTabStripRectangle(state);
		Point location = tabStripRectangle.Location;
		if (state == DockState.DockLeftAutoHide || state == DockState.DockRightAutoHide)
		{
			return new Rectangle(0, 0, tabStripRectangle.Height, tabStripRectangle.Width);
		}
		return new Rectangle(0, 0, tabStripRectangle.Width, tabStripRectangle.Height);
	}

	private Rectangle GetTabRectangle(TabFiraxis tab)
	{
		DockState dockState = tab.Content.DockHandler.DockState;
		Rectangle tabStripRectangle = GetTabStripRectangle(dockState);
		Point location = tabStripRectangle.Location;
		if (dockState == DockState.DockLeftAutoHide || dockState == DockState.DockRightAutoHide)
		{
			location.Y += tab.TabX;
			return new Rectangle(location.X, location.Y, tabStripRectangle.Width, tab.TabWidth);
		}
		location.X += tab.TabX;
		return new Rectangle(location.X, location.Y, tab.TabWidth, tabStripRectangle.Height);
	}

	private Rectangle GetTextRectangle(Rectangle tab, DockState state)
	{
		Rectangle result = new Rectangle(tab.Location, tab.Size);
		switch (state)
		{
		case DockState.DockLeftAutoHide:
			result.X += 3;
			result.Y = result.Y;
			result.Height = result.Height;
			result.Width -= 3;
			return result;
		case DockState.DockRightAutoHide:
			result.Y = result.Y;
			result.Height = result.Height;
			result.Width -= 3;
			return result;
		case DockState.DockBottomAutoHide:
			result.X = result.X;
			result.Width = result.Width;
			result.Height -= 3;
			return result;
		case DockState.DockTopAutoHide:
			result.X = result.X;
			result.Y += 3;
			result.Width = result.Width;
			result.Height -= 3;
			return result;
		default:
			return Rectangle.Empty;
		}
	}

	protected override IDockContent HitTest(Point point)
	{
		return TabHitTest(point)?.Content;
	}

	protected override Rectangle GetTabBounds(Tab tab)
	{
		GraphicsPath tabOutline = GetTabOutline((TabFiraxis)tab, rtlTransform: true);
		RectangleF bounds = tabOutline.GetBounds();
		return new Rectangle((int)bounds.Left, (int)bounds.Top, (int)bounds.Width, (int)bounds.Height);
	}

	protected Tab TabHitTest(Point ptMouse)
	{
		DockState[] dockStates = DockStates;
		foreach (DockState dockState in dockStates)
		{
			if (!GetTabStripRectangle(dockState).Contains(ptMouse))
			{
				continue;
			}
			foreach (Pane item in (IEnumerable<Pane>)GetPanes(dockState))
			{
				foreach (TabFiraxis item2 in (IEnumerable<Tab>)item.AutoHideTabs)
				{
					GraphicsPath tabOutline = GetTabOutline(item2, rtlTransform: true);
					if (tabOutline.IsVisible(ptMouse))
					{
						return item2;
					}
				}
			}
		}
		return null;
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
		base.OnMouseMove(e);
		TabFiraxis tabFiraxis = (TabFiraxis)TabHitTest(PointToClient(Control.MousePosition));
		if (tabFiraxis != null)
		{
			tabFiraxis.IsMouseOver = true;
			Invalidate();
		}
		if (lastSelectedTab != tabFiraxis)
		{
			if (lastSelectedTab != null)
			{
				lastSelectedTab.IsMouseOver = false;
				Invalidate();
			}
			lastSelectedTab = tabFiraxis;
		}
	}

	protected override void OnMouseLeave(EventArgs e)
	{
		base.OnMouseLeave(e);
		if (lastSelectedTab != null)
		{
			lastSelectedTab.IsMouseOver = false;
		}
		Invalidate();
	}

	protected override int MeasureHeight()
	{
		return 31;
	}

	protected override void OnRefreshChanges()
	{
		CalculateTabs();
		Invalidate();
	}

	protected override Tab CreateTab(IDockContent content)
	{
		return new TabFiraxis(content);
	}
}
