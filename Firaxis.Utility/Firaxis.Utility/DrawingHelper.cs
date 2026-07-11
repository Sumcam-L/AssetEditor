using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using Firaxis.MathEx;
using Firaxis.Utility.Properties;

namespace Firaxis.Utility;

public static class DrawingHelper
{
	private static float LuminanceRed = 0.3086f;

	private static float LuminanceGreen = 0.6094f;

	private static float LuminanceBlue = 0.082f;

	public static ColorMatrix MakeTint(Color color)
	{
		float num = (float)(int)color.R / 255f;
		float num2 = (float)(int)color.G / 255f;
		float num3 = (float)(int)color.B / 255f;
		float num4 = (float)(int)color.A / 255f;
		return new ColorMatrix(new float[5][]
		{
			new float[5] { num, 0f, 0f, 0f, 0f },
			new float[5] { 0f, num2, 0f, 0f, 0f },
			new float[5] { 0f, 0f, num3, 0f, 0f },
			new float[5] { 0f, 0f, 0f, num4, 0f },
			new float[5] { 0f, 0f, 0f, 0f, 1f }
		});
	}

	public static ColorMatrix MakeSaturation(float s)
	{
		return MakeSaturation(s, 1f);
	}

	public static ColorMatrix MakeSaturation(float s, float alpha)
	{
		if (s < 0f || s > 1f)
		{
			throw new ArgumentOutOfRangeException("s", "Saturation must be in the range 0.0 .. 1.0");
		}
		float num = 1f - s;
		float num2 = LuminanceRed * num;
		float num3 = LuminanceGreen * num;
		float num4 = LuminanceBlue * num;
		return new ColorMatrix(new float[5][]
		{
			new float[5]
			{
				num2 + s,
				num2,
				num2,
				0f,
				0f
			},
			new float[5]
			{
				num3,
				num3 + s,
				num3,
				0f,
				0f
			},
			new float[5]
			{
				num4,
				num4,
				num4 + s,
				0f,
				0f
			},
			new float[5] { 0f, 0f, 0f, alpha, 0f },
			new float[5] { 0f, 0f, 0f, 0f, 1f }
		});
	}

	public static GraphicsPath CreateRoundRect(Rectangle r, int radius)
	{
		return CreateRoundRect(r, radius, RectangleCorners.All);
	}

	public static GraphicsPath CreateRoundRect(Rectangle r, int radius, RectangleCorners corners)
	{
		GraphicsPath graphicsPath;
		if (r.Width < radius * 2 || r.Height < radius * 2)
		{
			graphicsPath = new GraphicsPath();
			graphicsPath.StartFigure();
			graphicsPath.AddRectangle(r);
			graphicsPath.CloseFigure();
			return graphicsPath;
		}
		Rectangle rectangle = new Rectangle(r.Left, r.Top, 2 * radius, 2 * radius);
		Rectangle rect = rectangle;
		rect.X = r.Right - 2 * radius;
		Rectangle rectangle2 = rectangle;
		rectangle2.Y = r.Bottom - 2 * radius;
		Rectangle rect2 = rectangle2;
		rect2.X = r.Right - 2 * radius;
		Point[] array = new Point[12]
		{
			new Point(rectangle.Left, rectangle.Bottom),
			rectangle.Location,
			new Point(rectangle.Right, rectangle.Top),
			rect.Location,
			new Point(rect.Right, rect.Top),
			new Point(rect.Right, rect.Bottom),
			new Point(rect2.Right, rect2.Top),
			new Point(rect2.Right, rect2.Bottom),
			new Point(rect2.Left, rect2.Bottom),
			new Point(rectangle2.Right, rectangle2.Bottom),
			new Point(rectangle2.Left, rectangle2.Bottom),
			rectangle2.Location
		};
		graphicsPath = new GraphicsPath();
		graphicsPath.StartFigure();
		if ((RectangleCorners.TopLeft & corners) == RectangleCorners.TopLeft)
		{
			graphicsPath.AddArc(rectangle, 180f, 90f);
		}
		else
		{
			graphicsPath.AddLines(new Point[3]
			{
				array[0],
				array[1],
				array[2]
			});
		}
		graphicsPath.AddLine(array[2], array[3]);
		if ((RectangleCorners.TopRight & corners) == RectangleCorners.TopRight)
		{
			graphicsPath.AddArc(rect, 270f, 90f);
		}
		else
		{
			graphicsPath.AddLines(new Point[3]
			{
				array[3],
				array[4],
				array[5]
			});
		}
		graphicsPath.AddLine(array[5], array[6]);
		if ((RectangleCorners.BottomRight & corners) == RectangleCorners.BottomRight)
		{
			graphicsPath.AddArc(rect2, 0f, 90f);
		}
		else
		{
			graphicsPath.AddLines(new Point[3]
			{
				array[6],
				array[7],
				array[8]
			});
		}
		graphicsPath.AddLine(array[8], array[9]);
		if ((RectangleCorners.BottomLeft & corners) == RectangleCorners.BottomLeft)
		{
			graphicsPath.AddArc(rectangle2, 90f, 90f);
		}
		else
		{
			graphicsPath.AddLines(new Point[3]
			{
				array[9],
				array[10],
				array[11]
			});
		}
		graphicsPath.AddLine(array[11], array[0]);
		graphicsPath.CloseFigure();
		return graphicsPath;
	}

