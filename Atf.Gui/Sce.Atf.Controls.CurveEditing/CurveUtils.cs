using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Sce.Atf.VectorMath;

namespace Sce.Atf.Controls.CurveEditing;

public static class CurveUtils
{
	private static List<IControlPoint> s_points = new List<IControlPoint>();

	private static float s_epsilone = 0.001f;

	private static float MaxTan = 1000000f;

	public static float Epsilone
	{
		get
		{
			return s_epsilone;
		}
		set
		{
			if (value < float.Epsilon)
			{
				throw new ArgumentOutOfRangeException("value");
			}
			s_epsilone = value;
		}
	}

	public static bool IsValid(ICurve curve)
	{
		if (curve == null)
		{
			return false;
		}
		ReadOnlyCollection<IControlPoint> controlPoints = curve.ControlPoints;
		if (controlPoints.Count < 2)
		{
			return true;
		}
		int count = controlPoints.Count;
		IControlPoint controlPoint = controlPoints[0];
		for (int i = 1; i < count; i++)
		{
			IControlPoint controlPoint2 = controlPoints[i];
			if (controlPoint2.X - controlPoint.X < s_epsilone)
			{
				return false;
			}
			controlPoint = controlPoint2;
		}
		return true;
	}

	public static ICurveEvaluator CreateCurveEvaluator(ICurve curve)
	{
		ICurveEvaluator curveEvaluator = null;
		if (curve.CurveInterpolation == InterpolationTypes.Linear)
		{
			return new LinearCurveEvaluator(curve);
		}
		if (curve.CurveInterpolation == InterpolationTypes.Hermite)
		{
			return new HermiteCurveEvaluator(curve);
		}
		throw new NotImplementedException("CurveEvaluator not implement for " + curve.CurveInterpolation);
	}

	public static void OffsetCurve(ICurve curve, float x, float y)
	{
		foreach (IControlPoint controlPoint in curve.ControlPoints)
		{
			controlPoint.X += x;
			controlPoint.Y += y;
		}
	}

	public static void ForceMinDistance(ICurve curve)
	{
		ForceMinDistance(curve, s_epsilone);
	}

	public static void ForceMinDistance(ICurve curve, float dist)
	{
		if (curve == null)
		{
			return;
		}
		ReadOnlyCollection<IControlPoint> controlPoints = curve.ControlPoints;
		if (controlPoints.Count < 2)
		{
			return;
		}
		int num = controlPoints.Count - 1;
		for (int i = 0; i < num; i++)
		{
			if (controlPoints[i + 1].X - controlPoints[i].X < dist)
			{
				controlPoints[i + 1].X = controlPoints[i].X + dist;
			}
		}
	}

	public static void ComputeTangent(ICurve curve)
	{
		if (curve == null)
		{
			return;
		}
		ReadOnlyCollection<IControlPoint> controlPoints = curve.ControlPoints;
		if (controlPoints.Count == 0)
		{
			return;
		}
		if (controlPoints.Count == 1)
		{
			Vec2F vec2F = new Vec2F(1f, 0f);
			IControlPoint controlPoint = controlPoints[0];
			if (controlPoint.TangentIn != vec2F)
			{
				controlPoint.TangentIn = vec2F;
			}
			if (controlPoint.TangentOut != vec2F)
			{
				controlPoint.TangentOut = vec2F;
			}
		}
		else
		{
			for (int i = 0; i < controlPoints.Count; i++)
			{
				ComputeTangentIn(curve, i);
				ComputeTangentOut(curve, i);
			}
		}
	}

