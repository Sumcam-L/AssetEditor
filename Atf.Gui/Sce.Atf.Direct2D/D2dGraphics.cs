using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using Sce.Atf.Adaptation;
using Sce.Atf.Controls.Adaptable.Graphs;
using Sce.Atf.VectorMath;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.DirectWrite;

namespace Sce.Atf.Direct2D;

public abstract class D2dGraphics : D2dResource
{
	private uint m_renderTargetNumber;

	private static readonly Result D2DERR_WRONG_RESOURCE_DOMAIN = new Result(2291728405u);

	private static readonly Result D2DERR_RECREATE_TARGET = new Result(2291728396u);

	private const double DPI = Math.PI;

	private const float ToRadian = (float)Math.PI / 180f;

	private readonly List<PointF> m_tempPoints = new List<PointF>(100);

	private PointF m_scale = new PointF(1f, 1f);

	private const float Tessellation = 4f;

	private GdiInteropRenderTarget m_gdiInterop;

	private Graphics m_graphics;

	private RenderTarget m_renderTarget;

	private Matrix3x2F m_xform = Matrix3x2F.Identity;

	private D2dSolidColorBrush m_solidColorBrush;

	private readonly Stack<System.Drawing.RectangleF> m_clipStack = new Stack<System.Drawing.RectangleF>();

	private readonly Dictionary<long, SharpDX.Direct2D1.LinearGradientBrush> m_linearGradients = new Dictionary<long, SharpDX.Direct2D1.LinearGradientBrush>();

	private static D2dStrokeStyle s_strokeStyle;

	public Matrix3x2F Transform
	{
		get
		{
			return m_xform;
		}
		set
		{
			if (!m_xform.Equals(value))
			{
				m_xform = value;
				Matrix3x2 transform = default(Matrix3x2);
				transform.M11 = m_xform.M11;
				transform.M12 = m_xform.M12;
				transform.M21 = m_xform.M21;
				transform.M22 = m_xform.M22;
				transform.M31 = m_xform.DX;
				transform.M32 = m_xform.DY;
				m_renderTarget.Transform = transform;
				Vec2F vec2F = new Vec2F(transform.M11, transform.M12);
				Vec2F vec2F2 = new Vec2F(transform.M21, transform.M22);
				m_scale = new PointF(vec2F.Length, vec2F2.Length);
				if (m_graphics != null)
				{
					ApplyXformToGdi();
				}
			}
		}
	}

	public D2dAntialiasMode AntialiasMode
	{
		get
		{
			return (D2dAntialiasMode)m_renderTarget.AntialiasMode;
		}
		set
		{
			m_renderTarget.AntialiasMode = (AntialiasMode)value;
		}
	}

	public Size PixelSize => new Size(m_renderTarget.PixelSize.Width, m_renderTarget.PixelSize.Height);

	public SizeF Size => new SizeF(m_renderTarget.Size.Width, m_renderTarget.Size.Height);

	public D2dTextAntialiasMode TextAntialiasMode
	{
		get
		{
			return (D2dTextAntialiasMode)m_renderTarget.TextAntialiasMode;
		}
		set
		{
			m_renderTarget.TextAntialiasMode = (TextAntialiasMode)value;
		}
	}

	public SizeF DotsPerInch
	{
		get
		{
			return new SizeF(m_renderTarget.DotsPerInch.Width, m_renderTarget.DotsPerInch.Height);
		}
		set
		{
			m_renderTarget.DotsPerInch = value.ToSharpDX();
		}
	}

	public Graphics Graphics
	{
		get
		{
			if (m_graphics == null)
			{
				throw new InvalidOperationException("Graphics only valid when called between BeginGDISection() and EndGDISection()");
			}
			return m_graphics;
		}
	}

	public System.Drawing.RectangleF ClipBounds
	{
		get
		{
			if (m_clipStack.Count > 0)
			{
				return m_clipStack.Peek();
			}
			return new System.Drawing.RectangleF(default(PointF), Size);
		}
	}

	public uint RenderTargetNumber => m_renderTargetNumber;

	protected internal RenderTarget D2dRenderTarget => m_renderTarget;

	public event EventHandler RecreateResources;

	public void TranslateTransform(float dx, float dy)
	{
		Matrix3x2F matrix3x2F = Matrix3x2F.CreateTranslation(dx, dy);
		Matrix3x2F transform = Transform;
		Transform = matrix3x2F * transform;
	}

	public void ScaleTransform(float sx, float sy)
	{
		Matrix3x2F matrix3x2F = Matrix3x2F.CreateScale(sx, sy);
		Matrix3x2F transform = Transform;
		Transform = matrix3x2F * transform;
	}

	public void RotateTransform(float angle)
	{
		Matrix3x2F matrix3x2F = Matrix3x2F.CreateRotation(angle);
		Matrix3x2F transform = Transform;
		Transform = matrix3x2F * transform;
	}

	public void Clear(System.Drawing.Color color)
	{
		m_renderTarget.Clear(color.ToColor4());
	}