	public static void DrawImageCentered(Graphics g, Image image, float x, float y, ColorMatrix m)
	{
		DrawImage(g, image, (int)x - image.Width / 2, (int)y - image.Height / 2, image.Width, image.Height, m);
	}

	public static void DrawImageCentered(Graphics g, Image image, float x, float y, Color color)
	{
		if (color == Color.White)
		{
			DrawImageCentered(g, image, x, y);
		}
		else
		{
			DrawImage(g, image, (int)x - image.Width / 2, (int)y - image.Height / 2, image.Width, image.Height, MakeTint(color));
		}
	}

	public static void DrawImageCentered(Graphics g, Image image, float x, float y)
	{
		g.DrawImage(image, (int)x - image.Width / 2, (int)y - image.Height / 2, image.Width, image.Height);
	}

	public static void DrawImage(Graphics g, Image image, float x, float y)
	{
		g.DrawImage(image, x, y, image.Width, image.Height);
	}

	public static void DrawImage(Graphics g, Image image, float x, float y, Color color)
	{
		if (color == Color.White)
		{
			g.DrawImage(image, x, y, image.Width, image.Height);
		}
		else
		{
			DrawImage(g, image, x, y, MakeTint(color));
		}
	}

	public static void DrawImage(Graphics g, Image image, float x, float y, int width, int height, ColorMatrix m)
	{
		ImageAttributes imageAttributes = new ImageAttributes();
		imageAttributes.SetColorMatrix(m);
		Rectangle destRect = new Rectangle((int)x, (int)y, width, height);
		g.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, imageAttributes);
	}

	public static void DrawImage(Graphics g, Image image, float x, float y, ColorMatrix m)
	{
		ImageAttributes imageAttributes = new ImageAttributes();
		imageAttributes.SetColorMatrix(m);
		Rectangle destRect = new Rectangle((int)x, (int)y, image.Width, image.Height);
		g.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, imageAttributes);
	}

	public static void DrawPSChecker(Graphics g, Rectangle r)
	{
		using TextureBrush textureBrush = new TextureBrush(Resources.psgrid, WrapMode.Tile);
		Matrix matrix = new Matrix();
		matrix.Translate(r.X, r.Y);
		textureBrush.Transform = matrix;
		g.FillRectangle(textureBrush, r);
	}

	public static void DrawChecker(Graphics g, Rectangle r)
	{
		DrawChecker(g, r, 5, Color.White, Color.FromArgb(240, 240, 240));
	}

	public static void DrawChecker(Graphics g, Rectangle r, int size)
	{
		DrawChecker(g, r, size, Color.White, Color.FromArgb(240, 240, 240));
	}

	public static void DrawChecker(Graphics g, Rectangle r, int size, Color c1, Color c2)
	{
		bool flag = false;
		bool flag2 = false;
		using Brush brush = new SolidBrush(c1);
		using Brush brush2 = new SolidBrush(c2);
		SmoothingMode smoothingMode = g.SmoothingMode;
		g.SmoothingMode = SmoothingMode.None;
		CompositingMode compositingMode = g.CompositingMode;
		g.CompositingMode = CompositingMode.SourceCopy;
		for (int i = r.Y; i < r.Y + r.Height; i += size)
		{
			flag2 = flag;
			flag = !flag;
			for (int j = r.X; j < r.X + r.Width; j += size)
			{
				g.FillRectangle(flag2 ? brush : brush2, j, i, size + 1, size + 1);
				flag2 = !flag2;
			}
		}
		g.SmoothingMode = smoothingMode;
		g.CompositingMode = compositingMode;
	}

	public static void DrawBezier(Graphics g, Pen pen, PointF p1, PointF p2)
	{
		PointF pt = p1;
		pt.X = (p1.X + p2.X) * 0.5f;
		PointF pt2 = p2;
		pt2.X = (p1.X + p2.X) * 0.5f;
		g.DrawBezier(pen, p1, pt, pt2, p2);
	}
}
