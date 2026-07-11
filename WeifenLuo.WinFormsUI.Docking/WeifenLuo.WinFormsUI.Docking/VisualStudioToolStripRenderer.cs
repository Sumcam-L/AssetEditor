using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace WeifenLuo.WinFormsUI.Docking;

public class VisualStudioToolStripRenderer : ToolStripProfessionalRenderer
{
	private static Rectangle[] baseSizeGripRectangles = new Rectangle[10]
	{
		new Rectangle(6, 0, 1, 1),
		new Rectangle(6, 2, 1, 1),
		new Rectangle(6, 4, 1, 1),
		new Rectangle(6, 6, 1, 1),
		new Rectangle(4, 2, 1, 1),
		new Rectangle(4, 4, 1, 1),
		new Rectangle(4, 6, 1, 1),
		new Rectangle(2, 4, 1, 1),
		new Rectangle(2, 6, 1, 1),
		new Rectangle(0, 6, 1, 1)
	};

	private const int GRIP_PADDING = 4;

	private SolidBrush _statusBarBrush;

	private SolidBrush _statusGripBrush;

	private SolidBrush _statusGripAccentBrush;

	private SolidBrush _toolBarBrush;

	private SolidBrush _gripBrush;

	private Pen _toolBarBorderPen;

	private VisualStudioColorTable _table;

	private DockPanelColorPalette _palette;

	public bool UseGlassOnMenuStrip { get; set; }

	public VisualStudioToolStripRenderer(DockPanelColorPalette palette)
		: base(new VisualStudioColorTable(palette))
	{
		_table = (VisualStudioColorTable)base.ColorTable;
		_palette = palette;
		base.RoundedEdges = false;
		_statusBarBrush = new SolidBrush(palette.MainWindowStatusBarDefault.Background);
		_statusGripBrush = new SolidBrush(palette.MainWindowStatusBarDefault.ResizeGrip);
		_statusGripAccentBrush = new SolidBrush(palette.MainWindowStatusBarDefault.ResizeGripAccent);
		_toolBarBrush = new SolidBrush(palette.CommandBarToolbarDefault.Background);
		_gripBrush = new SolidBrush(palette.CommandBarToolbarDefault.Grip);
		_toolBarBorderPen = new Pen(palette.CommandBarToolbarDefault.Border);
		UseGlassOnMenuStrip = true;
	}

	protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
	{
		if (!e.Item.Enabled)
		{
			return;
		}
		bool flag = e.Item.Owner is MenuStrip;
		if (flag && e.Item.Pressed)
		{
			base.OnRenderMenuItemBackground(e);
		}
		else if (e.Item.Selected)
		{
			Rectangle contentRectangle = e.Item.ContentRectangle;
			Rectangle rect = (flag ? new Rectangle(contentRectangle.X + 2, contentRectangle.Y - 2, contentRectangle.Width - 5, contentRectangle.Height + 3) : new Rectangle(contentRectangle.X, contentRectangle.Y - 1, contentRectangle.Width, contentRectangle.Height + 1));
			Color menuItemBorder = base.ColorTable.MenuItemBorder;
			Color brushBegin;
			Color brushEnd;
			if (flag)
			{
				brushBegin = base.ColorTable.MenuItemSelectedGradientBegin;
				brushEnd = base.ColorTable.MenuItemSelectedGradientEnd;
			}
			else
			{
				brushBegin = base.ColorTable.MenuItemSelected;
				brushEnd = Color.Empty;
			}
			DrawRectangle(e.Graphics, rect, brushBegin, brushEnd, menuItemBorder, UseGlassOnMenuStrip);
		}
	}

	protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
	{
		if (!(e.ToolStrip is StatusStrip))
		{
			if (e.ToolStrip is MenuStrip)
			{
				base.OnRenderToolStripBorder(e);
				return;
			}
			if (e.ToolStrip is ToolStripDropDown)
			{
				base.OnRenderToolStripBorder(e);
				return;
			}
			Rectangle clientRectangle = e.ToolStrip.ClientRectangle;
			e.Graphics.DrawRectangle(_toolBarBorderPen, new Rectangle(clientRectangle.Location, new Size(clientRectangle.Width - 1, clientRectangle.Height - 1)));
		}
	}

	protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
	{
		if (e.ToolStrip is StatusStrip)
		{
			base.OnRenderToolStripBackground(e);
		}
		else if (e.ToolStrip is MenuStrip)
		{
			base.OnRenderToolStripBackground(e);
		}
		else if (e.ToolStrip is ToolStripDropDown)
		{
			base.OnRenderToolStripBackground(e);
		}
		else
		{
			e.Graphics.FillRectangle(_toolBarBrush, e.ToolStrip.ClientRectangle);
		}
	}

