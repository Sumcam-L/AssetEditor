using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using Sce.Atf.Controls.Adaptable.Graphs;
using Sce.Atf.VectorMath;
using SharpDX;

namespace Sce.Atf.Direct2D;

public static class D2dUtil
{
	private static PointF[] s_expanderPoints = new PointF[3];

	public const int ThumbtackSize = 8;

	private static readonly PointF[] s_tempPtsF = new PointF[2];

	private static readonly PointF[] s_unitCurvedArrowData = new PointF[16]
	{
		new PointF(0.07094253f, 0f),
		new PointF(1f, 0f),
		new PointF(1f, 0.64827794f),
		new PointF(0.658584f, 0.43695468f),
		new PointF(0.658584f, 0.43695468f),
		new PointF(0.5315179f, 0.54116195f),
		new PointF(0.40445033f, 0.64827794f),
		new PointF(0.2773828f, 0.7553939f),
		new PointF(0.3329844f, 0.8624769f),
		new PointF(0.6029824f, 0.93012214f),
		new PointF(0.6029824f, 0.93012214f),
		new PointF(0f, 1f),
		new PointF(0.031254724f, 0.5355469f),
		new PointF(0.039974235f, 0.40607378f),
		new PointF(0.14512452f, 0.35655782f),
		new PointF(0.30916238f, 0.18605746f)
	};

	public static void DrawExpander(this D2dGraphics g, float x, float y, float size, D2dBrush brush, bool expanded)
	{
		s_expanderPoints[0] = new PointF(x, y + size);
		if (expanded)
		{
			s_expanderPoints[1] = new PointF(x + size, y + size);
			s_expanderPoints[2] = new PointF(x + size, y);
			g.FillPolygon(s_expanderPoints, brush);
		}
		else
		{
			s_expanderPoints[1] = new PointF(x + size, y + size / 2f);
			s_expanderPoints[2] = new PointF(x, y);
			g.DrawPolygon(s_expanderPoints, brush);
		}
	}

	public static void DrawEyeIcon(this D2dGraphics g, System.Drawing.RectangleF rect, D2dBrush pen, float strokeWidth)
	{
		float num = rect.Width / 3f;
		PointF pt = new PointF(rect.X, rect.Y + rect.Height / 2f);
		PointF pt2 = new PointF(pt.X + num, rect.Y);
		PointF pt3 = new PointF(pt.X + 2f * num, rect.Y);
		PointF pt4 = new PointF(rect.X + rect.Width, rect.Y + rect.Height / 2f);
		g.DrawBezier(pt, pt2, pt3, pt4, pen, strokeWidth);
		g.DrawBezier(pt2: new PointF(pt2.X, rect.Y + rect.Height), pt3: new PointF(pt3.X, rect.Y + rect.Height), pt1: pt, pt4: pt4, brush: pen, strokeWidth: strokeWidth);
		PointF pointF = new PointF(rect.X + rect.Width / 2f, rect.Y + rect.Height / 2f);
		float num2 = 0.2f * Math.Min(rect.Width, rect.Height);
		System.Drawing.RectangleF rect2 = new System.Drawing.RectangleF(pointF.X - num2, pointF.Y - num2, 2f * num2, 2f * num2);
		g.DrawEllipse(rect2, pen, strokeWidth * 1.8f);
	}

	public static void DrawVerticalScaleGrid(this D2dGraphics g, System.Drawing.Drawing2D.Matrix transform, System.Drawing.RectangleF graphRect, int majorSpacing, D2dBrush lineBrush)
	{
		double num = transform.Elements[0];
		System.Drawing.RectangleF rectangleF = Transform(transform, graphRect);
		double num2 = Math.Min(graphRect.Left, graphRect.Right);
		double num3 = Math.Max(graphRect.Left, graphRect.Right);
		double num4 = CalculateTickAnchor(num2, num3);
		double num5 = CalculateStep(num2, num3, Math.Abs(rectangleF.Right - rectangleF.Left), majorSpacing, 0.0);
		if (num5 > 0.0)
		{
			double num6 = num4 - num2;
			num6 = num6 - MathUtil.Remainder(num6, num5) + num5;
			for (double num7 = num4 - num6; num7 <= num3; num7 += num5)
			{
				double num8 = (num7 - (double)graphRect.Left) * num + (double)rectangleF.Left;
				g.DrawLine((float)num8, rectangleF.Top, (float)num8, rectangleF.Bottom, lineBrush);
			}
		}
	}

	public static System.Drawing.RectangleF Transform(System.Drawing.Drawing2D.Matrix matrix, System.Drawing.RectangleF r)
	{
		s_tempPtsF[0] = new PointF(r.Left, r.Top);
		s_tempPtsF[1] = new PointF(r.Right, r.Bottom);
		matrix.TransformPoints(s_tempPtsF);
		return MakeRectangle(s_tempPtsF[0], s_tempPtsF[1]);
	}