	public void BeginDraw()
	{
		D2dFactory.CheckForRecreateTarget();
		m_renderTarget.BeginDraw();
	}

	public D2dResult EndDraw()
	{
		try
		{
			m_renderTarget.EndDraw();
		}
		catch (SharpDXException ex)
		{
			if (!(ex.ResultCode == D2DERR_RECREATE_TARGET) && !(ex.ResultCode == D2DERR_WRONG_RESOURCE_DOMAIN))
			{
				while (m_clipStack.Count > 0)
				{
					PopAxisAlignedClip();
				}
				try
				{
					m_renderTarget.Flush();
				}
				catch
				{
				}
				throw;
			}
			D2dFactory.CheckForRecreateTarget();
			RecreateTargetAndResources();
		}
		return D2dResult.Ok;
	}

	public void DrawArc(D2dEllipse ellipse, D2dBrush brush, float startAngle, float sweepAngle, float strokeWidth = 1f, D2dStrokeStyle strokeStyle = null)
	{
		float num = 4f / m_scale.X;
		float num2 = startAngle * ((float)Math.PI / 180f);
		float num3 = (startAngle + sweepAngle) * ((float)Math.PI / 180f);
		if (num2 > num3)
		{
			float num4 = num2;
			num2 = num3;
			num3 = num4;
		}
		float x = ellipse.Center.X;
		float y = ellipse.Center.Y;
		Vec2F vec2F = new Vec2F
		{
			X = ellipse.RadiusX * (float)Math.Cos(num2),
			Y = ellipse.RadiusY * (float)Math.Sin(num2)
		};
		float length = (new Vec2F
		{
			X = ellipse.RadiusX * (float)Math.Cos(num3),
			Y = ellipse.RadiusY * (float)Math.Sin(num3)
		} - vec2F).Length;
		float num5 = length / num;
		float num6 = (num3 - num2) / num5;
		m_tempPoints.Clear();
		for (float num7 = num2; num7 < num3; num7 += num6)
		{
			PointF item = new PointF
			{
				X = x + ellipse.RadiusX * (float)Math.Cos(num7),
				Y = y + ellipse.RadiusY * (float)Math.Sin(num7)
			};
			m_tempPoints.Add(item);
		}
		DrawLines(m_tempPoints, brush, strokeWidth, strokeStyle);
	}

	public void DrawArc(D2dEllipse ellipse, System.Drawing.Color color, float startAngle, float sweepAngle, float strokeWidth = 1f, D2dStrokeStyle strokeStyle = null)
	{
		m_solidColorBrush.Color = color;
		DrawArc(ellipse, m_solidColorBrush, startAngle, sweepAngle, strokeWidth, strokeStyle);
	}

	public void DrawBezier(PointF pt1, PointF pt2, PointF pt3, PointF pt4, D2dBrush brush, float strokeWidth = 1f, D2dStrokeStyle strokeStyle = null)
	{
		using PathGeometry pathGeometry = new PathGeometry(D2dFactory.NativeFactory);
		GeometrySink geometrySink = pathGeometry.Open();
		geometrySink.BeginFigure(pt1.ToSharpDX(), FigureBegin.Hollow);
		BezierSegment bezier = new BezierSegment
		{
			Point1 = pt2.ToSharpDX(),
			Point2 = pt3.ToSharpDX(),
			Point3 = pt4.ToSharpDX()
		};
		geometrySink.AddBezier(bezier);
		geometrySink.EndFigure(FigureEnd.Open);
		geometrySink.Close();
		geometrySink.Dispose();
		StrokeStyle strokeStyle2 = strokeStyle?.NativeStrokeStyle;
		m_renderTarget.DrawGeometry(pathGeometry, brush.NativeBrush, strokeWidth, strokeStyle2);
	}

	public void DrawBezier(PointF pt1, PointF pt2, PointF pt3, PointF pt4, System.Drawing.Color color, float strokeWidth = 1f, D2dStrokeStyle strokeStyle = null)
	{
		m_solidColorBrush.Color = color;
		DrawBezier(pt1, pt2, pt3, pt4, m_solidColorBrush, strokeWidth, strokeStyle);
	}

	public void DrawBitmap(D2dBitmap bmp, PointF point, float opacity = 1f)
	{
		SizeF sizeF = bmp.PixelSize;
		SharpDX.RectangleF value = new SharpDX.RectangleF(point.X, point.Y, sizeF.Width, sizeF.Height);
		m_renderTarget.DrawBitmap(bmp.NativeBitmap, value, opacity, BitmapInterpolationMode.Linear, null);
	}

	public void DrawBitmap(D2dBitmap bmp, System.Drawing.RectangleF destRect, float opacity = 1f, D2dBitmapInterpolationMode interpolationMode = D2dBitmapInterpolationMode.Linear)
	{
		m_renderTarget.DrawBitmap(bmp.NativeBitmap, destRect.ToSharpDX(), opacity, (BitmapInterpolationMode)interpolationMode, null);
	}

