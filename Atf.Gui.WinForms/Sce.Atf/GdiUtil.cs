using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Sce.Atf.VectorMath;

namespace Sce.Atf;

public static class GdiUtil
{
	public const int ShadowSize = 4;

	public const int ExpanderSize = 8;

	public const int OfficeExpanderSize = 9;

	public const int SortDirectionIndicatorWidth = 12;

	public const int SortDirectionIndicatorHeight = 6;

	private static float s_dpiYFactor = 0f;

	private static readonly Point[] s_tempPts = new Point[2];

	private static readonly PointF[] s_tempPtsF = new PointF[2];

	private static readonly PointF[] s_tempPtF = new PointF[1];

	private static readonly Point[] s_directionIndicatorPoints = new Point[3];

	private static readonly Point[] s_expanderPoints = new Point[3];

	private static readonly Pen s_expanderPen = new Pen(Color.Black);

	public static float DpiFactor
	{
		get
		{
			if (s_dpiYFactor == 0f)
			{
				Form form = new Form();
				Graphics graphics = form.CreateGraphics();
				float dpiY = graphics.DpiY;
				s_dpiYFactor = graphics.DpiY / 96f;
				graphics.Dispose();
			}
			return s_dpiYFactor;
		}
	}

	public static Matrix GetTransform(Point translation, float scale)
	{
		Matrix matrix = new Matrix();
		matrix.Translate(translation.X, translation.Y);
		matrix.Scale(scale, scale);
		return matrix;
	}

	public static Matrix GetTransform(Point translation, float xScale, float yScale)
	{
		Matrix matrix = new Matrix();
		matrix.Translate(translation.X, translation.Y);
		matrix.Scale(xScale, yScale);
		return matrix;
	}

	public static Point Transform(Matrix matrix, Point p)
	{
		s_tempPts[0] = p;
		matrix.TransformPoints(s_tempPts);
		return s_tempPts[0];
	}

	public static PointF Transform(Matrix matrix, PointF p)
	{
		s_tempPtF[0] = p;
		matrix.TransformPoints(s_tempPtF);
		return s_tempPtF[0];
	}

	public static PointF TransformVector(Matrix matrix, PointF v)
	{
		s_tempPtF[0] = v;
		matrix.TransformVectors(s_tempPtF);
		return s_tempPtF[0];
	}

	public static float Transform(Matrix matrix, float x)
	{
		s_tempPtF[0].X = x;
		s_tempPtF[0].Y = 0f;
		matrix.TransformPoints(s_tempPtF);
		return s_tempPtF[0].X;
	}

	public static float TransformVector(Matrix matrix, float x)
	{
		s_tempPtF[0].X = x;
		s_tempPtF[0].Y = 0f;
		matrix.TransformVectors(s_tempPtF);
		return s_tempPtF[0].X;
	}

	public static Point InverseTransform(Matrix matrix, Point p)
	{
		Matrix matrix2 = matrix.Clone();
		matrix2.Invert();
		s_tempPts[0] = p;
		matrix2.TransformPoints(s_tempPts);
		return s_tempPts[0];
	}

	public static PointF InverseTransform(Matrix matrix, PointF p)
	{
		Matrix matrix2 = matrix.Clone();
		matrix2.Invert();
		s_tempPtF[0] = p;
		matrix2.TransformPoints(s_tempPtF);
		return s_tempPtF[0];
	}

	public static PointF InverseTransformVector(Matrix matrix, PointF v)
	{
		Matrix matrix2 = matrix.Clone();
		matrix2.Invert();
		s_tempPtF[0] = v;
		matrix2.TransformVectors(s_tempPtF);
		return s_tempPtF[0];
	}

	public static float InverseTransform(Matrix matrix, float x)
	{
		Matrix matrix2 = matrix.Clone();
		matrix2.Invert();
		s_tempPtF[0].X = x;
		s_tempPtF[0].Y = 0f;
		matrix2.TransformPoints(s_tempPtF);
		return s_tempPtF[0].X;
	}

	public static float InverseTransformVector(Matrix matrix, float x)
	{
		Matrix matrix2 = matrix.Clone();
		matrix2.Invert();
		s_tempPtF[0].X = x;
		s_tempPtF[0].Y = 0f;
		matrix2.TransformVectors(s_tempPtF);
		return s_tempPtF[0].X;
	}

