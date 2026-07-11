using System;
using System.ComponentModel;
using System.Drawing;
using Firaxis.Controls.Properties;
using Firaxis.Utility;

namespace Firaxis.Controls.Scrollables;

public class ScrollableItemTree : IScrollableItem, IDisposable, IScrollableItemTree
{
	private Font font;

	private int height;

	private const int indent_x = 16;

	private const int pad_x = 2;

	private Rectangle expand = Rectangle.Empty;

	private bool disposedValue = false;

	[Browsable(false)]
	public virtual int ItemHeight => height;

	[Browsable(false)]
	public Font Font => font;

	[Browsable(false)]
	public bool ShowSeparator { get; set; }

	[Browsable(false)]
	public virtual Image Image { get; set; }

	[Browsable(false)]
	public object Tag { get; set; }

	[Browsable(false)]
	public bool Visible { get; set; }

	[Browsable(false)]
	public virtual string Text { get; set; }

	[Browsable(false)]
	public Color SeparatorColor { get; set; }

	[Browsable(false)]
	public Color HardSeparatorColor { get; set; }

	[Browsable(false)]
	public Color HighlightedBackColor { get; set; }

	[Browsable(false)]
	public Color HighlightedTextColor { get; set; }

	[Browsable(false)]
	public Color HoverTextColor { get; set; }

	[Browsable(false)]
	public Color TextColor { get; set; }

	public ScrollableItemTree(Font font)
	{
		this.font = font;
		ShowSeparator = true;
	}

	public override string ToString()
	{
		return Text;
	}

	public ScrollableItemTree(string text, Font font)
	{
		this.font = font;
		ShowSeparator = true;
		Text = text;
		SeparatorColor = Color.FromArgb(186, 182, 169);
		HighlightedBackColor = Color.FromArgb(62, 128, 208);
		HighlightedTextColor = Color.White;
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!disposedValue)
		{
			if (disposing)
			{
			}
			disposedValue = true;
		}
	}

	public void Dispose()
	{
		Dispose(disposing: true);
	}

	public virtual void CalcLayout(Graphics g, SizeF size)
	{
		height = Math.Max(19, font.Height);
	}

	public virtual void PaintItem(object sender, ScrollableItemPaintEventArgs e)
	{
		bool flag = e.State == ScrollableItemState.Selected;
		Graphics graphics = e.Graphics;
		Rectangle bounds = e.Bounds;
		if (flag)
		{
			using Brush brush = new SolidBrush(HighlightedBackColor);
			graphics.FillRectangle(brush, e.Bounds);
		}
		if (ShowSeparator)
		{
			using Pen pen = new Pen(SeparatorColor);
			graphics.DrawLine(pen, bounds.X, bounds.Bottom, bounds.Right, bounds.Bottom);
		}
		bounds.X = 2 + 16 * e.Level;
		Image styleImage = GetStyleImage(e.Style);
		if (styleImage != null)
		{
			DrawingHelper.DrawImage(graphics, styleImage, bounds.X + 8 - styleImage.Width / 2, bounds.Y + bounds.Height / 2 - styleImage.Height / 2, flag ? HighlightedTextColor : TextColor);
			expand.X = bounds.X + 8 - styleImage.Width / 2;
			expand.Y = bounds.Y + bounds.Height / 2 - styleImage.Height / 2;
			expand.Width = 16;
			expand.Height = 16;
		}
		else
		{
			expand = Rectangle.Empty;
		}
		bounds.X += 18;
		if (Image != null)
		{
			DrawingHelper.DrawImage(graphics, Image, bounds.X + 8 - Image.Width / 2, bounds.Y + bounds.Height / 2 - Image.Height / 2);
			bounds.X += 18;
		}
		using StringFormat stringFormat = new StringFormat();
		stringFormat.LineAlignment = StringAlignment.Center;
		stringFormat.FormatFlags |= StringFormatFlags.NoWrap;
		using Brush brush2 = new SolidBrush(flag ? HighlightedTextColor : TextColor);
		graphics.DrawString(Text, font, brush2, bounds, stringFormat);
	}

	private Image GetStyleImage(ScrollableItemStyle style)
	{
		Image result = null;
		switch (style)
		{
		case ScrollableItemStyle.Expanded:
			result = Firaxis.Controls.Properties.Resources.tl_tree_open;
			break;
		case ScrollableItemStyle.Collapsed:
			result = Firaxis.Controls.Properties.Resources.tl_tree_close;
			break;
		}
		return result;
	}

	public bool HitExpand(int x, int y)
	{
		return x >= expand.X && x < expand.X + expand.Width;
	}
}