	public void DrawBitmap(D2dBitmap bmp, System.Drawing.RectangleF destRect, float opacity, D2dBitmapInterpolationMode interpolationMode, System.Drawing.RectangleF sourceRect)
	{
		m_renderTarget.DrawBitmap(bmp.NativeBitmap, destRect.ToSharpDX(), opacity, (BitmapInterpolationMode)interpolationMode, sourceRect.ToSharpDX());
	}

	public void DrawEllipse(System.Drawing.RectangleF rect, D2dBrush brush, float strokeWidth = 1f, D2dStrokeStyle strokeStyle = null)
	{
		Ellipse ellipse = default(Ellipse);
		ellipse.RadiusX = rect.Width * 0.5f;
		ellipse.RadiusY = rect.Height * 0.5f;
		ellipse.Point = new Vector2(rect.X + ellipse.RadiusX, rect.Y + ellipse.RadiusY);
		m_renderTarget.DrawEllipse(ellipse, brush.NativeBrush, strokeWidth, strokeStyle?.NativeStrokeStyle);
	}

	public void DrawEllipse(System.Drawing.RectangleF rect, System.Drawing.Color color, float strokeWidth = 1f, D2dStrokeStyle strokeStyle = null)
	{
		m_solidColorBrush.Color = color;
		DrawEllipse(rect, m_solidColorBrush, strokeWidth, strokeStyle);
	}

	public void DrawEllipse(D2dEllipse ellipse, D2dBrush brush, float strokeWidth = 1f, D2dStrokeStyle strokeStyle = null)
	{
		m_renderTarget.DrawEllipse(ellipse.ToSharpDX(), brush.NativeBrush, strokeWidth, strokeStyle?.NativeStrokeStyle);
	}

	public void DrawEllipse(D2dEllipse ellipse, System.Drawing.Color color, float strokeWidth = 1f, D2dStrokeStyle strokeStyle = null)
	{
		m_solidColorBrush.Color = color;
		DrawEllipse(ellipse, m_solidColorBrush, strokeWidth, strokeStyle);
	}

	public void DrawLine(float pt1X, float pt1Y, float pt2X, float pt2Y, D2dBrush brush, float strokeWidth = 1f, D2dStrokeStyle strokeStyle = null)
	{
		m_renderTarget.DrawLine(new Vector2(pt1X, pt1Y), new Vector2(pt2X, pt2Y), brush.NativeBrush, strokeWidth, strokeStyle?.NativeStrokeStyle);
	}

	public void DrawLine(float pt1X, float pt1Y, float pt2X, float pt2Y, System.Drawing.Color color, float strokeWidth = 1f, D2dStrokeStyle strokeStyle = null)
	{
		m_solidColorBrush.Color = color;
		DrawLine(pt1X, pt1Y, pt2X, pt2Y, m_solidColorBrush, strokeWidth, strokeStyle);
	}

	public void DrawLine(PointF pt1, PointF pt2, D2dBrush brush, float strokeWidth = 1f, D2dStrokeStyle strokeStyle = null)
	{
		m_renderTarget.DrawLine(pt1.ToSharpDX(), pt2.ToSharpDX(), brush.NativeBrush, strokeWidth, strokeStyle?.NativeStrokeStyle);
	}

	public void DrawLine(PointF pt1, PointF pt2, System.Drawing.Color color, float strokeWidth = 1f, D2dStrokeStyle strokeStyle = null)
	{
		m_solidColorBrush.Color = color;
		DrawLine(pt1, pt2, m_solidColorBrush, strokeWidth, strokeStyle);
	}

	public void DrawLines(IEnumerable<PointF> points, D2dBrush brush, float strokeWidth = 1f, D2dStrokeStyle strokeStyle = null)
	{
		IEnumerator<PointF> enumerator = points.GetEnumerator();
		if (!enumerator.MoveNext())
		{
			return;
		}
		bool flag = brush.Opacity < 1f;
		if (!flag && brush is D2dSolidColorBrush { Color: var color })
		{
			flag = color.A < byte.MaxValue;
		}
		StrokeStyle nativeStrokeStyle = (strokeStyle ?? s_strokeStyle).NativeStrokeStyle;
		if (flag)
		{
			using (PathGeometry pathGeometry = new PathGeometry(D2dFactory.NativeFactory))
			{
				GeometrySink geometrySink = pathGeometry.Open();
				PointF current = enumerator.Current;
				geometrySink.BeginFigure(current.ToSharpDX(), FigureBegin.Hollow);
				while (enumerator.MoveNext())
				{
					geometrySink.AddLine(enumerator.Current.ToSharpDX());
				}
				geometrySink.EndFigure(FigureEnd.Open);
				geometrySink.Close();
				geometrySink.Dispose();
				m_renderTarget.DrawGeometry(pathGeometry, brush.NativeBrush, strokeWidth, nativeStrokeStyle);
				return;
			}
		}
		SharpDX.Direct2D1.Brush nativeBrush = brush.NativeBrush;
		PointF point = enumerator.Current;
		while (enumerator.MoveNext())
		{
			PointF current2 = enumerator.Current;
			m_renderTarget.DrawLine(point.ToSharpDX(), current2.ToSharpDX(), nativeBrush, strokeWidth, nativeStrokeStyle);
			point = current2;
		}
	}