	protected override void OnRenderStatusStripSizingGrip(ToolStripRenderEventArgs e)
	{
		Graphics graphics = e.Graphics;
		if (!(e.ToolStrip is StatusStrip { SizeGripBounds: var sizeGripBounds } statusStrip) || LayoutUtils.IsZeroWidthOrHeight(sizeGripBounds))
		{
			return;
		}
		Rectangle[] array = new Rectangle[baseSizeGripRectangles.Length];
		Rectangle[] array2 = new Rectangle[baseSizeGripRectangles.Length];
		for (int i = 0; i < baseSizeGripRectangles.Length; i++)
		{
			Rectangle rectangle = baseSizeGripRectangles[i];
			if (statusStrip.RightToLeft == RightToLeft.Yes)
			{
				rectangle.X = sizeGripBounds.Width - rectangle.X - rectangle.Width;
			}
			rectangle.Offset(sizeGripBounds.X, sizeGripBounds.Bottom - 12);
			array2[i] = rectangle;
			if (statusStrip.RightToLeft == RightToLeft.Yes)
			{
				rectangle.Offset(1, -1);
			}
			else
			{
				rectangle.Offset(-1, -1);
			}
			array[i] = rectangle;
		}
		graphics.FillRectangles(_statusGripAccentBrush, array);
		graphics.FillRectangles(_statusGripBrush, array2);
	}

	protected override void OnRenderGrip(ToolStripGripRenderEventArgs e)
	{
		Graphics graphics = e.Graphics;
		Rectangle gripBounds = e.GripBounds;
		ToolStrip toolStrip = e.ToolStrip;
		bool flag = e.ToolStrip.RightToLeft == RightToLeft.Yes;
		int num = ((toolStrip.Orientation == Orientation.Horizontal) ? gripBounds.Height : gripBounds.Width);
		int num2 = ((toolStrip.Orientation == Orientation.Horizontal) ? gripBounds.Width : gripBounds.Height);
		int num3 = (num - 8) / 4;
		if (num3 <= 0)
		{
			return;
		}
		num3++;
		int num4 = ((toolStrip is MenuStrip) ? 2 : 0);
		Rectangle[] array = new Rectangle[num3];
		int num5 = 5 + num4;
		int num6 = num2 / 2;
		for (int i = 0; i < num3; i++)
		{
			array[i] = ((toolStrip.Orientation == Orientation.Horizontal) ? new Rectangle(num6, num5, 1, 1) : new Rectangle(num5, num6, 1, 1));
			num5 += 4;
		}
		int num7 = (flag ? 2 : (-2));
		if (flag)
		{
			for (int j = 0; j < num3; j++)
			{
				array[j].Offset(-num7, 0);
			}
		}
		Brush gripBrush = _gripBrush;
		for (int k = 0; k < num3 - 1; k++)
		{
			graphics.FillRectangle(gripBrush, array[k]);
		}
		for (int l = 0; l < num3; l++)
		{
			array[l].Offset(num7, -2);
		}
		graphics.FillRectangles(gripBrush, array);
		for (int m = 0; m < num3; m++)
		{
			array[m].Offset(-2 * num7, 0);
		}
		graphics.FillRectangles(gripBrush, array);
	}

	protected override void OnRenderButtonBackground(ToolStripItemRenderEventArgs e)
	{
		if (e.Item is ToolStripButton { Enabled: not false } toolStripButton)
		{
			if (!toolStripButton.Selected && !toolStripButton.Checked)
			{
				return;
			}
			Rectangle rect = new Rectangle(0, 0, toolStripButton.Width - 1, toolStripButton.Height - 1);
			Color penColor;
			Color brushBegin;
			Color brushMiddle;
			Color brushEnd;
			if (toolStripButton.Checked)
			{
				if (toolStripButton.Selected)
				{
					penColor = _table.ButtonCheckedHoveredBorder;
					brushBegin = _table.ButtonCheckedHoveredBackground;
					brushMiddle = _table.ButtonCheckedHoveredBackground;
					brushEnd = _table.ButtonCheckedHoveredBackground;
				}
				else
				{
					penColor = _table.ButtonCheckedBorder;
					brushBegin = base.ColorTable.ButtonCheckedGradientBegin;
					brushMiddle = base.ColorTable.ButtonCheckedGradientMiddle;
					brushEnd = base.ColorTable.ButtonCheckedGradientEnd;
				}
			}
			else if (toolStripButton.Pressed)
			{
				penColor = base.ColorTable.ButtonPressedBorder;
				brushBegin = base.ColorTable.ButtonPressedGradientBegin;
				brushMiddle = base.ColorTable.ButtonPressedGradientMiddle;
				brushEnd = base.ColorTable.ButtonPressedGradientEnd;
			}
			else
			{
				penColor = base.ColorTable.ButtonSelectedBorder;
				brushBegin = base.ColorTable.ButtonSelectedGradientBegin;
				brushMiddle = base.ColorTable.ButtonSelectedGradientMiddle;
				brushEnd = base.ColorTable.ButtonSelectedGradientEnd;
			}
			DrawRectangle(e.Graphics, rect, brushBegin, brushMiddle, brushEnd, penColor, glass: false);
		}
		else
		{
			base.OnRenderButtonBackground(e);
		}
	}

