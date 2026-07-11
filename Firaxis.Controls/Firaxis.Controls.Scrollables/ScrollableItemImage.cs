using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using Firaxis.MathEx;
using Firaxis.Utility;

namespace Firaxis.Controls.Scrollables;

public class ScrollableItemImage : IScrollableItem, IDisposable
{
	private Image image;

	private Font font;

	private Font hotFont;

	private const int ItemPad = 5;

	private object tag;

	private bool visible;

	private string caption;

	private bool disposedValue = false;

	public int ItemHeight => ImageHeight + 10;

	public int ImageHeight { get; set; }

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

	public ScrollableItemImage(string caption, Image image, Font font)
	{
		this.caption = caption;
		this.image = (Image)image.Clone();
		this.font = font;
		hotFont = new Font(font, FontStyle.Bold);
		visible = true;
		ImageHeight = 32;
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!disposedValue)
		{
			if (disposing)
			{
				hotFont?.Dispose();
				image?.Dispose();
			}
			disposedValue = true;
		}
	}

	public void Dispose()
	{
		Dispose(disposing: true);
	}

	public void RefreshImage(Image image)
	{
		if (this.image != null)
		{
			this.image.Dispose();
		}
		this.image = (Image)image.Clone();
	}

	public void CalcLayout(Graphics g, SizeF size)
	{
	}

	private Vec2 ScaleSize(int cw, int ch, int dw, int dh)
	{
		Vec2 vec = default(Vec2);
		vec.X = (float)dw * 1f / (float)cw;
		vec.Y = (float)dh * 1f / (float)ch;
		float num = Math.Min(vec.X, vec.Y);
		Vec2 result = default(Vec2);
		result.X = (float)cw * num;
		result.Y = (float)ch * num;
		return result;
	}

	public void PaintItem(object sender, ScrollableItemPaintEventArgs e)
	{
		bool flag = e.State == ScrollableItemState.Selected;
		bool flag2 = (e.Style & ScrollableItemStyle.Hot) != 0;
		Graphics graphics = e.Graphics;
		Rectangle bounds = e.Bounds;
		using (Pen pen = new Pen(Color.FromArgb(79, 79, 79)))
		{
			pen.DashStyle = DashStyle.Dot;
			graphics.DrawLine(pen, bounds.X, bounds.Bottom, bounds.Right, bounds.Bottom);
		}
		if (flag)
		{
			using Brush brush = new SolidBrush(Color.FromArgb(51, 153, 255));
			graphics.FillRectangle(brush, bounds);
		}
		if (image != null)
		{
			Rectangle empty = Rectangle.Empty;
			empty.X = 5;
			empty.Y = bounds.Y + ItemHeight / 2 - ImageHeight / 2;
			empty.Width = ImageHeight;
			empty.Height = ImageHeight;
			DrawingHelper.DrawPSChecker(graphics, empty);
			Rectangle rect = empty;
			Vec2 vec = ScaleSize(image.Width, image.Height, ImageHeight, ImageHeight);
			rect.Width = (int)vec.X;
			rect.Height = (int)vec.Y;
			rect.X = empty.X + empty.Width / 2 - rect.Width / 2;
			rect.Y = empty.Y + empty.Height / 2 - rect.Height / 2;
			InterpolationMode interpolationMode = graphics.InterpolationMode;
			graphics.InterpolationMode = ((rect.Width < image.Width || rect.Height < image.Height) ? InterpolationMode.HighQualityBilinear : InterpolationMode.NearestNeighbor);
			graphics.DrawImage(image, rect);
			graphics.InterpolationMode = interpolationMode;
			using StringFormat stringFormat = new StringFormat();
			stringFormat.Alignment = StringAlignment.Far;
			stringFormat.LineAlignment = StringAlignment.Center;
			string s = $"{caption}\n({image.Width} x {image.Height})";
			graphics.DrawString(s, flag2 ? hotFont : font, flag2 ? SystemBrushes.HotTrack : (flag ? Brushes.White : Brushes.Black), bounds, stringFormat);
		}
	}
}