	public void DrawLines(IEnumerable<PointF> points, System.Drawing.Color color, float strokeWidth = 1f, D2dStrokeStyle strokeStyle = null)
	{
		m_solidColorBrush.Color = color;
		DrawLines(points, m_solidColorBrush, strokeWidth, strokeStyle);
	}

	public void DrawPolygon(IEnumerable<PointF> points, D2dBrush brush, float strokeWidth = 1f, D2dStrokeStyle strokeStyle = null)
	{
		IEnumerator<PointF> enumerator = points.GetEnumerator();
		if (!enumerator.MoveNext())
		{
			return;
		}
		using PathGeometry pathGeometry = new PathGeometry(D2dFactory.NativeFactory);
		GeometrySink geometrySink = pathGeometry.Open();
		PointF current = enumerator.Current;
		geometrySink.BeginFigure(current.ToSharpDX(), FigureBegin.Hollow);
		while (enumerator.MoveNext())
		{
			geometrySink.AddLine(enumerator.Current.ToSharpDX());
		}
		geometrySink.EndFigure(FigureEnd.Closed);
		geometrySink.Close();
		geometrySink.Dispose();
		m_renderTarget.DrawGeometry(pathGeometry, brush.NativeBrush, strokeWidth, strokeStyle?.NativeStrokeStyle);
	}

	public void DrawPolygon(IEnumerable<PointF> points, System.Drawing.Color color, float strokeWidth = 1f, D2dStrokeStyle strokeStyle = null)
	{
		m_solidColorBrush.Color = color;
		DrawPolygon(points, m_solidColorBrush, strokeWidth, strokeStyle);
	}

	public void DrawRectangle(System.Drawing.RectangleF rect, D2dBrush brush, float strokeWidth = 1f, D2dStrokeStyle strokeStyle = null)
	{
		m_renderTarget.DrawRectangle(rect.ToSharpDX(), brush.NativeBrush, strokeWidth, strokeStyle?.NativeStrokeStyle);
	}

	public void DrawRectangle(System.Drawing.RectangleF rect, System.Drawing.Color color, float strokeWidth = 1f, D2dStrokeStyle strokeStyle = null)
	{
		m_solidColorBrush.Color = color;
		DrawRectangle(rect, m_solidColorBrush, strokeWidth, strokeStyle);
	}

	public void DrawRoundedRectangle(D2dRoundedRect roundedRect, D2dBrush brush, float strokeWidth = 1f, D2dStrokeStyle strokeStyle = null)
	{
		m_renderTarget.DrawRoundedRectangle(roundedRect.ToSharpDX(), brush.NativeBrush, strokeWidth, strokeStyle?.NativeStrokeStyle);
	}

	public void DrawRoundedRectangle(D2dRoundedRect roundedRect, System.Drawing.Color color, float strokeWidth = 1f, D2dStrokeStyle strokeStyle = null)
	{
		m_solidColorBrush.Color = color;
		DrawRoundedRectangle(roundedRect, m_solidColorBrush, strokeWidth, strokeStyle);
	}

	public void DrawText(string text, D2dTextFormat textFormat, PointF upperLeft, D2dBrush brush)
	{
		SizeF size = Size;
		using TextLayout textLayout = new TextLayout(D2dFactory.NativeDwFactory, text, textFormat.NativeTextFormat, size.Width, size.Height);
		if (textFormat.Underlined)
		{
			textLayout.SetUnderline(true, new TextRange(0, text.Length));
		}
		if (textFormat.Strikeout)
		{
			textLayout.SetStrikethrough(true, new TextRange(0, text.Length));
		}
		m_renderTarget.DrawTextLayout(upperLeft.ToSharpDX(), textLayout, brush.NativeBrush, (DrawTextOptions)textFormat.DrawTextOptions);
	}

	public void DrawText(string text, D2dTextFormat textFormat, PointF upperLeft, System.Drawing.Color color)
	{
		m_solidColorBrush.Color = color;
		DrawText(text, textFormat, upperLeft, m_solidColorBrush);
	}

	public void DrawText(string text, D2dTextFormat textFormat, System.Drawing.RectangleF layoutRect, D2dBrush brush)
	{
		using TextLayout textLayout = new TextLayout(D2dFactory.NativeDwFactory, text, textFormat.NativeTextFormat, layoutRect.Width, layoutRect.Height);
		if (textFormat.Underlined)
		{
			textLayout.SetUnderline(true, new TextRange(0, text.Length));
		}
		if (textFormat.Strikeout)
		{
			textLayout.SetStrikethrough(true, new TextRange(0, text.Length));
		}
		m_renderTarget.DrawTextLayout(layoutRect.Location.ToSharpDX(), textLayout, brush.NativeBrush, (DrawTextOptions)textFormat.DrawTextOptions);
	}

	public void DrawText(string text, D2dTextFormat textFormat, System.Drawing.RectangleF layoutRect, System.Drawing.Color color)
	{
		m_solidColorBrush.Color = color;
		DrawText(text, textFormat, layoutRect, m_solidColorBrush);
	}