	public static Rectangle Transform(Matrix matrix, Rectangle r)
	{
		s_tempPts[0] = new Point(r.Left, r.Top);
		s_tempPts[1] = new Point(r.Right, r.Bottom);
		matrix.TransformPoints(s_tempPts);
		return new Rectangle(s_tempPts[0].X, s_tempPts[0].Y, s_tempPts[1].X - s_tempPts[0].X, s_tempPts[1].Y - s_tempPts[0].Y);
	}

	public static RectangleF Transform(Matrix matrix, RectangleF r)
	{
		s_tempPtsF[0] = new PointF(r.Left, r.Top);
		s_tempPtsF[1] = new PointF(r.Right, r.Bottom);
		matrix.TransformPoints(s_tempPtsF);
		return MakeRectangle(s_tempPtsF[0], s_tempPtsF[1]);
	}

	public static Rectangle InverseTransform(Matrix matrix, Rectangle r)
	{
		Matrix matrix2 = matrix.Clone();
		matrix2.Invert();
		s_tempPts[0] = new Point(r.Left, r.Top);
		s_tempPts[1] = new Point(r.Right, r.Bottom);
		matrix2.TransformPoints(s_tempPts);
		return new Rectangle(s_tempPts[0].X, s_tempPts[0].Y, s_tempPts[1].X - s_tempPts[0].X, s_tempPts[1].Y - s_tempPts[0].Y);
	}

	public static RectangleF InverseTransform(Matrix matrix, RectangleF r)
	{
		Matrix matrix2 = matrix.Clone();
		matrix2.Invert();
		s_tempPtsF[0] = new PointF(r.Left, r.Top);
		s_tempPtsF[1] = new PointF(r.Right, r.Bottom);
		matrix2.TransformPoints(s_tempPtsF);
		return MakeRectangle(s_tempPtsF[0], s_tempPtsF[1]);
	}

	public static bool Intersects(Seg2F seg, RectangleF rect)
	{
		if (rect.Contains(seg.P1) || rect.Contains(seg.P2))
		{
			return true;
		}
		if (seg.P1.X < rect.Left && seg.P2.X < rect.Left)
		{
			return false;
		}
		if (seg.P1.Y < rect.Top && seg.P2.Y < rect.Top)
		{
			return false;
		}
		if (seg.P1.X > rect.Right && seg.P2.X > rect.Right)
		{
			return false;
		}
		if (seg.P1.Y > rect.Bottom && seg.P2.Y > rect.Bottom)
		{
			return false;
		}
		Vec2F vec2F = new Vec2F(seg.P2.X - seg.P1.X, seg.P2.Y - seg.P1.Y);
		if (vec2F.LengthSquared > 9.9999994E-11f)
		{
			Vec2F perp = vec2F.Perp;
			float num = Vec2F.Dot(new Vec2F(rect.Left, rect.Top) - seg.P1, perp);
			float num2 = Vec2F.Dot(new Vec2F(rect.Right, rect.Top) - seg.P1, perp);
			if (num * num2 > 0f)
			{
				num2 = Vec2F.Dot(new Vec2F(rect.Left, rect.Bottom) - seg.P1, perp);
				if (num * num2 > 0f)
				{
					num2 = Vec2F.Dot(new Vec2F(rect.Right, rect.Bottom) - seg.P1, perp);
					if (num * num2 > 0f)
					{
						return false;
					}
				}
			}
		}
		return true;
	}

	public static bool Intersects(BezierCurve2F curve, RectangleF rect, float tolerance)
	{
		if (curve.Flatness <= tolerance)
		{
			Seg2F seg = new Seg2F(curve.P1, curve.P4);
			if (Intersects(seg, rect))
			{
				return true;
			}
		}
		else
		{
			curve.Subdivide(0.5f, out var left, out var right);
			if (Intersects(left, rect, tolerance))
			{
				return true;
			}
			if (Intersects(right, rect, tolerance))
			{
				return true;
			}
		}
		return false;
	}

	public static Image ResizeImage(Image image, int size)
	{
		return ResizeImage(image, size, size);
	}

