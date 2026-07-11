using System;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using Sce.Atf.Direct2D;

namespace Sce.Atf.Controls.Timelines.Direct2D;

public class D2dDefaultTimelineRenderer : D2dTimelineRenderer, IDisposable
{
	protected D2dBrush SelectedBrush;

	protected D2dBrush CollapsedBrush;

	protected D2dBrush InvalidBrush;

	protected D2dLinearGradientBrush GroupBrush;

	protected D2dBrush GhostGroupBrush;

	protected D2dBrush TrackBrush;

	protected D2dBrush GhostTrackBrush;

	protected D2dSolidColorBrush TextBrush;

	private bool m_disposed;

	private int m_minimumDrawnIntervalLength = 28;

	private static readonly StringFormat s_leftBottomFormat;

	[DefaultValue(1)]
	public virtual float TrackHeight
	{
		get
		{
			return D2dTimelineRenderer.GlobalTrackHeight;
		}
		set
		{
			if (D2dTimelineRenderer.GlobalTrackHeight != value)
			{
				D2dTimelineRenderer.GlobalTrackHeight = value;
				OnInvalidated(EventArgs.Empty);
			}
		}
	}

	[DefaultValue(12)]
	public virtual int KeySize
	{
		get
		{
			return D2dTimelineRenderer.GlobalKeySize;
		}
		set
		{
			if (D2dTimelineRenderer.GlobalKeySize != value)
			{
				D2dTimelineRenderer.GlobalKeySize = value;
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

	public D2dDefaultTimelineRenderer()
		: this(SystemFonts.StatusFont)
	{
	}

	public D2dDefaultTimelineRenderer(Font font)
		: base(font)
	{
	}

	public override void Init(D2dGraphics graphics)
	{
		base.Init(graphics);
		SelectedBrush = graphics.CreateSolidBrush(Color.Tomato);
		CollapsedBrush = graphics.CreateSolidBrush(Color.LightGray);
		InvalidBrush = graphics.CreateSolidBrush(Color.DimGray);
		GroupBrush = graphics.CreateLinearGradientBrush(new D2dGradientStop(Color.LightGoldenrodYellow, 0f), new D2dGradientStop(Color.Khaki, 1f));
		GhostGroupBrush = graphics.CreateSolidBrush(Color.FromArgb(128, Color.Gray));
		TrackBrush = graphics.CreateSolidBrush(Color.LightGray);
		GhostTrackBrush = graphics.CreateSolidBrush(Color.FromArgb(128, Color.Gray));
		TextBrush = graphics.CreateSolidBrush(SystemColors.WindowText);
	}

	protected override void Dispose(bool disposing)
	{
		if (!m_disposed)
		{
			if (disposing)
			{
				SelectedBrush.Dispose();
				CollapsedBrush.Dispose();
				InvalidBrush.Dispose();
				GroupBrush.Dispose();
				GhostGroupBrush.Dispose();
				TrackBrush.Dispose();
				GhostTrackBrush.Dispose();
				TextBrush.Dispose();
			}
			m_disposed = true;
		}
		base.Dispose(disposing);
	}

	protected override void Draw(IGroup group, RectangleF bounds, DrawMode drawMode, Context c)
	{
		switch (drawMode & DrawMode.States)
		{
		case DrawMode.Normal:
		case DrawMode.Collapsed:
			GroupBrush.StartPoint = new PointF(0f, bounds.Top);
			GroupBrush.EndPoint = new PointF(0f, bounds.Bottom);
			c.Graphics.FillRectangle(bounds, GroupBrush);
			break;
		case DrawMode.Ghost:
			c.Graphics.FillRectangle(bounds, GhostGroupBrush);
			break;
		case DrawMode.Normal | DrawMode.Collapsed:
			break;
		}
	}

	protected override void Draw(ITrack track, RectangleF bounds, DrawMode drawMode, Context c)
	{
		switch (drawMode & DrawMode.States)
		{
		case DrawMode.Normal:
			c.Graphics.DrawRectangle(bounds, TrackBrush);
			break;
		case DrawMode.Collapsed:
			break;
		case DrawMode.Ghost:
			c.Graphics.FillRectangle(bounds, GhostTrackBrush);
			break;
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
			c.Graphics.FillRectangle(rect, new PointF(0f, rect.Top), new PointF(0f, rect.Bottom), color, color2);
			if (flag)
			{
				color2 = ColorUtil.FromAhsb(64, hue, saturation * 0.3f, brightness);
				RectangleF rect2 = new RectangleF(rect.Right, bounds.Y, bounds.Width - rect.Width, bounds.Height);
				c.Graphics.FillRectangle(rect2, color2);
			}
			if (color.R + color.G + color.B < 480)
			{
				TextBrush.Color = SystemColors.HighlightText;
			}
			else
			{
				TextBrush.Color = SystemColors.WindowText;
			}
			c.Graphics.DrawText(interval.Name, c.TextFormat, bounds.Location, TextBrush);
			if ((drawMode & DrawMode.Selected) != 0)
			{
				c.Graphics.DrawRectangle(new RectangleF(bounds.X + 1f, bounds.Y + 1f, bounds.Width - 2f, bounds.Height - 2f), SelectedBrush, 3f);
			}
			break;
		}
		case DrawMode.Collapsed:
			c.Graphics.FillRectangle(bounds, CollapsedBrush);
			break;
		case DrawMode.Ghost:
		{
			c.Graphics.FillRectangle(bounds, Color.FromArgb(128, color));
			float x = (((drawMode & DrawMode.ResizeRight) != 0) ? bounds.Right : bounds.Left);
			c.Graphics.DrawText(GetXPositionString(x, c), c.TextFormat, new PointF(x, bounds.Bottom - c.FontHeight), TextBrush);
			break;
		}
		case DrawMode.Invalid:
			c.Graphics.FillRectangle(bounds, InvalidBrush);
			break;
		}
	}

	protected override void Draw(IKey key, RectangleF bounds, DrawMode drawMode, Context c)
	{
		Color color = key.Color;
		float width = (bounds.Height = KeySize);
		bounds.Width = width;
		switch (drawMode & DrawMode.States)
		{
		case DrawMode.Normal:
			c.Graphics.FillEllipse(bounds, color);
			if ((drawMode & DrawMode.Selected) != 0)
			{
				D2dAntialiasMode antialiasMode = c.Graphics.AntialiasMode;
				c.Graphics.AntialiasMode = D2dAntialiasMode.PerPrimitive;
				c.Graphics.DrawEllipse(new D2dEllipse(new PointF(bounds.X + bounds.Width * 0.5f, bounds.Y + bounds.Height * 0.5f), bounds.Width * 0.5f, bounds.Height * 0.5f), SelectedBrush, 3f);
				c.Graphics.AntialiasMode = antialiasMode;
			}
			break;
		case DrawMode.Collapsed:
			c.Graphics.FillEllipse(bounds, CollapsedBrush);
			break;
		case DrawMode.Ghost:
			c.Graphics.FillEllipse(bounds, Color.FromArgb(128, color));
			c.Graphics.DrawText(GetXPositionString(bounds.Left + (float)KeySize * 0.5f, c), c.TextFormat, new PointF(bounds.Right + 16f, bounds.Y), TextBrush);
			break;
		case DrawMode.Invalid:
			c.Graphics.FillEllipse(bounds, InvalidBrush);
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
			c.Graphics.DrawLine(num, bounds.Top, num, bounds.Bottom, color);
			Color color2 = (((drawMode & DrawMode.Selected) != 0) ? Color.Tomato : color);
			RectangleF rect = new RectangleF(bounds.X, bounds.Y, bounds.Width, bounds.Width);
			c.Graphics.FillRectangle(rect, color2);
			break;
		}
		case DrawMode.Collapsed:
			c.Graphics.FillRectangle(new RectangleF(num, bounds.Y, 1f, bounds.Height), CollapsedBrush);
			break;
		case DrawMode.Ghost:
			c.Graphics.DrawLine(num, bounds.Top, num, bounds.Bottom, Color.FromArgb(128, color));
			c.Graphics.DrawText(GetXPositionString(num, c), c.TextFormat, new PointF(bounds.Right + 16f, bounds.Y), TextBrush);
			break;
		case DrawMode.Invalid:
			c.Graphics.DrawRectangle(bounds, Color.DimGray);
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

	static D2dDefaultTimelineRenderer()
	{
		s_leftBottomFormat = new StringFormat();
		s_leftBottomFormat.Alignment = StringAlignment.Near;
		s_leftBottomFormat.LineAlignment = StringAlignment.Far;
	}
}
