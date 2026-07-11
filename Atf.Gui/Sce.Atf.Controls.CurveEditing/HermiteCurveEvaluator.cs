using System;
using System.Collections.ObjectModel;
using Sce.Atf.VectorMath;

namespace Sce.Atf.Controls.CurveEditing;

public class HermiteCurveEvaluator : ICurveEvaluator
{
	private readonly ICurve m_curve;

	private readonly float[] m_coeff = new float[4];

	private ReadOnlyCollection<IControlPoint> m_points;

	private IControlPoint m_lastP1;

	public HermiteCurveEvaluator(ICurve curve)
	{
		if (curve == null)
		{
			throw new ArgumentNullException("curve");
		}
		m_curve = curve;
		Reset();
	}

	public void Reset()
	{
		m_lastP1 = null;
		m_points = m_curve.ControlPoints;
	}

	public float Evaluate(float x)
	{
		if (m_points.Count == 0)
		{
			return 0f;
		}
		if (m_points.Count == 1)
		{
			return m_points[0].Y;
		}
		IControlPoint controlPoint = m_points[0];
		IControlPoint controlPoint2 = m_points[m_points.Count - 1];
		float num = 0f;
		if (x < controlPoint.X)
		{
			CurveLoopTypes preInfinity = m_curve.PreInfinity;
			switch (preInfinity)
			{
			case CurveLoopTypes.Constant:
				return controlPoint.Y;
			case CurveLoopTypes.Linear:
				return controlPoint.Y - controlPoint.TangentIn.Y / controlPoint.TangentIn.X * (controlPoint.X - x);
			}
			float x2 = controlPoint.X;
			float x3 = controlPoint2.X;
			float num2 = x3 - x2;
			float num3 = (int)((x2 - x) / num2);
			float num4 = x2 - x - num3 * num2;
			switch (preInfinity)
			{
			case CurveLoopTypes.Cycle:
				x = x3 - num4;
				break;
			case CurveLoopTypes.CycleWithOffset:
				x = x3 - num4;
				num = 0f - (num3 + 1f) * (controlPoint2.Y - controlPoint.Y);
				break;
			case CurveLoopTypes.Oscillate:
				x = ((((int)num3 & 1) == 0) ? (x2 + num4) : (x3 - num4));
				break;
			}
		}
		else if (x > controlPoint2.X)
		{
			CurveLoopTypes postInfinity = m_curve.PostInfinity;
			switch (postInfinity)
			{
			case CurveLoopTypes.Constant:
				return controlPoint2.Y;
			case CurveLoopTypes.Linear:
				return controlPoint2.Y + controlPoint2.TangentOut.Y / controlPoint2.TangentOut.X * (x - controlPoint2.X);
			}
			float x4 = controlPoint.X;
			float x5 = controlPoint2.X;
			float num5 = x5 - x4;
			float num6 = (int)((x - x5) / num5);
			float num7 = x - x5 - num6 * num5;
			switch (postInfinity)
			{
			case CurveLoopTypes.Cycle:
				x = x4 + num7;
				break;
			case CurveLoopTypes.CycleWithOffset:
				x = x4 + num7;
				num = (num6 + 1f) * (controlPoint2.Y - controlPoint.Y);
				break;
			case CurveLoopTypes.Oscillate:
				x = ((((int)num6 & 1) == 0) ? (x5 - num7) : (x4 + num7));
				break;
			}
		}
		x = MathUtil.Clamp(x, controlPoint.X, controlPoint2.X);
		bool exactMatch;
		int num8 = FindIndex(x, out exactMatch);
		if (exactMatch)
		{
			return m_points[num8].Y + num;
		}
		IControlPoint controlPoint3 = m_points[num8];
		IControlPoint controlPoint4 = m_points[num8 + 1];
		if (controlPoint3.TangentOut.X == 0f && controlPoint3.TangentOut.Y == 0f)
		{
			return controlPoint3.Y + num;
		}
		if (controlPoint3.TangentOut.X == float.MaxValue && controlPoint3.TangentOut.Y == float.MaxValue)
		{
			return controlPoint4.Y + num;
		}
		if (m_lastP1 != controlPoint3)
		{
			ComputeHermiteCoeff(controlPoint3, controlPoint4, m_coeff);
			m_lastP1 = controlPoint3;
		}
		return CubicPolyEval(controlPoint3.X, x, m_coeff) + num;
	}

	private int FindIndex(float x, out bool exactMatch)
	{
		exactMatch = false;
		int num = 0;
		int num2 = m_points.Count - 1;
		do
		{
			int num3 = num + num2 >> 1;
			if (x < m_points[num3].X)
			{
				num2 = num3 - 1;
				continue;
			}
			if (x > m_points[num3].X)
			{
				num = num3 + 1;
				continue;
			}
			exactMatch = true;
			return num3;
		}
		while (num <= num2);
		return num2;
	}

	private float CubicPolyEval(float x0, float x, float[] Coeff)
	{
		float num = x - x0;
		return num * (num * (num * Coeff[0] + Coeff[1]) + Coeff[2]) + Coeff[3];
	}

	private void ComputeHermiteCoeff(IControlPoint p1, IControlPoint p2, float[] Coeff)
	{
		Vec2F vec2F = new Vec2F(p1.TangentOut.X, p1.TangentOut.Y);
		Vec2F vec2F2 = new Vec2F(p2.TangentIn.X, p2.TangentIn.Y);
		float num = 0f;
		if (vec2F.X != 0f)
		{
			num = vec2F.Y / vec2F.X;
		}
		float num2 = 0f;
		if (vec2F2.X != 0f)
		{
			num2 = vec2F2.Y / vec2F2.X;
		}
		float num3 = p2.X - p1.X;
		float num4 = p2.Y - p1.Y;
		float num5 = 1f / (num3 * num3);
		float num6 = num3 * num;
		float num7 = num3 * num2;
		Coeff[0] = (num6 + num7 - num4 - num4) * num5 / num3;
		Coeff[1] = (num4 + num4 + num4 - num6 - num6 - num7) * num5;
		Coeff[2] = num;
		Coeff[3] = p1.Y;
	}
}