	public static Image ResizeImage(Image image, int width, int height)
	{
		if (image == null)
		{
			throw new ArgumentNullException();
		}
		if (width <= 0 || height <= 0)
		{
			throw new ArgumentOutOfRangeException();
		}
		if (image.Width == width && image.Height == height)
		{
			return new Bitmap(image);
		}
		Bitmap bitmap = new Bitmap(width, height, image.PixelFormat);
		bitmap.SetResolution(image.HorizontalResolution, image.VerticalResolution);
		Graphics graphics = Graphics.FromImage(bitmap);
		graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
		graphics.DrawImage(image, new Rectangle(0, 0, width, height));
		graphics.Dispose();
		return bitmap;
	}

	public static Image GetImage(string imageName)
	{
		return GetImage(Assembly.GetCallingAssembly(), imageName);
	}

	public static Image GetImage(string path, string imageName)
	{
		return GetImage(Assembly.GetCallingAssembly(), path + "." + imageName);
	}

	public static Image GetImage(Assembly assembly, string imageName)
	{
		Image image = null;
		Stream stream = null;
		try
		{
			stream = assembly.GetManifestResourceStream(imageName);
			if (stream != null)
			{
				image = new Bitmap(stream);
				image = new Bitmap(image);
			}
		}
		catch (Exception ex)
		{
			Outputs.WriteLine(OutputMessageType.Error, ex.Message);
			Outputs.WriteLine(OutputMessageType.Error, ex.StackTrace);
		}
		finally
		{
			stream?.Close();
		}
		return image;
	}

	public static Icon GetIcon(string iconName)
	{
		return GetIcon(Assembly.GetCallingAssembly(), iconName);
	}

	public static Icon GetIcon(string path, string iconName)
	{
		return GetIcon(Assembly.GetCallingAssembly(), path + "." + iconName);
	}

	public static Icon GetIcon(Assembly assembly, string iconName)
	{
		Icon result = null;
		Stream stream = null;
		try
		{
			stream = assembly.GetManifestResourceStream(iconName);
			result = new Icon(stream);
		}
		catch (Exception ex)
		{
			Outputs.WriteLine(OutputMessageType.Warning, ex.Message);
		}
		finally
		{
			stream?.Close();
		}
		return result;
	}

	public static Icon CreateIcon(Image image)
	{
		int size = Math.Max(image.Width, image.Height);
		return CreateIcon(image, size, keepAspectRatio: false);
	}

	public static Icon CreateIcon(Image image, int size, bool keepAspectRatio)
	{
		Bitmap bitmap = new Bitmap(size, size);
		using (Graphics graphics = Graphics.FromImage(bitmap))
		{
			int x;
			int y;
			int num;
			int num2;
			if (!keepAspectRatio || image.Height == image.Width)
			{
				x = (y = 0);
				num = (num2 = size);
			}
			else
			{
				float num3 = (float)image.Width / (float)image.Height;
				if (num3 > 1f)
				{
					num = size;
					num2 = (int)((float)size / num3);
					x = 0;
					y = (size - num2) / 2;
				}
				else
				{
					num = (int)((float)size * num3);
					num2 = size;
					y = 0;
					x = (size - num) / 2;
				}
			}
			graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
			graphics.DrawImage(image, x, y, num, num2);
		}
		return Icon.FromHandle(bitmap.GetHicon());
	}

	public static SizeF MeasureDisplayString(Graphics graphics, string text, Font font)
	{
		Bitmap bitmap = new Bitmap(32, 1, graphics);
		SizeF sizeF = graphics.MeasureString(text, font);
		Graphics graphics2 = Graphics.FromImage(bitmap);
		int num = (int)sizeF.Width;
		if (graphics2 != null)
		{
			graphics2.Clear(Color.White);
			graphics2.DrawString(text + "|", font, Brushes.Black, 32 - num, -font.Height / 2);
			for (int num2 = 31; num2 >= 0; num2--)
			{
				num--;
				if (bitmap.GetPixel(num2, 0).R == 0)
				{
					break;
				}
			}
		}
		return new SizeF(num, sizeF.Height);
	}

	public static int MeasureDisplayStringWidth(Graphics graphics, string text, Font font)
	{
		return (int)MeasureDisplayString(graphics, text, font).Width;
	}