	protected override void Initialize(ToolStrip toolStrip)
	{
		base.Initialize(toolStrip);
		toolStrip.GripMargin = new Padding(toolStrip.GripMargin.All + 1);
	}

	protected override void OnRenderOverflowButtonBackground(ToolStripItemRenderEventArgs e)
	{
		Color backgroundTop = _palette.CommandBarMenuPopupDefault.BackgroundTop;
		if (e.Item.Pressed)
		{
			_palette.CommandBarMenuPopupDefault.BackgroundTop = _palette.CommandBarToolbarOverflowPressed.Background;
		}
		base.OnRenderOverflowButtonBackground(e);
		if (e.Item.Pressed)
		{
			_palette.CommandBarMenuPopupDefault.BackgroundTop = backgroundTop;
		}
	}

	protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
	{
		Color black = Color.Black;
		ToolStrip toolStrip = e.ToolStrip;
		if (toolStrip is StatusStrip)
		{
			black = ((!e.Item.Selected) ? _palette.MainWindowStatusBarDefault.Text : _palette.MainWindowStatusBarDefault.HighlightText);
		}
		else if (toolStrip is MenuStrip)
		{
			ToolStripButton toolStripButton = e.Item as ToolStripButton;
			bool flag = toolStripButton?.Checked ?? false;
			black = ((!e.Item.Enabled) ? _palette.CommandBarMenuPopupDisabled.Text : ((toolStripButton != null && toolStripButton.Pressed) ? _palette.CommandBarToolbarButtonPressed.Text : ((e.Item.Selected && flag) ? _palette.CommandBarToolbarButtonCheckedHovered.Text : (e.Item.Selected ? _palette.CommandBarMenuTopLevelHeaderHovered.Text : ((!flag) ? _palette.CommandBarMenuDefault.Text : _palette.CommandBarToolbarButtonChecked.Text)))));
		}
		else if (!(toolStrip is ToolStripDropDown))
		{
			black = (e.Item.Enabled ? _palette.CommandBarMenuDefault.Text : _palette.CommandBarMenuPopupDisabled.Text);
		}
		else
		{
			ToolStripButton toolStripButton2 = e.Item as ToolStripButton;
			bool flag2 = toolStripButton2?.Checked ?? false;
			black = ((!e.Item.Enabled) ? _palette.CommandBarMenuPopupDisabled.Text : ((toolStripButton2 != null && toolStripButton2.Pressed) ? _palette.CommandBarToolbarButtonPressed.Text : ((e.Item.Selected && flag2) ? _palette.CommandBarToolbarButtonCheckedHovered.Text : (e.Item.Selected ? _palette.CommandBarMenuTopLevelHeaderHovered.Text : ((!flag2) ? _palette.CommandBarMenuDefault.Text : _palette.CommandBarToolbarButtonChecked.Text)))));
		}
		TextRenderer.DrawText(e.Graphics, e.Text, e.TextFont, e.TextRectangle, black, e.TextFormat);
	}

	private static void DrawRectangle(Graphics graphics, Rectangle rect, Color brushBegin, Color brushMiddle, Color brushEnd, Color penColor, bool glass)
	{
		RectangleF rect2 = new RectangleF(rect.X, rect.Y, rect.Width, (float)rect.Height / 2f);
		RectangleF rect3 = new RectangleF(rect.X, (float)rect.Y + (float)rect.Height / 2f, rect.Width, (float)rect.Height / 2f);
		if (brushMiddle.IsEmpty && brushEnd.IsEmpty)
		{
			graphics.FillRectangle(new SolidBrush(brushBegin), rect);
		}
		if (brushMiddle.IsEmpty)
		{
			Brush brush = new LinearGradientBrush(rect, brushBegin, brushEnd, LinearGradientMode.Vertical);
			graphics.FillRectangle(brush, rect);
		}
		else
		{
			Brush brush2 = new LinearGradientBrush(rect2, brushBegin, brushMiddle, LinearGradientMode.Vertical);
			Brush brush3 = new LinearGradientBrush(rect3, brushMiddle, brushEnd, LinearGradientMode.Vertical);
			graphics.FillRectangle(brush2, rect2);
			graphics.FillRectangle(brush3, rect3);
		}
		if (glass)
		{
			Brush brush4 = new SolidBrush(Color.FromArgb(120, Color.White));
			graphics.FillRectangle(brush4, rect2);
		}
		if (penColor.A > 0)
		{
			graphics.DrawRectangle(new Pen(penColor), rect);
		}
	}