	public void DrawTextLayout(PointF origin, D2dTextLayout textLayout, D2dBrush brush)
	{
		m_renderTarget.DrawTextLayout(origin.ToSharpDX(), textLayout.NativeTextLayout, brush.NativeBrush, (DrawTextOptions)textLayout.DrawTextOptions);
	}

	public void DrawTextLayout(PointF origin, D2dTextLayout textLayout, System.Drawing.Color color)
	{
		m_solidColorBrush.Color = color;
		DrawTextLayout(origin, textLayout, m_solidColorBrush);
	}

	public SizeF MeasureText(string text, D2dTextFormat format)
	{
		return MeasureText(text, format, new SizeF(2024f, 2024f));
	}

	public SizeF MeasureText(string text, D2dTextFormat format, SizeF maxSize)
	{
		using TextLayout textLayout = new TextLayout(D2dFactory.NativeDwFactory, text, format.NativeTextFormat, maxSize.Width, maxSize.Height);
		TextMetrics metrics = textLayout.Metrics;
		return new SizeF(metrics.WidthIncludingTrailingWhitespace, metrics.Height);
	}

	public void FillEllipse(System.Drawing.RectangleF rect, D2dBrush brush)
	{
		Ellipse ellipse = new Ellipse
		{
			RadiusX = rect.Width * 0.5f,
			RadiusY = rect.Height * 0.5f
		};
		ellipse.Point = new Vector2(rect.X + ellipse.RadiusX, rect.Y + ellipse.RadiusY);
		m_renderTarget.FillEllipse(ellipse, brush.NativeBrush);
	}

	public void FillEllipse(System.Drawing.RectangleF rect, System.Drawing.Color color)
	{
		m_solidColorBrush.Color = color;
		FillEllipse(rect, m_solidColorBrush);
	}

	public void FillEllipse(D2dEllipse ellipse, D2dBrush brush)
	{
		m_renderTarget.FillEllipse(ellipse.ToSharpDX(), brush.NativeBrush);
	}

	public void FillEllipse(D2dEllipse ellipse, System.Drawing.Color color)
	{
		m_solidColorBrush.Color = color;
		FillEllipse(ellipse, m_solidColorBrush);
	}

	public void FillOpacityMask(D2dBitmap opacityMask, D2dBrush brush, System.Drawing.RectangleF destRect)
	{
		m_renderTarget.FillOpacityMask(opacityMask.NativeBitmap, brush.NativeBrush, OpacityMaskContent.Graphics, destRect.ToSharpDX(), null);
	}

	public void FillOpacityMask(D2dBitmap opacityMask, System.Drawing.Color color, System.Drawing.RectangleF destRect)
	{
		m_solidColorBrush.Color = color;
		FillOpacityMask(opacityMask, m_solidColorBrush, destRect);
	}

	public void FillOpacityMask(D2dBitmap opacityMask, D2dBrush brush, System.Drawing.RectangleF destRect, System.Drawing.RectangleF sourceRect)
	{
		m_renderTarget.FillOpacityMask(opacityMask.NativeBitmap, brush.NativeBrush, OpacityMaskContent.Graphics, destRect.ToSharpDX(), sourceRect.ToSharpDX());
	}

	public void FillOpacityMask(D2dBitmap opacityMask, System.Drawing.Color color, System.Drawing.RectangleF destRect, System.Drawing.RectangleF sourceRect)
	{
		m_solidColorBrush.Color = color;
		FillOpacityMask(opacityMask, m_solidColorBrush, destRect, sourceRect);
	}

	public void FillPolygon(IEnumerable<PointF> points, D2dBrush brush)
	{
		IEnumerator<PointF> enumerator = points.GetEnumerator();
		if (!enumerator.MoveNext())
		{
			return;
		}
		using PathGeometry pathGeometry = new PathGeometry(D2dFactory.NativeFactory);
		GeometrySink geometrySink = pathGeometry.Open();
		PointF current = enumerator.Current;
		geometrySink.BeginFigure(current.ToSharpDX(), FigureBegin.Filled);
		while (enumerator.MoveNext())
		{
			geometrySink.AddLine(enumerator.Current.ToSharpDX());
		}
		geometrySink.EndFigure(FigureEnd.Closed);
		geometrySink.Close();
		geometrySink.Dispose();
		m_renderTarget.FillGeometry(pathGeometry, brush.NativeBrush);
	}

	public void FillPolygon(IEnumerable<PointF> points, System.Drawing.Color color)
	{
		m_solidColorBrush.Color = color;
		FillPolygon(points, m_solidColorBrush);
	}

	public void FillRectangle(System.Drawing.RectangleF rect, D2dBrush brush)
	{
		m_renderTarget.FillRectangle(rect.ToSharpDX(), brush.NativeBrush);
	}

	public void FillRectangle(System.Drawing.RectangleF rect, System.Drawing.Color color)
	{
		m_solidColorBrush.Color = color;
		FillRectangle(rect, m_solidColorBrush);
	}

