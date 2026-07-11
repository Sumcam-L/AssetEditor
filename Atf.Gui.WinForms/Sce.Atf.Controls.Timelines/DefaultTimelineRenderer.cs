using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;

namespace Sce.Atf.Controls.Timelines;

public class DefaultTimelineRenderer : TimelineRenderer, IDisposable
{
	private readonly Pen m_selectedPen;

	private readonly Brush m_collapsedBrush;

	private readonly Brush m_invalidBrush;

	private float m_trackHeight = 1f;

	private int m_keySize = 12;

	private int m_minimumDrawnIntervalLength = 28;

	private static readonly StringFormat s_leftBottomFormat;

	[DefaultValue(1)]
	public float TrackHeight
	{
		get
		{
			return m_trackHeight;
		}
		set
		{
			if (m_trackHeight != value)
			{
				m_trackHeight = value;
				OnInvalidated(EventArgs.Empty);
			}
		}
	}

	[DefaultValue(12)]
	public int KeySize
	{
		get
		{
			return m_keySize;
		}
		set
		{
			if (m_keySize != value)
			{
				m_keySize = value;
				OnInvalidated(EventArgs.Empty);
			}
		}
	}

	[DefaultValue(28)]
	public int MinimumDrawnIntervalLength
	{
		get
		{
			return m_minimumDrawnIntervalLength;
		}
		set
		{
			m_minimumDrawnIntervalLength = value;
		}
	}

	public DefaultTimelineRenderer()
		: this(SystemFonts.StatusFont)
	{
	}

	public DefaultTimelineRenderer(Font font)
		: base(font)
	{
		m_selectedPen = new Pen(Color.Tomato, 3f);
		Color lightGray = Color.LightGray;
		m_collapsedBrush = new SolidBrush(lightGray);
		m_invalidBrush = new HatchBrush(HatchStyle.ForwardDiagonal, Color.DimGray, Color.FromArgb(0, 0, 0, 0));
	}

	protected override void Dispose(bool disposing)
	{
		m_selectedPen.Dispose();
		m_collapsedBrush.Dispose();
		m_invalidBrush.Dispose();
		base.Dispose(disposing);
	}

	protected override void Draw(IGroup group, RectangleF bounds, DrawMode drawMode, Context c)
	{
		switch (drawMode & DrawMode.States)
		{
		case DrawMode.Normal:
		case DrawMode.Collapsed:
		{
			using Brush brush2 = new LinearGradientBrush(bounds, Color.LightGoldenrodYellow, Color.Khaki, LinearGradientMode.Vertical);
			c.Graphics.FillRectangle(brush2, bounds);
			break;
		}
		case DrawMode.Ghost:
		{
			using Brush brush = new SolidBrush(Color.FromArgb(128, Color.Gray));
			c.Graphics.FillRectangle(brush, bounds);
			break;
		}
		case DrawMode.Normal | DrawMode.Collapsed:
			break;
		}
	}

	protected override void Draw(ITrack track, RectangleF bounds, DrawMode drawMode, Context c)
	{
		switch (drawMode & DrawMode.States)
		{
		case DrawMode.Normal:
			c.Graphics.DrawRectangle(Pens.LightGray, bounds.X, bounds.Y, bounds.Width, bounds.Height);
			break;
		case DrawMode.Collapsed:
			break;
		case DrawMode.Ghost:
		{
			using Brush brush = new SolidBrush(Color.FromArgb(128, Color.Gray));
			c.Graphics.FillRectangle(brush, bounds);
			break;
		}
		case DrawMode.Normal | DrawMode.Collapsed:
			break;
		}
	}

	protected override void Draw(IInterval interval, RectangleF bounds, DrawMode drawMode, Context c)
	{
		Color color = interval.Color;
		switch (drawMode & DrawMode.States)
		{
		case DrawMode.Normal:
		{
			RectangleF rect = new RectangleF(bounds.X, bounds.Y, GdiUtil.TransformVector(c.Transform, interval.Length), bounds.Height);
			bool flag = rect.Width < (float)MinimumDrawnIntervalLength;
			float hue = color.GetHue();
			float saturation = color.GetSaturation();
			float brightness = color.GetBrightness();
			Color color2 = ColorUtil.FromAhsb(color.A, hue, saturation * 0.3f, brightness);
			using (LinearGradientBrush linearGradientBrush = new LinearGradientBrush(rect, color, color2, LinearGradientMode.Vertical))
			{
				c.Graphics.FillRectangle(linearGradientBrush, rect);
				if (flag)
				{
					Color[] linearColors = linearGradientBrush.LinearColors;
					linearColors[0] = Color.FromArgb(64, linearColors[0]);
					linearColors[1] = Color.FromArgb(64, linearColors[1]);
					linearGradientBrush.LinearColors = linearColors;
					RectangleF rect2 = new RectangleF(rect.Right, bounds.Y, bounds.Width - rect.Width, bounds.Height);
					c.Graphics.FillRectangle(linearGradientBrush, rect2);
				}
			}
			Brush brush2 = SystemBrushes.WindowText;
			if (color.R + color.G + color.B < 480)
			{
				brush2 = SystemBrushes.HighlightText;
			}
			c.Graphics.DrawString(interval.Name, c.Font, brush2, bounds.Location);
			if ((drawMode & DrawMode.Selected) != 0)
			{
				c.Graphics.DrawRectangle(m_selectedPen, bounds.X + 1f, bounds.Y + 1f, bounds.Width - 2f, bounds.Height - 2f);
			}
			break;
		}
		case DrawMode.Collapsed:
			c.Graphics.FillRectangle(m_collapsedBrush, bounds);
			break;
		case DrawMode.Ghost:
		{
			using Brush brush = new SolidBrush(Color.FromArgb(128, color));
			c.Graphics.FillRectangle(brush, bounds);
			float x = (((drawMode & DrawMode.ResizeRight) != 0) ? bounds.Right : bounds.Left);
			c.Graphics.DrawString(GetXPositionString(x, c), c.Font, SystemBrushes.WindowText, x, bounds.Bottom - c.FontHeight);
			break;
		}
		case DrawMode.Invalid:
			c.Graphics.FillRectangle(m_invalidBrush, bounds);
			break;
		}
	}

