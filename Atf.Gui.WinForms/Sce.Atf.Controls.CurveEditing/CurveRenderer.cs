using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using Sce.Atf.VectorMath;

namespace Sce.Atf.Controls.CurveEditing;

public class CurveRenderer : IDisposable
{
	private Cartesian2dCanvas m_canvas;

	private const float FinLength = 10f;

	private const float sin = 0.5f;

	private const float cos = 0.8660254f;

	private Color m_tangentColor = Color.DeepSkyBlue;

	private Brush m_pointBrush = new SolidBrush(Color.Black);

	private Brush m_pointHiBrush = new SolidBrush(Color.Red);

	private float m_pointSize = 5f;

	private float m_tangentLength = 40f;

	private int m_tessellation = 4;

	private Pen m_tangentArrowLinePen = new Pen(Color.DarkCyan, 2f);

	private SolidBrush m_tangentArrowHeadBrush = (SolidBrush)Brushes.DarkCyan;

	private Pen m_infinityPen = new Pen(Color.FromArgb(40, 40, 40))
	{
		DashPattern = new float[2] { 2f, 2f }
	};

	private Pen m_curvePen = new Pen(Color.Black);

	private static readonly PointF[] s_arrowPts = new PointF[3];

	public float PointSize
	{
		get
		{
			return m_pointSize;
		}
		set
		{
			if (value < 2f || value > 64f)
			{
				throw new ArgumentOutOfRangeException();
			}
			m_pointSize = value;
		}
	}

	public Color TangentColor
	{
		get
		{
			return m_tangentColor;
		}
		set
		{
			m_tangentColor = value;
		}
	}

	public Brush PointBrush
	{
		get
		{
			return m_pointBrush;
		}
		set
		{
			m_pointBrush.Dispose();
			m_pointBrush = value;
		}
	}

	public Brush PointHighlightBrush
	{
		get
		{
			return m_pointHiBrush;
		}
		set
		{
			m_pointHiBrush.Dispose();
			m_pointHiBrush = value;
		}
	}

	public void PickPoints(IEnumerable<ICurve> curves, RectangleF pickRect, List<IControlPoint> points, List<PointSelectionRegions> regions, bool singlePick = false)
	{
		points.Clear();
		regions.Clear();
		if (curves == null)
		{
			return;
		}
		foreach (ICurve curf in curves)
		{
			if (!curf.Visible)
			{
				continue;
			}
			ReadOnlyCollection<IControlPoint> controlPoints = curf.ControlPoints;
			for (int i = 0; i < controlPoints.Count; i++)
			{
				IControlPoint controlPoint = controlPoints[i];
				Vec2F vec2F = m_canvas.GraphToClient(controlPoint.X, controlPoint.Y);
				if (pickRect.Contains(vec2F))
				{
					points.Add(controlPoint);
					regions.Add(PointSelectionRegions.Point);
				}
				else if (curf.CurveInterpolation != InterpolationTypes.Linear && controlPoint.EditorData.SelectedRegion != PointSelectionRegions.None)
				{
					bool flag = false;
					if (controlPoint.TangentOutType != CurveTangentTypes.Stepped && controlPoint.TangentOutType != CurveTangentTypes.SteppedNext)
					{
						Vec2F vec2F2 = Vec2F.Normalize(m_canvas.GraphToClientTangent(controlPoint.TangentOut));
						Seg2F seg = new Seg2F(vec2F, vec2F + vec2F2 * m_tangentLength);
						if (GdiUtil.Intersects(seg, pickRect))
						{
							points.Add(controlPoint);
							regions.Add(PointSelectionRegions.TangentOut);
							flag = true;
						}
					}
					bool flag2 = true;
					if (i > 0)
					{
						IControlPoint controlPoint2 = controlPoints[i - 1];
						flag2 = controlPoint2.TangentOutType != CurveTangentTypes.Stepped && controlPoint2.TangentOutType != CurveTangentTypes.SteppedNext;
					}
					if (!flag && flag2)
					{
						Vec2F vec2F3 = Vec2F.Normalize(m_canvas.GraphToClientTangent(controlPoint.TangentIn));
						vec2F3.X = 0f - vec2F3.X;
						vec2F3.Y = 0f - vec2F3.Y;
						Seg2F seg2 = new Seg2F(vec2F, vec2F + vec2F3 * m_tangentLength);
						if (GdiUtil.Intersects(seg2, pickRect))
						{
							points.Add(controlPoint);
							regions.Add(PointSelectionRegions.TangentIn);
						}
					}
				}
				if (singlePick && points.Count > 0)
				{
					break;
				}
			}
			if (!singlePick || points.Count <= 0)
			{
				continue;
			}
			break;
		}
	}

