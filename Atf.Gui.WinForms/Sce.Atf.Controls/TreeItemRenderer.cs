using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Sce.Atf.Controls;

public class TreeItemRenderer
{
	[Flags]
	public enum NodeFilteringStatus
	{
		Normal = 0,
		PartiallyExpanded = 1,
		Visible = 2,
		ChildVisible = 4
	}

	public Func<TreeControl.Node, NodeFilteringStatus> FilteringStatus;

	private Control m_owner;

	private readonly Dictionary<int, Font> m_fonts = new Dictionary<int, Font>();

	private Size m_checkBoxSize = new Size(16, 16);

	private Size m_expanderSize = new Size(8, 8);

	private Brush m_highlightBrush = SystemBrushes.Highlight;

	private Brush m_highlightTextBrush = SystemBrushes.HighlightText;

	private Brush m_deactiveHighlightBrush = SystemBrushes.Control;

	private Brush m_deactiveHighlightTextBrush = SystemBrushes.WindowText;

	private Brush m_textBrush = SystemBrushes.WindowText;

	private Pen m_expanderPen = SystemPens.ControlDarkDark;

	private Pen m_hierarchyLinePen = SystemPens.InactiveBorder;

	private SolidBrush m_brushMatchedHighLight = new SolidBrush(Color.FromArgb(239, 203, 5));

	private SolidBrush m_brushNonMatchedBg = new SolidBrush(Color.FromArgb(230, 230, 230));

	private Color m_categoryStartColor = Color.FromArgb(0, 0, 0, 0);

	private Color m_categoryEndColor = Color.FromArgb(0, 0, 0, 0);

	public Brush HighlightBrush
	{
		get
		{
			return m_highlightBrush;
		}
		set
		{
			m_highlightBrush = value;
		}
	}

	public Brush HighlightTextBrush
	{
		get
		{
			return m_highlightTextBrush;
		}
		set
		{
			m_highlightTextBrush = value;
		}
	}

	public Brush DeactiveHighlightBrush
	{
		get
		{
			return m_deactiveHighlightBrush;
		}
		set
		{
			m_deactiveHighlightBrush = value;
		}
	}

	public Brush DeactiveHighlightTextBrush
	{
		get
		{
			return m_deactiveHighlightTextBrush;
		}
		set
		{
			m_deactiveHighlightTextBrush = value;
		}
	}

	public Brush TextBrush
	{
		get
		{
			return m_textBrush;
		}
		set
		{
			m_textBrush = value;
		}
	}

	public Pen ExpanderPen
	{
		get
		{
			return m_expanderPen;
		}
		set
		{
			m_expanderPen = value;
		}
	}

	public Pen HierarchyLinePen
	{
		get
		{
			return m_hierarchyLinePen;
		}
		set
		{
			m_hierarchyLinePen = value;
		}
	}

	public Color CategoryStartColor
	{
		get
		{
			return m_categoryStartColor;
		}
		set
		{
			m_categoryStartColor = value;
		}
	}

	public Color CategoryEndColor
	{
		get
		{
			return m_categoryEndColor;
		}
		set
		{
			m_categoryEndColor = value;
		}
	}

	public string FilteringPattern { get; set; }

	public Size CheckBoxSize
	{
		get
		{
			return m_checkBoxSize;
		}
		set
		{
			if (m_checkBoxSize != value)
			{
				m_checkBoxSize = value;
				if (Owner != null)
				{
					Owner.Invalidate();
				}
			}
		}
	}

	public Size ExpanderSize
	{
		get
		{
			return m_expanderSize;
		}
		set
		{
			if (m_expanderSize != value)
			{
				m_expanderSize = value;
				if (Owner != null)
				{
					Owner.Invalidate();
				}
			}
		}
	}

	public Control Owner
	{
		get
		{
			return m_owner;
		}
		internal set
		{
			m_owner = value;
			m_fonts.Clear();
		}
	}

	public virtual Size MeasureLabel(TreeControl.Node node, Graphics g)
	{
		Font defaultFont = GetDefaultFont(node, g);
		return Size.Ceiling(g.MeasureString(node.Label, defaultFont));
	}

	private static SizeF MeasureDisplayStringWidth(Graphics graphics, string text, Font font)
	{
		if (string.IsNullOrEmpty(text))
		{
			return new SizeF(0f, 0f);
		}
		StringFormat stringFormat = new StringFormat();
		RectangleF layoutRect = new RectangleF(0f, 0f, 1600f, 1600f);
		CharacterRange[] measurableCharacterRanges = new CharacterRange[1]
		{
			new CharacterRange(0, text.Length)
		};
		stringFormat.SetMeasurableCharacterRanges(measurableCharacterRanges);
		Region[] array = graphics.MeasureCharacterRanges(text, font, layoutRect, stringFormat);
		layoutRect = array[0].GetBounds(graphics);
		layoutRect.Width += 1f;
		return new SizeF(layoutRect.Width, layoutRect.Height);
	}