	public static int GetPreferredWidth<T>(ToolStrip owner) where T : ToolStripItem
	{
		int num = owner.DisplayRectangle.Width;
		if (owner.OverflowButton.Visible)
		{
			num = num - owner.OverflowButton.Width - owner.OverflowButton.Margin.Horizontal;
		}
		int num2 = 0;
		foreach (ToolStripItem item in owner.Items)
		{
			if (!item.IsOnOverflow)
			{
				if (item is T)
				{
					num2++;
					num -= item.Margin.Horizontal;
				}
				else
				{
					num = num - item.Width - item.Margin.Horizontal;
				}
			}
		}
		if (num2 > 1)
		{
			num /= num2;
		}
		return num;
	}

	public static Image CreateLozengeImage(Color color1, Color color2, int cornerRadius)
	{
		return CreateLozengeImage(color1, color2, Pens.Transparent, Color.FromArgb(0, 0, 0, 0), cornerRadius);
	}

	public static Image CreateLozengeImage(Color color1, Color color2, Pen outlinePen, Color shadowColor, int cornerRadius)
	{
		int num = 2 * cornerRadius;
		int num2 = num + 4;
		Bitmap bitmap = new Bitmap(num2, num2);
		using (Graphics graphics = Graphics.FromImage(bitmap))
		{
			graphics.SmoothingMode = SmoothingMode.HighQuality;
			graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
			using (SolidBrush brush = new SolidBrush(shadowColor))
			{
				graphics.FillEllipse(brush, 4, 4, num, num);
			}
			using (LinearGradientBrush brush2 = new LinearGradientBrush(new Rectangle(0, 0, num, num), color1, color2, LinearGradientMode.ForwardDiagonal))
			{
				graphics.FillEllipse(brush2, 0, 0, num, num);
			}
			graphics.DrawEllipse(outlinePen, 0, 0, num, num);
		}
		return bitmap;
	}

	public static void DrawLozenge(Image lozengeImage, Rectangle bounds, Graphics g)
	{
		int num = (lozengeImage.Width - 4) / 2;
		g.DrawImage(lozengeImage, new Rectangle(bounds.Left, bounds.Top, num + 1, num + 1), new Rectangle(0, 0, num, num), GraphicsUnit.Pixel);
		g.DrawImage(lozengeImage, new Rectangle(bounds.Right - num, bounds.Top, num + 4, num + 1), new Rectangle(num, 0, num + 4, num), GraphicsUnit.Pixel);
		g.DrawImage(lozengeImage, new Rectangle(bounds.Right - num, bounds.Bottom - num, num + 4, num + 4), new Rectangle(num, num, num + 4, num + 4), GraphicsUnit.Pixel);
		g.DrawImage(lozengeImage, new Rectangle(bounds.Left, bounds.Bottom - num, num + 1, num + 4), new Rectangle(0, num, num, num + 4), GraphicsUnit.Pixel);
		g.DrawImage(lozengeImage, new Rectangle(bounds.Left + num, bounds.Top, bounds.Width - 2 * num, num), new Rectangle(num, 0, 1, num), GraphicsUnit.Pixel);
		g.DrawImage(lozengeImage, new Rectangle(bounds.Right - num, bounds.Top + num, num + 4, bounds.Height - 2 * num), new Rectangle(num, num, num + 4, 1), GraphicsUnit.Pixel);
		g.DrawImage(lozengeImage, new Rectangle(bounds.Left + num, bounds.Bottom - num, bounds.Width - 2 * num, num + 4), new Rectangle(num, num, 1, num + 4), GraphicsUnit.Pixel);
		g.DrawImage(lozengeImage, new Rectangle(bounds.Left, bounds.Top + num, num, bounds.Height - 2 * num), new Rectangle(0, num, num, 1), GraphicsUnit.Pixel);
		g.DrawImage(lozengeImage, new Rectangle(bounds.Left + num - 1, bounds.Top + num - 1, bounds.Width - 2 * num + 1, bounds.Height - 2 * num + 1), new Rectangle(num, num, 1, 1), GraphicsUnit.Pixel);
	}