	public void FillRectangle(System.Drawing.RectangleF rect, PointF pt1, PointF pt2, System.Drawing.Color color1, System.Drawing.Color color2)
	{
		SharpDX.Direct2D1.LinearGradientBrush cachedLinearGradientBrush = GetCachedLinearGradientBrush(color1, color2);
		cachedLinearGradientBrush.StartPoint = pt1.ToSharpDX();
		cachedLinearGradientBrush.EndPoint = pt2.ToSharpDX();
		m_renderTarget.FillRectangle(rect.ToSharpDX(), cachedLinearGradientBrush);
	}

	public void FillRoundedRectangle(D2dRoundedRect roundedRect, D2dBrush brush)
	{
		RoundedRectangle roundedRect2 = roundedRect.ToSharpDX();
		m_renderTarget.FillRoundedRectangle(ref roundedRect2, brush.NativeBrush);
	}

	public void FillRoundedRectangle(D2dRoundedRect roundedRect, System.Drawing.Color color)
	{
		m_solidColorBrush.Color = color;
		FillRoundedRectangle(roundedRect, m_solidColorBrush);
	}

	public void DrawPath(IEnumerable<EdgeStyleData> path, D2dBrush brush, float strokeWidth = 1f, D2dStrokeStyle strokeStyle = null)
	{
		using PathGeometry pathGeometry = new PathGeometry(D2dFactory.NativeFactory);
		GeometrySink geometrySink = pathGeometry.Open();
		bool flag = true;
		foreach (EdgeStyleData item in path)
		{
			if (item.ShapeType == EdgeStyleData.EdgeShape.Line)
			{
				PointF[] array = item.EdgeData.As<PointF[]>();
				if (flag)
				{
					geometrySink.BeginFigure(array[0].ToSharpDX(), FigureBegin.Hollow);
					flag = false;
				}
				for (int i = 1; i < array.Length; i++)
				{
					geometrySink.AddLine(array[i].ToSharpDX());
				}
			}
			else if (item.ShapeType == EdgeStyleData.EdgeShape.Bezier)
			{
				BezierCurve2F bezierCurve2F = item.EdgeData.As<BezierCurve2F>();
				if (flag)
				{
					geometrySink.BeginFigure(bezierCurve2F.P1.ToSharpDX(), FigureBegin.Hollow);
					flag = false;
				}
				BezierSegment bezier = new BezierSegment
				{
					Point1 = bezierCurve2F.P2.ToSharpDX(),
					Point2 = bezierCurve2F.P3.ToSharpDX(),
					Point3 = bezierCurve2F.P4.ToSharpDX()
				};
				geometrySink.AddBezier(bezier);
			}
		}
		geometrySink.EndFigure(FigureEnd.Open);
		geometrySink.Close();
		geometrySink.Dispose();
		m_renderTarget.DrawGeometry(pathGeometry, brush.NativeBrush, strokeWidth, strokeStyle?.NativeStrokeStyle);
	}

	public void DrawPath(IEnumerable<EdgeStyleData> path, System.Drawing.Color color, float strokeWidth = 1f, D2dStrokeStyle strokeStyle = null)
	{
		m_solidColorBrush.Color = color;
		DrawPath(path, m_solidColorBrush, strokeWidth, strokeStyle);
	}

	public void FillPath(IEnumerable<EdgeStyleData> path, D2dBrush brush)
	{
		using PathGeometry pathGeometry = new PathGeometry(D2dFactory.NativeFactory);
		GeometrySink geometrySink = pathGeometry.Open();
		bool flag = true;
		foreach (EdgeStyleData item in path)
		{
			if (item.ShapeType == EdgeStyleData.EdgeShape.Line)
			{
				PointF[] array = item.EdgeData.As<PointF[]>();
				if (flag)
				{
					geometrySink.BeginFigure(array[0].ToSharpDX(), FigureBegin.Filled);
					flag = false;
				}
				for (int i = 1; i < array.Length; i++)
				{
					geometrySink.AddLine(array[i].ToSharpDX());
				}
			}
			else if (item.ShapeType == EdgeStyleData.EdgeShape.Bezier)
			{
				BezierCurve2F bezierCurve2F = item.EdgeData.As<BezierCurve2F>();
				if (flag)
				{
					geometrySink.BeginFigure(bezierCurve2F.P1.ToSharpDX(), FigureBegin.Hollow);
					flag = false;
				}
				BezierSegment bezier = new BezierSegment
				{
					Point1 = bezierCurve2F.P2.ToSharpDX(),
					Point2 = bezierCurve2F.P3.ToSharpDX(),
					Point3 = bezierCurve2F.P4.ToSharpDX()
				};
				geometrySink.AddBezier(bezier);
			}
		}
		geometrySink.EndFigure(FigureEnd.Closed);
		geometrySink.Close();
		geometrySink.Dispose();
		m_renderTarget.FillGeometry(pathGeometry, brush.NativeBrush);
	}

	public void FillPath(IEnumerable<EdgeStyleData> path, System.Drawing.Color color)
	{
		m_solidColorBrush.Color = color;
		FillPath(path, m_solidColorBrush);
	}

