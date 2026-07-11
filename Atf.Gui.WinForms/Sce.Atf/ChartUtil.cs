using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using Sce.Atf.Direct2D;

namespace Sce.Atf;

public class ChartUtil
{
	public static void DrawHorizontalGrid(Matrix transform, RectangleF graphRect, double step, Color color, Graphics g)
	{
		double num = transform.Elements[3];
		RectangleF rectangleF = GdiUtil.Transform(transform, graphRect);
		double screenStep = Math.Abs(num * step);
		Pen pen = CreateFadedPen(screenStep, color);
		double num2 = (double)graphRect.Top - MathUtil.Remainder(graphRect.Top, step) + step;
		for (double num3 = num2; num3 < (double)graphRect.Bottom; num3 += step)
		{
			double num4 = (num3 - (double)graphRect.Top) * num + (double)rectangleF.Top;
			g.DrawLine(pen, rectangleF.Left, (float)num4, rectangleF.Right, (float)num4);
		}
		pen.Dispose();
	}

	public static void DrawHorizontalGrid(Matrix transform, RectangleF graphRect, double step, Color color, D2dGraphics g)
	{
		double num = transform.Elements[3];
		RectangleF rectangleF = GdiUtil.Transform(transform, graphRect);
		double screenStep = Math.Abs(num * step);
		int alpha = ComputeOpacity(screenStep);
		color = Color.FromArgb(alpha, color);
		double num2 = (double)graphRect.Top - MathUtil.Remainder(graphRect.Top, step) + step;
		for (double num3 = num2; num3 < (double)graphRect.Bottom; num3 += step)
		{
			double num4 = (num3 - (double)graphRect.Top) * num + (double)rectangleF.Top;
			g.DrawLine(rectangleF.Left, (float)num4, rectangleF.Right, (float)num4, color);
		}
	}

	public static void DrawVerticalGrid(Matrix transform, RectangleF graphRect, double step, Color color, Graphics g)
	{
		double num = transform.Elements[0];
		RectangleF rectangleF = GdiUtil.Transform(transform, graphRect);
		double screenStep = Math.Abs(num * step);
		Pen pen = CreateFadedPen(screenStep, color);
		double num2 = (double)graphRect.Left - MathUtil.Remainder(graphRect.Left, step) + step;
		for (double num3 = num2; num3 < (double)graphRect.Right; num3 += step)
		{
			double num4 = (num3 - (double)graphRect.Left) * num + (double)rectangleF.Left;
			g.DrawLine(pen, (float)num4, rectangleF.Top, (float)num4, rectangleF.Bottom);
		}
		pen.Dispose();
	}

	public static void DrawVerticalGrid(Matrix transform, RectangleF graphRect, double step, Color color, D2dGraphics g)
	{
		double num = transform.Elements[0];
		RectangleF rectangleF = GdiUtil.Transform(transform, graphRect);
		double screenStep = Math.Abs(num * step);
		int alpha = ComputeOpacity(screenStep);
		color = Color.FromArgb(alpha, color);
		double num2 = (double)graphRect.Left - MathUtil.Remainder(graphRect.Left, step) + step;
		for (double num3 = num2; num3 < (double)graphRect.Right; num3 += step)
		{
			double num4 = (num3 - (double)graphRect.Left) * num + (double)rectangleF.Left;
			g.DrawLine((float)num4, rectangleF.Top, (float)num4, rectangleF.Bottom, color);
		}
	}

	public static void LabelGrid(Matrix transform, RectangleF graphRect, double step, Font font, Brush textBrush, Graphics g)
	{
		double num = transform.Elements[0];
		double num2 = transform.Elements[3];
		RectangleF rectangleF = GdiUtil.Transform(transform, graphRect);
		double num3 = Math.Min(Math.Abs(num * step), Math.Abs(num2 * step));
		while (num3 < 96.0)
		{
			num3 *= 2.0;
			step *= 2.0;
		}
		double num4 = (double)graphRect.Left - MathUtil.Remainder(graphRect.Left, step);
		double num5 = (double)graphRect.Top - MathUtil.Remainder(graphRect.Top, step);
		for (double num6 = num5; num6 <= (double)graphRect.Bottom; num6 += step)
		{
			double num7 = (num6 - (double)graphRect.Top) * num2 + (double)rectangleF.Top;
			string text = $"{num6:G4}";
			for (double num8 = num4; num8 <= (double)graphRect.Right; num8 += step)
			{
				double num9 = (num8 - (double)graphRect.Left) * num + (double)rectangleF.Left;
				string s = string.Format("({0:G4}, " + text + ")", num8);
				g.DrawString(s, font, textBrush, (float)num9, (float)num7);
			}
		}
	}