	public void Pick(IEnumerable<ICurve> curves, RectangleF pickRect, List<IControlPoint> points, List<PointSelectionRegions> regions, bool singlePick = false)
	{
		PickPoints(curves, pickRect, points, regions, singlePick);
		if (curves == null || points.Count != 0)
		{
			return;
		}
		foreach (ICurve curf in curves)
		{
			if (!curf.Visible || !HitTest(curf, pickRect))
			{
				continue;
			}
			foreach (IControlPoint controlPoint in curf.ControlPoints)
			{
				points.Add(controlPoint);
				regions.Add(PointSelectionRegions.Point);
			}
			if (!singlePick)
			{
				continue;
			}
			break;
		}
	}

	public bool HitTest(ICurve curve, RectangleF pickRect)
	{
		if (curve == null)
		{
			return false;
		}
		ReadOnlyCollection<IControlPoint> controlPoints = curve.ControlPoints;
		if (!curve.Visible || controlPoints.Count == 0)
		{
			return false;
		}
		float num = (float)m_tessellation / m_canvas.Zoom.X;
		ICurveEvaluator curveEvaluator = CurveUtils.CreateCurveEvaluator(curve);
		float num2 = m_canvas.ClientToGraph(pickRect.X);
		float num3 = m_canvas.ClientToGraph(pickRect.Right);
		float num4 = num3 - num2;
		PointF pointF = default(PointF);
		float x = controlPoints[0].X;
		float x2 = controlPoints[controlPoints.Count - 1].X;
		if ((num2 < x && num3 < x) || (num2 > x2 && num3 > x2))
		{
			return false;
		}
		for (float num5 = 0f; num5 < num4; num5 += num)
		{
			float x3 = num2 + num5;
			float y = curveEvaluator.Evaluate(x3);
			pointF = m_canvas.GraphToClient(x3, y);
			if (pickRect.Contains(pointF))
			{
				return true;
			}
		}
		return false;
	}

