using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Sce.Atf.VectorMath;

public class BezierSpline : Collection<BezierPoint>, IFormattable
{
	private readonly IList<BezierCurve> m_curves = new List<BezierCurve>();

	private bool m_isClosed = true;

	private bool m_dirty = true;

	public IList<BezierCurve> Curves
	{
		get
		{
			if (m_dirty)
			{
				BuildCurves();
				m_dirty = false;
			}
			return m_curves;
		}
	}

	public bool IsClosed
	{
		get
		{
			return m_isClosed;
		}
		set
		{
			m_isClosed = value;
		}
	}

	protected override void ClearItems()
	{
		base.ClearItems();
		m_dirty = true;
	}

	protected override void InsertItem(int index, BezierPoint pt)
	{
		base.InsertItem(index, pt);
		m_dirty = true;
	}

	protected override void RemoveItem(int index)
	{
		base.RemoveItem(index);
		m_dirty = true;
	}

	protected override void SetItem(int index, BezierPoint pt)
	{
		base.SetItem(index, pt);
		m_dirty = true;
	}

	private void BuildCurves()
	{
		m_curves.Clear();
		if (base.Count <= 1)
		{
			return;
		}
		if (base.Count == 2)
		{
			BuildInitialCurveFrom2Points();
			return;
		}
		Vec3F vec3F = new Vec3F(0f, 0f, 0f);
		Vec3F[] array = CalcPointTangents();
		for (int i = 1; i < base.Count; i++)
		{
			Vec3F vec3F2 = base[i].Position - base[i - 1].Position;
			float num = vec3F2.Length * 0.333f;
			Vec3F[] array2 = new Vec3F[4]
			{
				base[i - 1].Position,
				default(Vec3F),
				default(Vec3F),
				base[i].Position
			};
			if (base[i - 1].Tangent2 != vec3F)
			{
				array2[1] = base[i - 1].Position + base[i - 1].Tangent2;
			}
			else
			{
				Vec3F vec3F3 = array[i - 1];
				if (Vec3F.Dot(vec3F2, vec3F3) < 0f)
				{
					vec3F3 = -vec3F3;
				}
				array2[1] = base[i - 1].Position + vec3F3 * num;
			}
			if (base[i].Tangent1 != vec3F)
			{
				array2[2] = base[i].Position + base[i].Tangent1;
			}
			else
			{
				Vec3F vec3F4 = array[i];
				if (Vec3F.Dot(-vec3F2, vec3F4) < 0f)
				{
					vec3F4 = -vec3F4;
				}
				array2[2] = base[i].Position + vec3F4 * num;
			}
			BezierCurve item = new BezierCurve(array2);
			m_curves.Add(item);
		}
		if (m_isClosed)
		{
			Vec3F[] array3 = new Vec3F[4]
			{
				base[base.Count - 1].Position,
				default(Vec3F),
				default(Vec3F),
				base[0].Position
			};
			float num2 = (array3[3] - array3[0]).Length / 3f;
			Vec3F vec3F5 = m_curves[m_curves.Count - 1].ControlPoints[2] - array3[0];
			vec3F5 /= vec3F5.Length;
			array3[1] = array3[0] - vec3F5 * num2;
			vec3F5 = m_curves[0].ControlPoints[1] - array3[3];
			vec3F5 /= vec3F5.Length;
			array3[2] = array3[3] - vec3F5 * num2;
			BezierCurve item2 = new BezierCurve(array3);
			m_curves.Add(item2);
		}
	}

	private void BuildInitialCurveFrom2Points()
	{
		Vec3F[] array = new Vec3F[4];
		array[0] = base[0].Position;
		array[3] = base[1].Position;
		array[1] = array[0] + (array[3] - array[0]) * 0.333f;
		array[2] = array[0] + (array[3] - array[0]) * 0.666f;
		m_curves.Add(new BezierCurve(array));
	}

	private Vec3F CalcTangent(Vec3F p1, Vec3F p2, Vec3F p3)
	{
		Vec3F vec3F = (p1 - p2) / (p1 - p2).Length;
		Vec3F vec3F2 = (p3 - p2) / (p3 - p2).Length;
		Vec3F v = vec3F + vec3F2;
		Vec3F v2 = Vec3F.Cross(vec3F, vec3F2);
		if (v2.Length < 1E-06f)
		{
			return vec3F2;
		}
		Vec3F vec3F3 = Vec3F.Cross(v2, v);
		return vec3F3 / vec3F3.Length;
	}

	private Vec3F CalcEndTangents(Vec3F p1, Vec3F p2, Vec3F p2Tangent)
	{
		Vec3F vec3F = p1 - p2;
		float length = vec3F.Length;
		Vec3F v = vec3F / length;
		if (Vec3F.Dot(vec3F, p2Tangent) < 0f)
		{
			p2Tangent = -p2Tangent;
		}
		float num = 0.5f * length / Vec3F.Dot(p2Tangent, v);
		Vec3F vec3F2 = p2 + p2Tangent * num;
		return (vec3F2 - p1) / (vec3F2 - p1).Length;
	}

	private Vec3F[] CalcPointTangents()
	{
		int count = base.Count;
		Vec3F[] array = new Vec3F[count];
		for (int i = 1; i < count - 1; i++)
		{
			array[i] = CalcTangent(base[i - 1].Position, base[i].Position, base[i + 1].Position);
		}
		if (m_isClosed)
		{
			array[0] = CalcTangent(base[count - 1].Position, base[0].Position, base[1].Position);
			array[count - 1] = CalcTangent(base[count - 2].Position, base[count - 1].Position, base[0].Position);
		}
		else
		{
			array[0] = CalcEndTangents(base[0].Position, base[1].Position, array[1]);
			array[count - 1] = CalcEndTangents(base[count - 2].Position, base[count - 1].Position, array[count - 2]);
		}
		return array;
	}

	public override string ToString()
	{
		return ToString(null, null);
	}

	public string ToString(string format, IFormatProvider formatProvider)
	{
		string value = StringUtil.GetNumberListSeparator(formatProvider) + " ";
		if (format == null)
		{
			format = "R";
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(base.Count.ToString("D", formatProvider));
		for (int i = 0; i < base.Count; i++)
		{
			stringBuilder.Append(value);
			stringBuilder.Append(base[i].ToString(format, formatProvider));
		}
		return stringBuilder.ToString();
	}
}