	public void DrawGeometry(D2dGeometry geometry, D2dBrush brush, float strokeWidth = 1f, D2dStrokeStyle strokeStyle = null)
	{
		m_renderTarget.DrawGeometry(geometry.NativeGeometry, brush.NativeBrush, strokeWidth, strokeStyle?.NativeStrokeStyle);
	}

	public void DrawGeometry(D2dGeometry geometry, System.Drawing.Color color, float strokeWidth = 1f, D2dStrokeStyle strokeStyle = null)
	{
		m_solidColorBrush.Color = color;
		DrawGeometry(geometry, m_solidColorBrush, strokeWidth, strokeStyle);
	}

	public void FillGeometry(D2dGeometry geometry, D2dBrush brush)
	{
		m_renderTarget.FillGeometry(geometry.NativeGeometry, brush.NativeBrush);
	}

	public void FillGeometry(D2dGeometry geometry, System.Drawing.Color color)
	{
		m_solidColorBrush.Color = color;
		FillGeometry(geometry, m_solidColorBrush);
	}

	public void PushAxisAlignedClip(System.Drawing.RectangleF clipRect)
	{
		PushAxisAlignedClip(clipRect, AntialiasMode);
	}

	public void PushAxisAlignedClip(System.Drawing.RectangleF clipRect, D2dAntialiasMode antialiasMode)
	{
		m_clipStack.Push(clipRect);
		m_renderTarget.PushAxisAlignedClip(clipRect.ToSharpDX(), (AntialiasMode)antialiasMode);
	}

	public void PopAxisAlignedClip()
	{
		m_clipStack.Pop();
		m_renderTarget.PopAxisAlignedClip();
	}

	public D2dSolidColorBrush CreateSolidBrush(System.Drawing.Color color)
	{
		return new D2dSolidColorBrush(this, color);
	}

	public D2dLinearGradientBrush CreateLinearGradientBrush(params D2dGradientStop[] gradientStops)
	{
		return CreateLinearGradientBrush(default(PointF), default(PointF), gradientStops, D2dExtendMode.Clamp, D2dGamma.StandardRgb);
	}

	public D2dLinearGradientBrush CreateLinearGradientBrush(PointF pt1, PointF pt2, D2dGradientStop[] gradientStops, D2dExtendMode extendMode, D2dGamma gamma)
	{
		return new D2dLinearGradientBrush(this, pt1, pt2, gradientStops, extendMode, gamma);
	}

	public D2dRadialGradientBrush CreateRadialGradientBrush(params D2dGradientStop[] gradientStops)
	{
		return CreateRadialGradientBrush(new PointF(0f, 0f), new PointF(0f, 0f), 1f, 1f, gradientStops);
	}

	public D2dRadialGradientBrush CreateRadialGradientBrush(PointF center, PointF gradientOriginOffset, float radiusX, float radiusY, params D2dGradientStop[] gradientStops)
	{
		return new D2dRadialGradientBrush(this, center, gradientOriginOffset, radiusX, radiusY, gradientStops);
	}

	public D2dBitmapBrush CreateBitmapBrush(D2dBitmap bitmap)
	{
		return new D2dBitmapBrush(this, bitmap);
	}

	public D2dBitmap CreateBitmap(Type type, string resource)
	{
		using System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(type, resource);
		return CreateBitmap(bmp);
	}

	public D2dBitmap CreateBitmap(Stream stream)
	{
		using System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(stream);
		return CreateBitmap(bmp);
	}

	public D2dBitmap CreateBitmap(string filename)
	{
		using System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(filename);
		return CreateBitmap(bmp);
	}

	public D2dBitmap CreateBitmap(Image img)
	{
		if (img == null)
		{
			throw new ArgumentNullException("img");
		}
		using System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(img);
		return CreateBitmap(bmp);
	}

	public D2dBitmap CreateBitmap(System.Drawing.Bitmap bmp)
	{
		if (bmp == null)
		{
			throw new ArgumentNullException("bmp");
		}
		System.Drawing.Bitmap bmp2 = bmp.Clone(new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height), System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
		return new D2dBitmap(this, bmp2);
	}

	public D2dBitmap CreateBitmap(int width, int height)
	{
		if (width < 1 || height < 1)
		{
			throw new ArgumentOutOfRangeException("Width and height must be greater than zero");
		}
		System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
		return new D2dBitmap(this, bmp);
	}

	public D2dBitmapGraphics CreateCompatibleGraphics()
	{
		SharpDX.Direct2D1.BitmapRenderTarget renderTarget = new SharpDX.Direct2D1.BitmapRenderTarget(m_renderTarget, CompatibleRenderTargetOptions.None);
		return new D2dBitmapGraphics(this, renderTarget);
	}

	public D2dBitmapGraphics CreateCompatibleGraphics(D2dCompatibleGraphicsOptions options)
	{
		SharpDX.Direct2D1.BitmapRenderTarget renderTarget = new SharpDX.Direct2D1.BitmapRenderTarget(m_renderTarget, (CompatibleRenderTargetOptions)options);
		return new D2dBitmapGraphics(this, renderTarget);
	}