	public static void ComputeTangentOut(ICurve curve, int index)
	{
		if (curve == null)
		{
			return;
		}
		ReadOnlyCollection<IControlPoint> controlPoints = curve.ControlPoints;
		if (index < 0 || index >= controlPoints.Count)
		{
			return;
		}
		IControlPoint controlPoint = controlPoints[index];
		if (controlPoint.TangentOutType == CurveTangentTypes.Fixed)
		{
			return;
		}
		Vec2F tan = new Vec2F(0f, 0f);
		int num = controlPoints.Count - 1;
		IControlPoint controlPoint2 = ((index > 0) ? controlPoints[index - 1] : null);
		IControlPoint controlPoint3 = ((index < num) ? controlPoints[index + 1] : null);
		if (controlPoint.TangentOutType == CurveTangentTypes.Clamped && controlPoint3 != null)
		{
			float num2 = controlPoint3.Y - controlPoint.Y;
			if (num2 < 0f)
			{
				num2 = 0f - num2;
			}
			float num3 = ((controlPoint2 == null) ? num2 : (controlPoint2.Y - controlPoint.Y));
			if (num3 < 0f)
			{
				num3 = 0f - num3;
			}
			if (num2 <= 0.05f || num3 <= 0.05f)
			{
				controlPoint.TangentOutType = CurveTangentTypes.Flat;
			}
		}
		switch (controlPoint.TangentOutType)
		{
		case CurveTangentTypes.Spline:
		case CurveTangentTypes.Clamped:
			if (controlPoint.TangentOutType == CurveTangentTypes.Clamped)
			{
				controlPoint.TangentOutType = CurveTangentTypes.Spline;
			}
			if (controlPoint2 == null && controlPoint3 != null)
			{
				tan.X = controlPoint3.X - controlPoint.X;
				tan.Y = controlPoint3.Y - controlPoint.Y;
			}
			else if (controlPoint2 != null && controlPoint3 == null)
			{
				tan.X = controlPoint.X - controlPoint2.X;
				tan.Y = controlPoint.Y - controlPoint2.Y;
			}
			else if (controlPoint2 != null && controlPoint3 != null)
			{
				float num4 = controlPoint3.X - controlPoint2.X;
				float num5 = 0f;
				num5 = ((!(num4 < s_epsilone)) ? ((controlPoint3.Y - controlPoint2.Y) / num4) : MaxTan);
				tan.X = controlPoint3.X - controlPoint.X;
				tan.Y = num5 * tan.X;
			}
			else
			{
				tan = new Vec2F(1f, 0f);
			}
			break;
		case CurveTangentTypes.Linear:
			tan = ((controlPoint3 != null) ? new Vec2F(controlPoint3.X - controlPoint.X, controlPoint3.Y - controlPoint.Y) : new Vec2F(1f, 0f));
			break;
		case CurveTangentTypes.Stepped:
			tan = new Vec2F(0f, 0f);
			break;
		case CurveTangentTypes.SteppedNext:
			tan = new Vec2F(float.MaxValue, float.MaxValue);
			break;
		case CurveTangentTypes.Flat:
			if (controlPoint3 == null)
			{
				float x = ((controlPoint2 == null) ? 0f : (controlPoint.X - controlPoint2.X));
				tan = new Vec2F(x, 0f);
			}
			else
			{
				tan = new Vec2F(controlPoint3.X - controlPoint.X, 0f);
			}
			break;
		default:
			throw new NotImplementedException(controlPoint.TangentOutType.ToString());
		}
		tan = EnsureValidTangent(tan, isWeighted: false);
		if (controlPoint.TangentOut != tan)
		{
			controlPoint.TangentOut = tan;
		}
	}