	public void DrawCurve(ICurve curve, Graphics g, float thickness = 1f)
	{
		ReadOnlyCollection<IControlPoint> controlPoints = curve.ControlPoints;
		if (controlPoints.Count == 0)
		{
			return;
		}
		m_infinityPen.Width = thickness;
		m_curvePen.Width = thickness;
		m_infinityPen.Color = curve.CurveColor;
		m_curvePen.Color = curve.CurveColor;
		if (controlPoints.Count == 1)
		{
			Vec2F vec2F = m_canvas.GraphToClient(0f, controlPoints[0].Y);
			g.DrawLine(m_infinityPen, 0f, vec2F.Y, m_canvas.ClientSize.Width, vec2F.Y);
			return;
		}
		float px = m_canvas.ClientSize.Width;
		float num = m_canvas.ClientSize.Height;
		float num2 = m_canvas.ClientToGraph(0f);
		float num3 = m_canvas.ClientToGraph(px);
		IControlPoint controlPoint = controlPoints[0];
		IControlPoint controlPoint2 = controlPoints[controlPoints.Count - 1];
		float num4 = (float)m_tessellation / m_canvas.Zoom.X;
		List<PointF> list = new List<PointF>(m_canvas.Width / m_tessellation);
		ICurveEvaluator curveEvaluator = CurveUtils.CreateCurveEvaluator(curve);
		PointF pointF = default(PointF);
		float num5 = 500f;
		float min = 0f - num5;
		float max = num + num5;
		if (controlPoint.X > num2)
		{
			float num6 = num2;
			float num7 = Math.Min(controlPoint.X, num3);
			float num8 = num7 - num6;
			for (float num9 = 0f; num9 < num8; num9 += num4)
			{
				float x = num6 + num9;
				float y = curveEvaluator.Evaluate(x);
				pointF = m_canvas.GraphToClient(x, y);
				pointF.Y = MathUtil.Clamp(pointF.Y, min, max);
				list.Add(pointF);
			}
			pointF = m_canvas.GraphToClient(num7, curveEvaluator.Evaluate(num7));
			pointF.Y = MathUtil.Clamp(pointF.Y, min, max);
			list.Add(pointF);
			if (list.Count > 1)
			{
				g.DrawLines(m_infinityPen, list.ToArray());
			}
		}
		if ((controlPoint.X > num2 || controlPoint2.X > num2) && (controlPoint.X < num3 || controlPoint2.X < num3))
		{
			ComputeIndices(curve, out var lIndex, out var rIndex);
			if (curve.CurveInterpolation == InterpolationTypes.Linear)
			{
				for (int i = lIndex; i < rIndex; i++)
				{
					IControlPoint controlPoint3 = controlPoints[i];
					IControlPoint controlPoint4 = controlPoints[i + 1];
					PointF pointF2 = m_canvas.GraphToClient(controlPoint3.X, controlPoint3.Y);
					PointF pointF3 = m_canvas.GraphToClient(controlPoint4.X, controlPoint4.Y);
					g.DrawLine(m_curvePen, pointF2.X, pointF2.Y, pointF3.X, pointF3.Y);
				}
			}
			else
			{
				for (int j = lIndex; j < rIndex; j++)
				{
					IControlPoint controlPoint5 = controlPoints[j];
					IControlPoint controlPoint6 = controlPoints[j + 1];
					if (controlPoint5.TangentOutType == CurveTangentTypes.Stepped)
					{
						PointF pointF4 = m_canvas.GraphToClient(controlPoint5.X, controlPoint5.Y);
						PointF pointF5 = m_canvas.GraphToClient(controlPoint6.X, controlPoint6.Y);
						g.DrawLine(m_curvePen, pointF4.X, pointF4.Y, pointF5.X, pointF4.Y);
						g.DrawLine(m_curvePen, pointF5.X, pointF4.Y, pointF5.X, pointF5.Y);
					}
					else if (controlPoint5.TangentOutType != CurveTangentTypes.SteppedNext)
					{
						float num10 = Math.Max(controlPoint5.X, num2);
						float num11 = Math.Min(controlPoint6.X, num3);
						list.Clear();
						float num12 = num11 - num10;
						for (float num13 = 0f; num13 < num12; num13 += num4)
						{
							float x2 = num10 + num13;
							float y2 = curveEvaluator.Evaluate(x2);
							pointF = m_canvas.GraphToClient(x2, y2);
							pointF.Y = MathUtil.Clamp(pointF.Y, min, max);
							list.Add(pointF);
						}
						pointF = m_canvas.GraphToClient(num11, curveEvaluator.Evaluate(num11));
						pointF.Y = MathUtil.Clamp(pointF.Y, min, max);
						list.Add(pointF);
						if (list.Count > 1)
						{
							g.DrawLines(m_curvePen, list.ToArray());
						}
					}
				}
			}
		}
		if (controlPoint2.X < num3)
		{
			list.Clear();
			float num14 = Math.Max(num2, controlPoint2.X);
			float num15 = num3;
			float num16 = num15 - num14;
			for (float num17 = 0f; num17 < num16; num17 += num4)
			{
				float x3 = num14 + num17;
				float y3 = curveEvaluator.Evaluate(x3);
				pointF = m_canvas.GraphToClient(x3, y3);
				pointF.Y = MathUtil.Clamp(pointF.Y, min, max);
				list.Add(pointF);
			}
			pointF = m_canvas.GraphToClient(num15, curveEvaluator.Evaluate(num15));
			pointF.Y = MathUtil.Clamp(pointF.Y, min, max);
			list.Add(pointF);
			if (list.Count > 1)
			{
				g.DrawLines(m_infinityPen, list.ToArray());
			}
		}
	}

	public void DrawControlPoints(ICurve curve, Graphics g)
	{
		ReadOnlyCollection<IControlPoint> controlPoints = curve.ControlPoints;
		ComputeIndices(curve, out var lIndex, out var rIndex);
		if (curve.CurveInterpolation == InterpolationTypes.Linear)
		{
			for (int i = lIndex; i <= rIndex; i++)
			{
				IControlPoint controlPoint = controlPoints[i];
				Vec2F vec2F = m_canvas.GraphToClient(controlPoint.X, controlPoint.Y);
				RectangleF rect = default(RectangleF);
				float num = m_pointSize / 2f;
				rect.X = vec2F.X - num;
				rect.Y = vec2F.Y - num;
				rect.Width = m_pointSize;
				rect.Height = m_pointSize;
				if (controlPoint.EditorData.SelectedRegion == PointSelectionRegions.Point)
				{
					g.FillRectangle(m_pointHiBrush, rect);
				}
				else
				{
					g.FillRectangle(m_pointBrush, rect);
				}
			}
		}
		else
		{
			for (int j = lIndex; j <= rIndex; j++)
			{
				CurveTangentTypes prevTanType = ((j == 0) ? CurveTangentTypes.Flat : controlPoints[j - 1].TangentOutType);
				DrawControlPoint(prevTanType, controlPoints[j], g);
			}
		}
	}