	private static void DrawRectangle(Graphics graphics, Rectangle rect, Color brushBegin, Color brushEnd, Color penColor, bool glass)
	{
		DrawRectangle(graphics, rect, brushBegin, Color.Empty, brushEnd, penColor, glass);
	}

	private static void DrawRectangle(Graphics graphics, Rectangle rect, Color brush, Color penColor, bool glass)
	{
		DrawRectangle(graphics, rect, brush, Color.Empty, Color.Empty, penColor, glass);
	}

	private static void FillRoundRectangle(Graphics graphics, Brush brush, Rectangle rect, int radius)
	{
		float x = Convert.ToSingle(rect.X);
		float y = Convert.ToSingle(rect.Y);
		float width = Convert.ToSingle(rect.Width);
		float height = Convert.ToSingle(rect.Height);
		float radius2 = Convert.ToSingle(radius);
		FillRoundRectangle(graphics, brush, x, y, width, height, radius2);
	}

	private static void FillRoundRectangle(Graphics graphics, Brush brush, float x, float y, float width, float height, float radius)
	{
		GraphicsPath roundedRect = GetRoundedRect(new RectangleF(x, y, width, height), radius);
		graphics.FillPath(brush, roundedRect);
	}

	private static void DrawRoundRectangle(Graphics graphics, Pen pen, Rectangle rect, int radius)
	{
		float x = Convert.ToSingle(rect.X);
		float y = Convert.ToSingle(rect.Y);
		float width = Convert.ToSingle(rect.Width);
		float height = Convert.ToSingle(rect.Height);
		float radius2 = Convert.ToSingle(radius);
		DrawRoundRectangle(graphics, pen, x, y, width, height, radius2);
	}

	private static void DrawRoundRectangle(Graphics graphics, Pen pen, float x, float y, float width, float height, float radius)
	{
		GraphicsPath roundedRect = GetRoundedRect(new RectangleF(x, y, width, height), radius);
		graphics.DrawPath(pen, roundedRect);
	}

	private static GraphicsPath GetRoundedRect(RectangleF baseRect, float radius)
	{
		if (radius <= 0f)
		{
			GraphicsPath graphicsPath = new GraphicsPath();
			graphicsPath.AddRectangle(baseRect);
			graphicsPath.CloseFigure();
			return graphicsPath;
		}
		if ((double)radius >= (double)Math.Min(baseRect.Width, baseRect.Height) / 2.0)
		{
			return GetCapsule(baseRect);
		}
		float num = radius * 2f;
		RectangleF rect = new RectangleF(size: new SizeF(num, num), location: baseRect.Location);
		GraphicsPath graphicsPath2 = new GraphicsPath();
		graphicsPath2.AddArc(rect, 180f, 90f);
		rect.X = baseRect.Right - num;
		graphicsPath2.AddArc(rect, 270f, 90f);
		rect.Y = baseRect.Bottom - num;
		graphicsPath2.AddArc(rect, 0f, 90f);
		rect.X = baseRect.Left;
		graphicsPath2.AddArc(rect, 90f, 90f);
		graphicsPath2.CloseFigure();
		return graphicsPath2;
	}

	private static GraphicsPath GetCapsule(RectangleF baseRect)
	{
		GraphicsPath graphicsPath = new GraphicsPath();
		try
		{
			if (baseRect.Width > baseRect.Height)
			{
				float height = baseRect.Height;
				RectangleF rect = new RectangleF(size: new SizeF(height, height), location: baseRect.Location);
				graphicsPath.AddArc(rect, 90f, 180f);
				rect.X = baseRect.Right - height;
				graphicsPath.AddArc(rect, 270f, 180f);
			}
			else if (baseRect.Width < baseRect.Height)
			{
				float height = baseRect.Width;
				RectangleF rect = new RectangleF(size: new SizeF(height, height), location: baseRect.Location);
				graphicsPath.AddArc(rect, 180f, 180f);
				rect.Y = baseRect.Bottom - height;
				graphicsPath.AddArc(rect, 0f, 180f);
			}
			else
			{
				graphicsPath.AddEllipse(baseRect);
			}
		}
		catch
		{
			graphicsPath.AddEllipse(baseRect);
		}
		finally
		{
			graphicsPath.CloseFigure();
		}
		return graphicsPath;
	}
}