	public static void ComputeTangentIn(ICurve curve, int index)
	{
		if (curve == null)
		{
			return;
		}
		ReadOnlyCollection<IControlPoint> controlPoints = curve.ControlPoints;
		if (index < 0 || index >= controlPoints.Count)
		{
			return;
		}
		IControlPoint controlPoint = controlPoints[index];
		if (controlPoint.TangentInType == CurveTangentTypes.Fixed)
		{
			return;
		}
		Vec2F tan = new Vec2F(0f, 0f);
		int num = controlPoints.Count - 1;
		IControlPoint controlPoint2 = ((index > 0) ? controlPoints[index - 1] : null);
		IControlPoint controlPoint3 = ((index < num) ? controlPoints[index + 1] : null);
		if (controlPoint.TangentInType == CurveTangentTypes.Clamped && controlPoint2 != null)
		{
			float num2 = controlPoint2.Y - controlPoint.Y;
			if (num2 < 0f)
			{
				num2 = 0f - num2;
			}
			float num3 = ((controlPoint3 == null) ? num2 : (controlPoint3.Y - controlPoint.Y));
			if (num3 < 0f)
			{
				num3 = 0f - num3;
			}
			if (num3 <= 0.05f || num2 <= 0.05f)
			{
				controlPoint.TangentInType = CurveTangentTypes.Flat;
			}
		}
		switch (controlPoint.TangentInType)
		{
		case CurveTangentTypes.Spline:
		case CurveTangentTypes.Clamped:
			if (controlPoint.TangentInType == CurveTangentTypes.Clamped)
			{
				controlPoint.TangentInType = CurveTangentTypes.Spline;
			}
			if (controlPoint2 == null && controlPoint3 != null)
			{
				tan.X = controlPoint3.X - controlPoint.X;
				tan.Y = controlPoint3.Y - controlPoint.Y;
			}
			else if (controlPoint2 != null && controlPoint3 == null)
			{
				tan.X = controlPoint.X - controlPoint2.X;
				tan.Y = controlPoint.Y - controlPoint2.Y;
			}
			else if (controlPoint2 != null && controlPoint3 != null)
			{
				float num4 = controlPoint3.X - controlPoint2.X;
				float num5 = 0f;
				num5 = ((!(num4 < s_epsilone)) ? ((controlPoint3.Y - controlPoint2.Y) / num4) : MaxTan);
				tan.X = controlPoint.X - controlPoint2.X;
				tan.Y = num5 * tan.X;
			}
			else
			{
				tan = new Vec2F(1f, 0f);
			}
			break;
		case CurveTangentTypes.Linear:
			tan = ((controlPoint2 != null) ? new Vec2F(controlPoint.X - controlPoint2.X, controlPoint.Y - controlPoint2.Y) : new Vec2F(1f, 0f));
			break;
		case CurveTangentTypes.Stepped:
			tan = new Vec2F(0f, 0f);
			break;
		case CurveTangentTypes.SteppedNext:
			tan = new Vec2F(float.MaxValue, float.MaxValue);
			break;
		case CurveTangentTypes.Flat:
			if (controlPoint2 == null)
			{
				float x = ((controlPoint3 == null) ? 0f : (controlPoint3.X - controlPoint.X));
				tan = new Vec2F(x, 0f);
			}
			else
			{
				tan = new Vec2F(controlPoint.X - controlPoint2.X, 0f);
			}
			break;
		default:
			throw new NotImplementedException(controlPoint.TangentInType.ToString());
		case CurveTangentTypes.Fixed:
			break;
		}
		tan = EnsureValidTangent(tan, isWeighted: false);
		if (controlPoint.TangentIn != tan)
		{
			controlPoint.TangentIn = tan;
		}
	}

	public static bool IsSorted(IControlPoint cp)
	{
		bool flag = true;
		ICurve parent = cp.Parent;
		int num = parent.ControlPoints.IndexOf(cp);
		if (num == -1)
		{
			throw new ArgumentException("cp not found in parent curve");
		}
		int num2 = parent.ControlPoints.Count - 1;
		if (num < num2)
		{
			IControlPoint controlPoint = parent.ControlPoints[num + 1];
			if (cp.X > controlPoint.X)
			{
				flag = false;
			}
		}
		if (flag && num > 0)
		{
			IControlPoint controlPoint2 = parent.ControlPoints[num - 1];
			if (cp.X < controlPoint2.X)
			{
				flag = false;
			}
		}
		return flag;
	}

	public static void Sort(ICurve curve)
	{
		if (curve == null)
		{
			return;
		}
		ReadOnlyCollection<IControlPoint> controlPoints = curve.ControlPoints;
		if (controlPoints.Count < 2)
		{
			return;
		}
		try
		{
			s_points.Clear();
			s_points.AddRange(controlPoints);
			for (int i = 1; i < s_points.Count; i++)
			{
				IControlPoint controlPoint = s_points[i];
				int num = i - 1;
				while (num >= 0 && s_points[num].X > controlPoint.X)
				{
					s_points[num + 1] = s_points[num];
					num--;
				}
				s_points[num + 1] = controlPoint;
			}
			for (int j = 0; j < s_points.Count; j++)
			{
				IControlPoint controlPoint2 = s_points[j];
				IControlPoint controlPoint3 = controlPoints[j];
				if (controlPoint2 != controlPoint3)
				{
					curve.InsertControlPoint(j, controlPoint2);
				}
			}
		}
		finally
		{
			s_points.Clear();
		}
	}

