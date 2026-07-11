using System;
using System.Collections.ObjectModel;

namespace Sce.Atf.Controls.CurveEditing;

public class LinearCurveEvaluator : ICurveEvaluator
{
	private float m;

	private float b;

	private readonly ICurve m_curve;

	private ReadOnlyCollection<IControlPoint> m_points;

	private IControlPoint m_lastP1;

	public LinearCurveEvaluator(ICurve curve)
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
			{
				IControlPoint controlPoint3 = m_points[1];
				float num2 = (controlPoint3.Y - controlPoint.Y) / (controlPoint3.X - controlPoint.X);
				return controlPoint.Y - num2 * (controlPoint.X - x);
			}
			}
			float x2 = controlPoint.X;
			float x3 = controlPoint2.X;
			float num3 = x3 - x2;
			float num4 = (int)((x2 - x) / num3);
			float num5 = x2 - x - num4 * num3;
			switch (preInfinity)
			{
			case CurveLoopTypes.Cycle:
				x = x3 - num5;
				break;
			case CurveLoopTypes.CycleWithOffset:
				x = x3 - num5;
				num = 0f - (num4 + 1f) * (controlPoint2.Y - controlPoint.Y);
				break;
			case CurveLoopTypes.Oscillate:
				x = ((((int)num4 & 1) == 0) ? (x2 + num5) : (x3 - num5));
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
			{
				IControlPoint controlPoint4 = m_points[m_points.Count - 2];
				float num6 = (controlPoint2.Y - controlPoint4.Y) / (controlPoint2.X - controlPoint4.X);
				return controlPoint2.Y + num6 * (x - controlPoint2.X);
			}
			}
			float x4 = controlPoint.X;
			float x5 = controlPoint2.X;
			float num7 = x5 - x4;
			float num8 = (int)((x - x5) / num7);
			float num9 = x - x5 - num8 * num7;
			switch (postInfinity)
			{
			case CurveLoopTypes.Cycle:
				x = x4 + num9;
				break;
			case CurveLoopTypes.CycleWithOffset:
				x = x4 + num9;
				num = (num8 + 1f) * (controlPoint2.Y - controlPoint.Y);
				break;
			case CurveLoopTypes.Oscillate:
				x = ((((int)num8 & 1) == 0) ? (x5 - num9) : (x4 + num9));
				break;
			}
		}
		x = MathUtil.Clamp(x, controlPoint.X, controlPoint2.X);
		bool exactMatch;
		int num10 = FindIndex(x, out exactMatch);
		if (exactMatch)
		{
			return m_points[num10].Y + num;
		}
		IControlPoint controlPoint5 = m_points[num10];
		IControlPoint controlPoint6 = m_points[num10 + 1];
		if (m_lastP1 != controlPoint5)
		{
			m = (controlPoint6.Y - controlPoint5.Y) / (controlPoint6.X - controlPoint5.X);
			b = controlPoint5.Y - m * controlPoint5.X;
			m_lastP1 = controlPoint5;
		}
		return m * x + b + num;
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
}