	public static Image CaptureWindow(IntPtr handle)
	{
		IntPtr dC = User32.GetDC(handle);
		if (dC == IntPtr.Zero)
		{
			return null;
		}
		User32.RECT rect = default(User32.RECT);
		User32.GetWindowRect(handle, ref rect);
		int width = rect.Right - rect.Left;
		int height = rect.Bottom - rect.Top;
		Bitmap bitmap = new Bitmap(width, height);
		using (Graphics graphics = Graphics.FromImage(bitmap))
		{
			User32.PrintWindow(handle, graphics.GetHdc(), 0);
		}
		return bitmap;
	}

	public static void DrawExpander(int x, int y, bool expanded, Graphics g)
	{
		DrawExpander(x, y, 8, SystemBrushes.ControlDarkDark, expanded, g);
	}

	public static void DrawExpander(int x, int y, bool expanded, Graphics g, Brush b)
	{
		DrawExpander(x, y, 8, b, expanded, g);
	}

	public static void DrawExpander(int x, int y, int size, Brush brush, bool expanded, Graphics g)
	{
		s_expanderPoints[0] = new Point(x, y + size);
		if (expanded)
		{
			s_expanderPoints[1] = new Point(x + size, y + size);
			s_expanderPoints[2] = new Point(x + size, y);
			g.FillPolygon(brush, s_expanderPoints);
		}
		else
		{
			s_expanderPen.Color = (brush as SolidBrush)?.Color ?? SystemColors.WindowText;
			s_expanderPoints[1] = new Point(x + size, y + size / 2);
			s_expanderPoints[2] = new Point(x, y);
			g.DrawPolygon(s_expanderPen, s_expanderPoints);
		}
	}

	public static void DrawOfficeExpander(int x, int y, Pen pen, bool expanded, Graphics g)
	{
		int num = x + 4;
		int num2 = y + 4;
		int num3 = x + 9 - 1;
		int num4 = y + 9 - 1;
		if (!expanded)
		{
			int num5 = x;
			x = num3;
			num3 = num5;
			num5 = y;
			y = num4;
			num4 = num5;
		}
		g.DrawLine(pen, x, y, num, num2);
		g.DrawLine(pen, num, num2, num3, y);
		g.DrawLine(pen, x, num2, num, num4);
		g.DrawLine(pen, num, num4, num3, num2);
	}

	public static void DrawSortDirectionIndicator(int x, int y, bool up, Graphics g)
	{
		DrawSortDirectionIndicator(x, y, 12, 6, up, SystemBrushes.ControlDark, g);
	}

	public static void DrawSortDirectionIndicator(int x, int y, int width, int height, bool up, Brush brush, Graphics g)
	{
		if (up)
		{
			y += height - 1;
			height = -height;
		}
		s_directionIndicatorPoints[0] = new Point(x, y);
		s_directionIndicatorPoints[1] = new Point(x + width, y);
		s_directionIndicatorPoints[2] = new Point(x + width / 2, y + height);
		g.FillPolygon(brush, s_directionIndicatorPoints);
	}

	public static Rectangle MakeRectangle(Point p1, Point p2)
	{
		int num = p1.X;
		int num2 = p1.Y;
		int num3 = p2.X - p1.X;
		int num4 = p2.Y - p1.Y;
		if (num3 < 0)
		{
			num += num3;
			num3 = -num3;
		}
		if (num4 < 0)
		{
			num2 += num4;
			num4 = -num4;
		}
		return new Rectangle(num, num2, num3, num4);
	}

	public static RectangleF MakeRectangle(PointF p1, PointF p2)
	{
		float num = p1.X;
		float num2 = p1.Y;
		float num3 = p2.X - p1.X;
		float num4 = p2.Y - p1.Y;
		if (num3 < 0f)
		{
			num += num3;
			num3 = 0f - num3;
		}
		if (num4 < 0f)
		{
			num2 += num4;
			num4 = 0f - num4;
		}
		return new RectangleF(num, num2, num3, num4);
	}

	public static RectangleF GetRegionBounds(Region region)
	{
		RectangleF rectangleF = default(RectangleF);
		RectangleF[] regionScans = region.GetRegionScans(new Matrix());
		foreach (RectangleF rectangleF2 in regionScans)
		{
			rectangleF = ((!rectangleF.IsEmpty) ? RectangleF.Union(rectangleF, rectangleF2) : rectangleF2);
		}
		return rectangleF;
	}
}