	public static void AddControlPoint(ICurve curve, Vec2F gpt, bool insert, bool computeTangent)
	{
		if (curve == null)
		{
			throw new ArgumentNullException("curve");
		}
		int validInsertionIndex = GetValidInsertionIndex(curve, gpt.X);
		if (validInsertionIndex >= 0)
		{
			IControlPoint controlPoint = curve.CreateControlPoint();
			controlPoint.EditorData.SelectedRegion = PointSelectionRegions.None;
			if (insert)
			{
				controlPoint.X = gpt.X;
				ICurveEvaluator curveEvaluator = CreateCurveEvaluator(curve);
				float num = curveEvaluator.Evaluate(gpt.X - s_epsilone);
				float num2 = curveEvaluator.Evaluate(gpt.X + s_epsilone);
				controlPoint.Y = curveEvaluator.Evaluate(gpt.X);
				controlPoint.TangentInType = CurveTangentTypes.Fixed;
				controlPoint.TangentOutType = CurveTangentTypes.Fixed;
				Vec2F tangentIn = new Vec2F(s_epsilone, controlPoint.Y - num);
				tangentIn.Normalize();
				Vec2F tangentOut = new Vec2F(s_epsilone, num2 - controlPoint.Y);
				tangentOut.Normalize();
				controlPoint.TangentIn = tangentIn;
				controlPoint.TangentOut = tangentOut;
			}
			else
			{
				controlPoint.X = gpt.X;
				controlPoint.Y = gpt.Y;
				controlPoint.TangentInType = CurveTangentTypes.Spline;
				controlPoint.TangentOutType = CurveTangentTypes.Spline;
			}
			curve.InsertControlPoint(validInsertionIndex, controlPoint);
			if (computeTangent)
			{
				ComputeTangent(curve);
			}
		}
	}

	public static void AddControlPoint(ICurve curve, Vec2F gpt, bool insert)
	{
		AddControlPoint(curve, gpt, insert, computeTangent: true);
	}

	public static float SnapTo(float x, float y)
	{
		double num = Math.Round(x / y);
		return (float)(num * (double)y);
	}

	public static int GetValidInsertionIndex(ICurve curve, float x)
	{
		ReadOnlyCollection<IControlPoint> controlPoints = curve.ControlPoints;
		if (controlPoints.Count == 0)
		{
			return 0;
		}
		IControlPoint controlPoint = controlPoints[controlPoints.Count - 1];
		if (x - controlPoint.X > s_epsilone)
		{
			return controlPoints.Count;
		}
		for (int i = 0; i < controlPoints.Count; i++)
		{
			IControlPoint controlPoint2 = controlPoints[i];
			if (x - controlPoint2.X < 0f - s_epsilone)
			{
				return i;
			}
			if (!(x - controlPoint2.X > s_epsilone))
			{
				return -1;
			}
		}
		return controlPoints.Count;
	}

	private static Vec2F EnsureValidTangent(Vec2F tan, bool isWeighted)
	{
		Vec2F result = tan;
		if (result.X == float.MaxValue && result.Y == float.MaxValue)
		{
			return result;
		}
		if (result.X < 0f)
		{
			result.X = 0f;
		}
		if (isWeighted)
		{
			return result;
		}
		float length = result.Length;
		if (length != 0f)
		{
			result.X /= length;
			result.Y /= length;
		}
		if (result.X == 0f && result.Y != 0f)
		{
			result.X = s_epsilone;
			result.Y = ((result.Y < 0f) ? (-1f) : 1f) * (result.X * MaxTan);
		}
		return result;
	}
}
