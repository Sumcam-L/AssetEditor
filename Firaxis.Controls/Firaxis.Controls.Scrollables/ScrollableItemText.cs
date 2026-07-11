using System;
using System.ComponentModel;
using System.Drawing;
using Firaxis.Utility;

namespace Firaxis.Controls.Scrollables;

public class ScrollableItemText : IScrollableItem, IDisposable
{
	private string caption;

	private string text;

	private object tag;

	private bool visible;

	private Font boldFont;

	private Font font;

	private int capHeight;

	private int height;

	private bool showCaption;

	private bool hardLine;

	private const int pad = 2;

	private bool disposedValue = false;

	public int ItemHeight => height;

	public object Tag
	{
		get
		{
			return tag;
		}
		set
		{
			tag = value;
		}
	}

	public bool Visible
	{
		get
		{
			return visible;
		}
		set
		{
			visible = value;
		}
	}

	public Image Image { get; set; }

	public virtual string Caption
	{
		get
		{
			return caption;
		}
		set
		{
			caption = value;
		}
	}

	public virtual string Text
	{
		get
		{
			return text;
		}
		set
		{
			text = value;
		}
	}

	[Browsable(false)]
	public Color HighlightBackColor { get; set; }

	[Browsable(false)]
	public Color SeparatorColor { get; set; }

	[Browsable(false)]
	public Color HardSeparatorColor { get; set; }

	[Browsable(false)]
	public Color HighlightTextColor { get; set; }

	[Browsable(false)]
	public Color TextColor { get; set; }

	public virtual bool ShowCaption
	{
		get
		{
			return showCaption;
		}
		set
		{
			showCaption = value;
		}
	}

	public virtual bool ShowHardLine
	{
		get
		{
			return hardLine;
		}
		set
		{
			hardLine = value;
		}
	}

	public ScrollableItemText(string caption, string text, Font font)
	{
		this.caption = caption;
		this.text = text;
		this.font = font;
		visible = true;
		showCaption = true;
		hardLine = false;
		boldFont = new Font(font.FontFamily.Name, font.SizeInPoints, FontStyle.Bold, GraphicsUnit.Point);
	}

	public void CalcLayout(Graphics g, SizeF size)
	{
		SizeF sizeF = g.MeasureString(Text, font, size);
		if (ShowCaption)
		{
			SizeF sizeF2 = g.MeasureString(Caption, boldFont);
			capHeight = (int)sizeF2.Height;
			height = (int)(sizeF2.Height + sizeF.Height);
		}
		else
		{
			capHeight = 0;
			height = (int)sizeF.Height;
		}
		height += 4;
	}

	public void PaintItem(object sender, ScrollableItemPaintEventArgs e)
	{
		bool flag = e.State == ScrollableItemState.Selected;
		Graphics graphics = e.Graphics;
		Rectangle bounds = e.Bounds;
		if (flag)
		{
			using Brush brush = new SolidBrush(HighlightBackColor);
			graphics.FillRectangle(brush, bounds);
		}
		using (Pen pen = new Pen(ShowHardLine ? HardSeparatorColor : SeparatorColor))
		{
			graphics.DrawLine(pen, bounds.X, bounds.Bottom, bounds.Right, bounds.Bottom);
		}
		StringFormat stringFormat = new StringFormat();
		stringFormat.Trimming = StringTrimming.EllipsisCharacter;
		if (Image != null)
		{
			DrawingHelper.DrawImage(graphics, Image, bounds.X, bounds.Y + bounds.Height / 2 - Image.Height / 2);
			bounds.X += 2 + Image.Width;
		}
		using Brush brush2 = new SolidBrush(flag ? HighlightTextColor : TextColor);
		if (ShowCaption)
		{
			bounds.Y += 2;
			bounds.Height = capHeight;
			graphics.DrawString(Caption, boldFont, brush2, bounds, stringFormat);
		}
		bounds = e.Bounds;
		if (Image != null)
		{
			bounds.X += 2 + Image.Width;
		}
		bounds.Y += capHeight + 2;
		bounds.Height -= capHeight;
		graphics.DrawString(Text, font, brush2, bounds);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!disposedValue)
		{
			if (disposing)
			{
				boldFont?.Dispose();
			}
			disposedValue = true;
		}
	}

	public void Dispose()
	{
		Dispose(disposing: true);
	}
}