	protected override void Draw(IKey key, RectangleF bounds, DrawMode drawMode, Context c)
	{
		Color color = key.Color;
		float width = (bounds.Height = m_keySize);
		bounds.Width = width;
		switch (drawMode & DrawMode.States)
		{
		case DrawMode.Normal:
		{
			using (SolidBrush brush2 = new SolidBrush(color))
			{
				c.Graphics.FillEllipse(brush2, bounds);
			}
			if ((drawMode & DrawMode.Selected) != 0)
			{
				c.Graphics.DrawEllipse(m_selectedPen, bounds.X + 1f, bounds.Y + 1f, bounds.Width - 2f, bounds.Height - 2f);
			}
			break;
		}
		case DrawMode.Collapsed:
			c.Graphics.FillEllipse(m_collapsedBrush, bounds);
			break;
		case DrawMode.Ghost:
		{
			using Brush brush = new SolidBrush(Color.FromArgb(128, color));
			c.Graphics.FillEllipse(brush, bounds);
			c.Graphics.DrawString(GetXPositionString(bounds.Left + (float)(m_keySize / 2), c), c.Font, SystemBrushes.WindowText, bounds.Right + 16f, bounds.Y);
			break;
		}
		case DrawMode.Invalid:
			c.Graphics.FillEllipse(m_invalidBrush, bounds);
			break;
		}
	}

	protected override void Draw(IMarker marker, RectangleF bounds, DrawMode drawMode, Context c)
	{
		float num = bounds.X + bounds.Width / 2f;
		Color color = marker.Color;
		switch (drawMode & DrawMode.States)
		{
		case DrawMode.Normal:
		{
			using (Pen pen2 = new Pen(color))
			{
				c.Graphics.DrawLine(pen2, num, bounds.Top, num, bounds.Bottom);
			}
			Color color2 = (((drawMode & DrawMode.Selected) != 0) ? Color.Tomato : color);
			using Brush brush = new SolidBrush(color2);
			RectangleF rect = new RectangleF(bounds.X, bounds.Y, bounds.Width, bounds.Width);
			c.Graphics.FillRectangle(brush, rect);
			break;
		}
		case DrawMode.Collapsed:
			c.Graphics.FillRectangle(m_collapsedBrush, num, bounds.Y, 1f, bounds.Height);
			break;
		case DrawMode.Ghost:
		{
			using Pen pen = new Pen(Color.FromArgb(128, color));
			c.Graphics.DrawLine(pen, num, bounds.Top, num, bounds.Bottom);
			c.Graphics.DrawString(GetXPositionString(num, c), c.Font, SystemBrushes.WindowText, bounds.Right + 16f, bounds.Y);
			break;
		}
		case DrawMode.Invalid:
			c.Graphics.DrawRectangle(Pens.DimGray, bounds.X, bounds.Y, bounds.Width, bounds.Height);
			break;
		}
	}

	protected override HitType Pick(IMarker marker, RectangleF bounds, RectangleF pickRect, Context c)
	{
		return new RectangleF(bounds.X, bounds.Y, bounds.Width, bounds.Width).IntersectsWith(pickRect) ? HitType.Marker : HitType.None;
	}

	protected override RectangleF GetBounds(IInterval interval, float trackTop, Context c)
	{
		float num = interval.Length;
		if (interval.Track != null && interval.Track.Group != null && interval.Track.Group.Expanded)
		{
			float val = c.PixelSize.Width * (float)MinimumDrawnIntervalLength;
			num = Math.Max(num, val);
		}
		return new RectangleF(interval.Start, trackTop, num, TrackHeight);
	}

	protected override RectangleF GetBounds(IKey key, float trackTop, Context c)
	{
		float num = c.PixelSize.Width * (float)KeySize;
		float height = c.PixelSize.Height * (float)KeySize;
		return new RectangleF(key.Start - num / 2f, trackTop, num, height);
	}

	protected override RectangleF GetBounds(IMarker marker, Context c)
	{
		float num = c.PixelSize.Width * 10f;
		return new RectangleF(marker.Start - num / 2f, c.Bounds.Top, num, c.Bounds.Height);
	}

	protected virtual string GetXPositionString(float x, Context c)
	{
		float num = x * c.InverseTransform.Elements[0] + c.InverseTransform.Elements[4];
		return ((float)Math.Round(num)).ToString(CultureInfo.CurrentCulture);
	}

	static DefaultTimelineRenderer()
	{
		s_leftBottomFormat = new StringFormat();
		s_leftBottomFormat.Alignment = StringAlignment.Near;
		s_leftBottomFormat.LineAlignment = StringAlignment.Far;
	}
}