	public static System.Drawing.RectangleF Transform(Matrix3x2F matrix, System.Drawing.RectangleF r)
	{
		s_tempPtsF[0] = new PointF(r.Left, r.Top);
		s_tempPtsF[1] = new PointF(r.Right, r.Bottom);
		s_tempPtsF[0] = Matrix3x2F.TransformPoint(matrix, s_tempPtsF[0]);
		s_tempPtsF[1] = Matrix3x2F.TransformPoint(matrix, s_tempPtsF[1]);
		return MakeRectangle(s_tempPtsF[0], s_tempPtsF[1]);
	}

	public static System.Drawing.Point InverseTransform(Matrix3x2F matrix, System.Drawing.Point p)
	{
		Matrix3x2F mat = matrix;
		mat.Invert();
		s_tempPtsF[0] = p;
		s_tempPtsF[0] = Matrix3x2F.TransformPoint(mat, s_tempPtsF[0]);
		return new System.Drawing.Point((int)s_tempPtsF[0].X, (int)s_tempPtsF[0].Y);
	}

	public static PointF TransformVector(Matrix3x2F matrix, PointF v)
	{
		s_tempPtsF[0] = v;
		Matrix3x2F.TransformVector(matrix, s_tempPtsF[0]);
		return s_tempPtsF[0];
	}