	public virtual void DrawLabel(TreeControl.Node node, Graphics g, int x, int y, int fullWidth)
	{
		Rectangle rect = new Rectangle(0, y, fullWidth, node.LabelHeight);
		Rectangle rectangle = new Rectangle(x, y, node.LabelWidth, node.LabelHeight);
		Brush brush = m_textBrush;
		Font defaultFont = GetDefaultFont(node, g);
		if (!string.IsNullOrEmpty(FilteringPattern))
		{
			int num = 0;
			PointF location = new PointF(rectangle.X, rectangle.Y);
			int num2;
			do
			{
				num2 = node.Label.IndexOf(FilteringPattern, num, StringComparison.CurrentCultureIgnoreCase);
				if (num2 >= 0)
				{
					string text = node.Label.Substring(num, num2 - num);
					SizeF sizeF = MeasureDisplayStringWidth(g, text, defaultFont);
					location.X += sizeF.Width;
					num = num2 + FilteringPattern.Length;
					string text2 = node.Label.Substring(num2, FilteringPattern.Length);
					SizeF size = MeasureDisplayStringWidth(g, text2, defaultFont);
					RectangleF rect2 = new RectangleF(location, size);
					rect2.X += 2f;
					rect2.Width -= 2f;
					g.FillRectangle(m_brushMatchedHighLight, rect2);
					location.X += size.Width;
				}
			}
			while (num2 >= 0);
		}
		if (node.Selected)
		{
			Brush brush2 = (Owner.ContainsFocus ? HighlightBrush : DeactiveHighlightBrush);
			Brush brush3 = (Owner.ContainsFocus ? HighlightTextBrush : DeactiveHighlightTextBrush);
			g.FillRectangle(brush2, rect);
			brush = brush3;
		}
		g.DrawString(node.Label, defaultFont, brush, rectangle);
	}

	public virtual void DrawData(TreeControl.Node node, Graphics g, int x, int y)
	{
	}

	public virtual void DrawBackground(TreeControl.Node node, Graphics g, int x, int y)
	{
		if (NeedGrayBackground(node))
		{
			Rectangle rect = new Rectangle(Owner.Margin.Left, Owner.Margin.Top + y, Owner.Width - Owner.Margin.Left - Owner.Margin.Right, node.LabelHeight + Owner.Margin.Top + Owner.Margin.Bottom);
			rect.Y -= 3;
			g.FillRectangle(m_brushNonMatchedBg, rect);
		}
	}

	public virtual void DrawCheckBox(TreeControl.Node node, Graphics g, int x, int y)
	{
		Rectangle rectangle = new Rectangle(x, y, CheckBoxSize.Width, CheckBoxSize.Height);
		if (node.CheckState == CheckState.Indeterminate)
		{
			Rectangle rect = Rectangle.Inflate(rectangle, -1, -1);
			g.FillRectangle(SystemBrushes.Window, rect);
			Rectangle rect2 = Rectangle.Inflate(rectangle, -4, -4);
			g.FillRectangle(SystemBrushes.ControlText, rect2);
			g.DrawRectangle(SystemPens.ControlDark, rect.X, rect.Y, rect.Width - 1, rect.Height - 1);
		}
		else
		{
			ButtonState buttonState = ButtonState.Flat;
			if (node.CheckState == CheckState.Checked)
			{
				buttonState |= ButtonState.Checked;
			}
			ControlPaint.DrawCheckBox(g, rectangle, buttonState);
		}
	}

	public virtual void DrawExpander(TreeControl.Node node, Graphics g, int x, int y)
	{
		node.PartiallyExpanded = false;
		GdiUtil.DrawExpander(x, y, ExpanderSize.Height, node.Selected ? HighlightTextBrush : TextBrush, node.Expanded, g);
	}

	public virtual void DrawCategory(TreeControl.Node node, Graphics g, Rectangle r)
	{
		Color color = ((CategoryStartColor.A == 0) ? ColorUtil.GetShade(Owner.BackColor, 0.97f) : CategoryStartColor);
		Color color2 = ((CategoryEndColor.A == 0) ? ColorUtil.GetShade(CategoryStartColor, 0.9f) : CategoryEndColor);
		using (LinearGradientBrush brush = new LinearGradientBrush(r, color, color2, LinearGradientMode.Vertical))
		{
			g.FillRectangle(brush, r);
		}
		Padding margin = Owner.Margin;
		int left = margin.Left;
		int top = margin.Top;
		GdiUtil.DrawOfficeExpander(r.Width - ExpanderSize.Width - left, r.Y + top, ExpanderPen, !node.Expanded, g);
	}

	public virtual void DrawHierarchyLine(Graphics g, Point a, Point b)
	{
		g.DrawLine(HierarchyLinePen, a.X, a.Y, b.X, b.Y);
	}

	public virtual void DrawImage(ImageList imageList, Graphics g, int x, int y, int index)
	{
		if (Owner == null || Owner.Enabled)
		{
			using (Image image = imageList.Images[index])
			{
				g.DrawImage(image, x, y);
				return;
			}
		}
		using Image image2 = imageList.Images[index];
		ControlPaint.DrawImageDisabled(g, image2, x, y, Owner.BackColor);
	}

	protected virtual Font GetDefaultFont(TreeControl.Node node, Graphics g)
	{
		Font value = Owner.Font;
		if (node.FontStyle != FontStyle.Regular)
		{
			int fontStyle = (int)node.FontStyle;
			if (!m_fonts.TryGetValue(fontStyle, out value))
			{
				value = new Font(Owner.Font, node.FontStyle);
				m_fonts.Add(fontStyle, value);
			}
		}
		return value;
	}

	private bool NeedGrayBackground(TreeControl.Node node)
	{
		if (string.IsNullOrEmpty(FilteringPattern))
		{
			return false;
		}
		return FilteringStatus == null || (FilteringStatus(node) & NodeFilteringStatus.Visible) == 0;
	}
}