	public static void DrawHorizontalScaleGrid(Matrix transform, RectangleF graphRect, int majorSpacing, Pen linePen, Graphics g)
	{
		double num = transform.Elements[3];
		RectangleF rectangleF = GdiUtil.Transform(transform, graphRect);
		double num2 = Math.Min(graphRect.Top, graphRect.Bottom);
		double num3 = Math.Max(graphRect.Top, graphRect.Bottom);
		double num4 = D2dUtil.CalculateTickAnchor(num2, num3);
		double num5 = D2dUtil.CalculateStep(num2, num3, Math.Abs(rectangleF.Bottom - rectangleF.Top), majorSpacing, 0.0);
		if (num5 > 0.0)
		{
			double num6 = num4 - num2;
			num6 = num6 - MathUtil.Remainder(num6, num5) + num5;
			for (double num7 = num4 - num6; num7 <= num3; num7 += num5)
			{
				double num8 = ((!(num > 0.0)) ? ((num7 - num3) * num + (double)rectangleF.Top) : ((num7 - num2) * num + (double)rectangleF.Top));
				g.DrawLine(linePen, rectangleF.Left, (float)num8, rectangleF.Right, (float)num8);
			}
		}
	}

	public static void DrawVerticalScaleGrid(Matrix transform, RectangleF graphRect, int majorSpacing, Pen linePen, Graphics g)
	{
		double num = transform.Elements[0];
		RectangleF rectangleF = GdiUtil.Transform(transform, graphRect);
		double num2 = Math.Min(graphRect.Left, graphRect.Right);
		double num3 = Math.Max(graphRect.Left, graphRect.Right);
		double num4 = D2dUtil.CalculateTickAnchor(num2, num3);
		double num5 = D2dUtil.CalculateStep(num2, num3, Math.Abs(rectangleF.Right - rectangleF.Left), majorSpacing, 0.0);
		if (num5 > 0.0)
		{
			double num6 = num4 - num2;
			num6 = num6 - MathUtil.Remainder(num6, num5) + num5;
			for (double num7 = num4 - num6; num7 <= num3; num7 += num5)
			{
				double num8 = (num7 - (double)graphRect.Left) * num + (double)rectangleF.Left;
				g.DrawLine(linePen, (float)num8, rectangleF.Top, (float)num8, rectangleF.Bottom);
			}
		}
	}

	public static void DrawHorizontalScale(Matrix transform, RectangleF graphRect, bool top, int majorSpacing, float minimumGraphStep, Pen linePen, Font font, Brush textBrush, Graphics g)
	{
		double num = transform.Elements[0];
		RectangleF rectangleF = GdiUtil.Transform(transform, graphRect);
		double num2;
		double num3;
		double num4;
		double num5;
		if (top)
		{
			num2 = rectangleF.Top + 1f;
			num3 = num2 + 12.0;
			num4 = num2 + 6.0;
			num5 = num2 + 8.0;
		}
		else
		{
			num2 = rectangleF.Bottom - 1f;
			num3 = num2 - 12.0;
			num4 = num2 - 6.0;
			num5 = num2 - 19.0;
		}
		double num6 = Math.Min(graphRect.Left, graphRect.Right);
		double num7 = Math.Max(graphRect.Left, graphRect.Right);
		double num8 = D2dUtil.CalculateTickAnchor(num6, num7);
		double num9 = D2dUtil.CalculateStep(num6, num7, Math.Abs(rectangleF.Right - rectangleF.Left), majorSpacing, minimumGraphStep);
		int num10 = D2dUtil.CalculateNumMinorTicks(num9, minimumGraphStep, 5);
		double num11 = num9 / (double)num10 * num;
		if (!(num9 > 0.0))
		{
			return;
		}
		double num12 = num8 - num6;
		num12 -= MathUtil.Remainder(num12, num9);
		double num13 = (num8 - (num12 + num9) - num6) * num + (double)rectangleF.Left + num11;
		for (int i = 0; i < num10 - 1; i++)
		{
			if (!(num13 < (double)rectangleF.Right))
			{
				break;
			}
			if (num13 > (double)rectangleF.Left)
			{
				g.DrawLine(linePen, (float)num13, (float)num4, (float)num13, (float)num2);
			}
			num13 += num11;
		}
		for (double num14 = num8 - num12; num14 < num7; num14 += num9)
		{
			double num15 = (num14 - num6) * num + (double)rectangleF.Left;
			g.DrawLine(linePen, (float)num15, (float)num3, (float)num15, (float)num2);
			string s = $"{Math.Round(num14, 6):G8}";
			g.DrawString(s, font, textBrush, (float)num15 + 1f, (float)num5);
			num13 = num15 + num11;
			for (int j = 0; j < num10 - 1; j++)
			{
				if (!(num13 < (double)rectangleF.Right))
				{
					break;
				}
				g.DrawLine(linePen, (float)num13, (float)num4, (float)num13, (float)num2);
				num13 += num11;
			}
		}
	}