	public static void DrawHorizontalScale(this D2dGraphics g, System.Drawing.Drawing2D.Matrix transform, System.Drawing.RectangleF graphRect, bool top, int majorSpacing, float minimumGraphStep, D2dBrush lineBrush, D2dTextFormat textFormat, D2dBrush textBrush)
	{
		double num = transform.Elements[0];
		System.Drawing.RectangleF rectangleF = Transform(transform, graphRect);
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
		double num8 = CalculateTickAnchor(num6, num7);
		double num9 = CalculateStep(num6, num7, Math.Abs(rectangleF.Right - rectangleF.Left), majorSpacing, minimumGraphStep);
		int num10 = CalculateNumMinorTicks(num9, minimumGraphStep, 5);
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
				g.DrawLine((float)num13, (float)num4, (float)num13, (float)num2, lineBrush);
			}
			num13 += num11;
		}
		for (double num14 = num8 - num12; num14 < num7; num14 += num9)
		{
			double num15 = (num14 - num6) * num + (double)rectangleF.Left;
			g.DrawLine((float)num15, (float)num3, (float)num15, (float)num2, lineBrush);
			string text = $"{Math.Round(num14, 6):G8}";
			SizeF size = g.MeasureText(text, textFormat);
			System.Drawing.RectangleF layoutRect = new System.Drawing.RectangleF(new PointF((float)num15 + 1f, (float)num5), size);
			g.DrawText(text, textFormat, layoutRect, textBrush);
			num13 = num15 + num11;
			for (int j = 0; j < num10 - 1; j++)
			{
				if (!(num13 < (double)rectangleF.Right))
				{
					break;
				}
				g.DrawLine((float)num13, (float)num4, (float)num13, (float)num2, lineBrush);
				num13 += num11;
			}
		}
	}

	public static double CalculateTickAnchor(double min, double max)
	{
		return (min * max <= 0.0) ? 0.0 : Math.Pow(10.0, Math.Floor(Math.Log10(Snap10(Math.Abs(max)))));
	}

	public static double CalculateStep(double graphMin, double graphMax, double screenLength, int majorScreenSpacing, double minimumGraphSpacing)
	{
		double num = graphMax - graphMin;
		double num2 = screenLength / (double)majorScreenSpacing;
		double proposed = num / num2;
		return Snap10(proposed);
	}

	public static int CalculateNumMinorTicks(double majorGraphStep, double minimumGraphStep, int maxMinorTicks)
	{
		if (minimumGraphStep <= 0.0)
		{
			return maxMinorTicks;
		}
		int num = (int)(majorGraphStep / minimumGraphStep);
		if (num > maxMinorTicks)
		{
			num = maxMinorTicks;
		}
		if (num <= 0)
		{
			num = 1;
		}
		return num;
	}

	public static double Snap10(double proposed)
	{
		double num = Math.Pow(10.0, Math.Floor(Math.Log10(proposed)));
		int num2 = (int)(proposed / num + 0.5);
		if (num2 > 5)
		{
			num2 = 10;
		}
		else if (num2 > 2)
		{
			num2 = 5;
		}
		else if (num2 > 1)
		{
			num2 = 2;
		}
		return (double)num2 * num;
	}

	public static void DrawPin(int x, int y, bool pinned, bool toLeft, D2dBrush pen, D2dGraphics g)
	{
		if (toLeft)
		{
			DrawLeftPin(x, y, 8, pen, pinned, g);
		}
		else
		{
			DrawRightPin(x, y, 8, pen, pinned, g);
		}
	}

	public static void DrawLeftPin(int x, int y, int size, D2dBrush pen, bool pinned, D2dGraphics g)
	{
		int num = size / 4;
		int num2 = 2 * size / 3;
		int num3 = size / 2;
		if (pinned)
		{
			g.DrawLine(x + num3, y + num2, x + num3, y + size, pen);
			g.DrawLine(x, y + num2, x + size, y + num2, pen);
			g.DrawRectangle(new System.Drawing.RectangleF(x + num, y, 2 * num, num2), pen);
			g.DrawLine(x + 3 * num - 1, y, x + 3 * num - 1, y + num2, pen);
		}
		else
		{
			g.DrawLine(x, y + num3, x + size - num2, y + num3, pen);
			g.DrawLine(x + size - num2, y, x + size - num2, y + size, pen);
			g.DrawRectangle(new System.Drawing.RectangleF(x + size - num2, y + (size - num) / 2 - 1, num2, 2 * num), pen);
			g.DrawLine(x + size - num2, y + (size - num) / 2 + 2 * num - 2, x + size, y + (size - num) / 2 + 2 * num - 2, pen);
		}
	}

	public static void DrawRightPin(int x, int y, int size, D2dBrush pen, bool pinned, D2dGraphics g)
	{
		int num = size / 4;
		int num2 = 2 * size / 3;
		int num3 = size / 2;
		if (pinned)
		{
			g.DrawLine(x + num3 - size, y + num2, x + num3 - size, y + size, pen);
			g.DrawLine(x - size, y + num2, x, y + num2, pen);
			g.DrawRectangle(new System.Drawing.RectangleF(x + num - size, y, 2 * num, num2), pen);
			g.DrawLine(x + 3 * num - 1 - size, y, x + 3 * num - 1 - size, y + num2, pen);
		}
		else
		{
			g.DrawLine(x, y + num3, x - size + num2, y + num3, pen);
			g.DrawLine(x - size + num2, y, x - size + num2, y + size, pen);
			g.DrawRectangle(new System.Drawing.RectangleF(x - size, y + (size - num) / 2 - 1, num2, 2 * num), pen);
			g.DrawLine(x - size + num2, y + (size - num) / 2 + 2 * num - 2, x - size, y + (size - num) / 2 + 2 * num - 2, pen);
		}
	}

	public static void DrawLink(this D2dGraphics g, float x, float y, float size, D2dBrush brush)
	{
		EdgeStyleData[] array = new EdgeStyleData[5];
		PointF[] array2 = new PointF[16];
		for (int i = 0; i < 16; i++)
		{
			array2[i] = new PointF(s_unitCurvedArrowData[i].X * size + x, s_unitCurvedArrowData[i].Y * size + y);
		}
		EdgeStyleData edgeStyleData = new EdgeStyleData();
		edgeStyleData.ShapeType = EdgeStyleData.EdgeShape.Line;
		edgeStyleData.EdgeData = new PointF[4]
		{
			array2[0],
			array2[1],
			array2[2],
			array2[3]
		};
		EdgeStyleData edgeStyleData2 = edgeStyleData;
		array[0] = edgeStyleData2;
		edgeStyleData2 = new EdgeStyleData
		{
			ShapeType = EdgeStyleData.EdgeShape.Bezier,
			EdgeData = new BezierCurve2F(array2[3], array2[4], array2[5], array2[6])
		};
		array[1] = edgeStyleData2;
		edgeStyleData2 = new EdgeStyleData
		{
			ShapeType = EdgeStyleData.EdgeShape.Bezier,
			EdgeData = new BezierCurve2F(array2[6], array2[7], array2[8], array2[9])
		};
		array[2] = edgeStyleData2;
		edgeStyleData2 = new EdgeStyleData
		{
			ShapeType = EdgeStyleData.EdgeShape.Bezier,
			EdgeData = new BezierCurve2F(array2[9], array2[10], array2[11], array2[12])
		};
		array[3] = edgeStyleData2;
		edgeStyleData2 = new EdgeStyleData
		{
			ShapeType = EdgeStyleData.EdgeShape.Bezier,
			EdgeData = new BezierCurve2F(array2[12], array2[13], array2[14], array2[15])
		};
		array[4] = edgeStyleData2;
		g.FillPath(array, brush);
	}

	internal static Color4 ToColor4(this System.Drawing.Color color)
	{
		uint num = (uint)color.ToArgb();
		uint rgba = (num & 0xFF00FF00u) | ((num >> 16) & 0xFF) | ((num & 0xFF) << 16);
		return new Color4(rgba);
	}

	internal static System.Drawing.Color ToSystemColor(this Color4 color4)
	{
		return System.Drawing.Color.FromArgb(color4.ToBgra());
	}

	public static System.Drawing.RectangleF MakeRectangle(PointF p1, PointF p2)
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
		return new System.Drawing.RectangleF(num, num2, num3, num4);
	}
}