	public void SetCartesian2dCanvas(Cartesian2dCanvas control)
	{
		if (m_canvas != null)
		{
			throw new InvalidOperationException("curveControl is already been set");
		}
		m_canvas = control;
	}

	private void DrawControlPoint(CurveTangentTypes prevTanType, IControlPoint cp, Graphics g)
	{
		Vec2F vec2F = m_canvas.GraphToClient(cp.X, cp.Y);
		PointSelectionRegions selectedRegion = cp.EditorData.SelectedRegion;
		if (selectedRegion != PointSelectionRegions.None)
		{
			if (prevTanType != CurveTangentTypes.Stepped && prevTanType != CurveTangentTypes.SteppedNext)
			{
				Vec2F vec2F2 = Vec2F.Normalize(m_canvas.GraphToClientTangent(cp.TangentIn));
				vec2F2.X = 0f - vec2F2.X;
				vec2F2.Y = 0f - vec2F2.Y;
				DrawArrow(vec2F, vec2F + vec2F2 * m_tangentLength, g, m_tangentColor);
			}
			if (cp.TangentOutType != CurveTangentTypes.Stepped && cp.TangentOutType != CurveTangentTypes.SteppedNext)
			{
				Vec2F vec2F3 = Vec2F.Normalize(m_canvas.GraphToClientTangent(cp.TangentOut));
				DrawArrow(vec2F, vec2F + vec2F3 * m_tangentLength, g, m_tangentColor);
			}
		}
		RectangleF rect = default(RectangleF);
		float num = m_pointSize / 2f;
		rect.X = vec2F.X - num;
		rect.Y = vec2F.Y - num;
		rect.Width = m_pointSize;
		rect.Height = m_pointSize;
		if (selectedRegion == PointSelectionRegions.Point)
		{
			g.FillRectangle(m_pointHiBrush, rect);
		}
		else
		{
			g.FillRectangle(m_pointBrush, rect);
		}
	}

	private void DrawArrow(Vec2F start, Vec2F end, Graphics g, Color color)
	{
		m_tangentArrowHeadBrush.Color = color;
		m_tangentArrowLinePen.Color = color;
		g.DrawLine(m_tangentArrowLinePen, start, end);
		Vec2F vec2F = 10f * Vec2F.Normalize(start - end);
		Vec2F vec2F2 = default(Vec2F);
		Vec2F vec2F3 = default(Vec2F);
		vec2F2.X = vec2F.X * 0.8660254f - vec2F.Y * 0.5f + end.X;
		vec2F2.Y = vec2F.X * 0.5f + vec2F.Y * 0.8660254f + end.Y;
		vec2F3.X = vec2F.X * 0.8660254f + vec2F.Y * 0.5f + end.X;
		vec2F3.Y = vec2F.X * -0.5f + vec2F.Y * 0.8660254f + end.Y;
		s_arrowPts[0] = new PointF(vec2F3.X, vec2F3.Y);
		s_arrowPts[1] = new PointF(end.X, end.Y);
		s_arrowPts[2] = new PointF(vec2F2.X, vec2F2.Y);
		g.FillPolygon(m_tangentArrowHeadBrush, s_arrowPts);
	}

	private void ComputeIndices(ICurve curve, out int lIndex, out int rIndex)
	{
		lIndex = -1;
		rIndex = -2;
		ReadOnlyCollection<IControlPoint> controlPoints = curve.ControlPoints;
		if (controlPoints.Count == 0)
		{
			return;
		}
		float num = m_canvas.ClientToGraph(0f);
		float num2 = m_canvas.ClientToGraph(m_canvas.ClientSize.Width);
		if (controlPoints[0].X >= num2 || controlPoints[controlPoints.Count - 1].X <= num)
		{
			return;
		}
		for (int num3 = controlPoints.Count - 1; num3 >= 0; num3--)
		{
			IControlPoint controlPoint = controlPoints[num3];
			lIndex = num3;
			if (controlPoint.X < num)
			{
				break;
			}
		}
		for (int i = lIndex; i < controlPoints.Count; i++)
		{
			IControlPoint controlPoint2 = controlPoints[i];
			rIndex = i;
			if (controlPoint2.X > num2)
			{
				break;
			}
		}
	}

	public void Dispose()
	{
		m_pointBrush.Dispose();
		m_pointHiBrush.Dispose();
		m_tangentArrowLinePen.Dispose();
		m_tangentArrowHeadBrush.Dispose();
		m_infinityPen.Dispose();
		m_curvePen.Dispose();
	}
}