	public static void DrawVerticalScale(Matrix transform, RectangleF graphRect, bool left, int majorSpacing, float minimumGraphStep, Pen linePen, Font font, Brush textBrush, Graphics g)
	{
		double num = transform.Elements[3];
		RectangleF rectangleF = GdiUtil.Transform(transform, graphRect);
		Matrix transform2 = g.Transform.Clone();
		Matrix transform3 = g.Transform;
		transform3.Translate(rectangleF.Right, rectangleF.Bottom);
		transform3.Rotate(90f);
		transform3.Translate(0f - rectangleF.Left, 0f - rectangleF.Top);
		g.Transform = transform3;
		double num2;
		double num3;
		double num4;
		if (left)
		{
			num2 = rectangleF.Right - rectangleF.X;
			num3 = num2 - 6.0;
			num4 = num2 - 19.0;
		}
		else
		{
			num2 = rectangleF.Left + 1f;
			num3 = num2 + 6.0;
			num4 = num2 + 8.0;
		}
		double num5 = Math.Min(graphRect.Top, graphRect.Bottom);
		double num6 = Math.Max(graphRect.Top, graphRect.Bottom);
		double num7 = D2dUtil.CalculateTickAnchor(num5, num6);
		double num8 = D2dUtil.CalculateStep(num5, num6, Math.Abs(rectangleF.Bottom - rectangleF.Top), majorSpacing, minimumGraphStep);
		int num9 = D2dUtil.CalculateNumMinorTicks(num8, minimumGraphStep, 5);
		double num10 = num8 / (double)num9 * num;
		if (num8 > 0.0)
		{
			double num11 = num7 - num5;
			num11 = num11 - MathUtil.Remainder(num11, num8) + num8;
			for (double num12 = num7 - num11; num12 <= num6; num12 += num8)
			{
				double num13 = (num12 - num5) * num + (double)rectangleF.Left;
				double num14 = num13;
				for (int i = 0; i < num9; i++)
				{
					num14 += num10;
					g.DrawLine(linePen, (float)num14, (float)num3, (float)num14, (float)num2);
				}
				string s = $"{Math.Round(num12, 6):G8}";
				g.DrawString(s, font, textBrush, (float)num13 + 2f, (float)num4);
			}
		}
		g.Transform = transform2;
	}

	public static void DrawXYLabel(float x, float y, Point position, Brush backgroundBrush, Pen linePen, Font font, Brush textBrush, Graphics g)
	{
		string text = x + ", " + y;
		SizeF sizeF = g.MeasureString(text, font);
		Rectangle rect = new Rectangle(position.X + 8, position.Y + 8, (int)sizeF.Width + 2, (int)sizeF.Height + 2);
		g.FillRectangle(backgroundBrush, rect);
		g.DrawRectangle(linePen, rect);
		g.DrawString(text, font, textBrush, rect.X + 1, rect.Y + 1);
	}

	public static Point SnapToGrid(Point original, float horizontalStep, float verticalStep)
	{
		int x = (int)MathUtil.Snap(original.X, horizontalStep);
		int y = (int)MathUtil.Snap(original.Y, verticalStep);
		return new Point(x, y);
	}

	private static Pen CreateFadedPen(double screenStep, Color color)
	{
		screenStep = Math.Min(screenStep, 64.0);
		screenStep = Math.Max(screenStep, 4.0);
		int alpha = (int)(255.0 * (screenStep - 4.0) / 60.0);
		return new Pen(Color.FromArgb(alpha, color));
	}

	private static int ComputeOpacity(double screenStep)
	{
		screenStep = Math.Min(screenStep, 64.0);
		screenStep = Math.Max(screenStep, 4.0);
		return (int)(255.0 * (screenStep - 4.0) / 60.0);
	}
}