	public D2dBitmapGraphics CreateCompatibleGraphics(Size pixelSize, D2dCompatibleGraphicsOptions options)
	{
		SharpDX.Direct2D1.BitmapRenderTarget renderTarget = new SharpDX.Direct2D1.BitmapRenderTarget(desiredPixelSize: new Size2(pixelSize.Width, pixelSize.Height), renderTarget: m_renderTarget, options: (CompatibleRenderTargetOptions)options, desiredSize: null, desiredFormat: null);
		return new D2dBitmapGraphics(this, renderTarget);
	}

	public D2dBitmapGraphics CreateCompatibleGraphics(SizeF size, D2dCompatibleGraphicsOptions options)
	{
		SharpDX.Direct2D1.BitmapRenderTarget renderTarget = new SharpDX.Direct2D1.BitmapRenderTarget(m_renderTarget, (CompatibleRenderTargetOptions)options, size.ToSharpDX(), null, null);
		return new D2dBitmapGraphics(this, renderTarget);
	}

	public void BeginGdiSection()
	{
		if (m_graphics != null)
		{
			throw new InvalidOperationException("EndGDISection() call required");
		}
		m_graphics = Graphics.FromHdc(m_gdiInterop.GetDC(DeviceContextInitializeMode.Copy));
		m_graphics.SmoothingMode = SmoothingMode.AntiAlias;
		ApplyXformToGdi();
	}

	public void EndGdiSection()
	{
		if (m_graphics == null)
		{
			throw new InvalidOperationException("BeginGDISection() call required");
		}
		m_graphics.Dispose();
		m_graphics = null;
		m_gdiInterop.ReleaseDC();
	}

	protected D2dGraphics(RenderTarget renderTarget)
	{
		SetRenderTarget(renderTarget);
	}

	protected void SetRenderTarget(RenderTarget renderTarget)
	{
		if (renderTarget == null)
		{
			throw new ArgumentNullException("renderTarget");
		}
		m_clipStack.Clear();
		ReleaseResources(disposing: true);
		m_renderTargetNumber++;
		m_renderTarget = renderTarget;
		m_gdiInterop = m_renderTarget.QueryInterface<GdiInteropRenderTarget>();
		Transform = Matrix3x2F.Identity;
		if (s_strokeStyle == null)
		{
			D2dStrokeStyleProperties props = new D2dStrokeStyleProperties
			{
				EndCap = D2dCapStyle.Round,
				StartCap = D2dCapStyle.Round
			};
			s_strokeStyle = D2dFactory.CreateD2dStrokeStyle(props);
		}
		m_solidColorBrush = CreateSolidBrush(System.Drawing.Color.Empty);
	}

	protected abstract void RecreateRenderTarget();

	protected override void Dispose(bool disposing)
	{
		if (!base.IsDisposed)
		{
			ReleaseResources(disposing);
			base.Dispose(disposing);
		}
	}

	private void ReleaseResources(bool disposing)
	{
		if (disposing && m_solidColorBrush != null)
		{
			m_solidColorBrush.Dispose();
			m_solidColorBrush = null;
		}
		if (m_gdiInterop != null)
		{
			m_gdiInterop.Dispose();
			m_gdiInterop = null;
		}
		if (m_renderTarget != null)
		{
			m_renderTarget.Dispose();
			m_renderTarget = null;
		}
		foreach (SharpDX.Direct2D1.LinearGradientBrush value in m_linearGradients.Values)
		{
			value.Dispose();
		}
		m_linearGradients.Clear();
	}

	private void ApplyXformToGdi()
	{
		using System.Drawing.Drawing2D.Matrix transform = new System.Drawing.Drawing2D.Matrix(m_xform.M11, m_xform.M12, m_xform.M21, m_xform.M22, m_xform.DX, m_xform.DY);
		m_graphics.Transform = transform;
	}

	private void RecreateTargetAndResources()
	{
		RecreateRenderTarget();
		this.RecreateResources.Raise(this, EventArgs.Empty);
	}

	private SharpDX.Direct2D1.LinearGradientBrush GetCachedLinearGradientBrush(System.Drawing.Color color1, System.Drawing.Color color2)
	{
		long num = color1.ToArgb();
		long num2 = color2.ToArgb();
		long key = (num << 32) | num2;
		if (!m_linearGradients.TryGetValue(key, out var value))
		{
			GradientStop[] array = new GradientStop[2];
			array[0].Color = color1.ToColor4();
			array[0].Position = 0f;
			array[1].Color = color2.ToColor4();
			array[1].Position = 1f;
			using (GradientStopCollection gradientStopCollection = new GradientStopCollection(m_renderTarget, array, Gamma.StandardRgb, ExtendMode.Clamp))
			{
				value = new SharpDX.Direct2D1.LinearGradientBrush(m_renderTarget, default(LinearGradientBrushProperties), gradientStopCollection);
			}
			m_linearGradients.Add(key, value);
		}
		return value;
	}
}
