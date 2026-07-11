using System;

namespace Sce.Atf.VectorMath;

public class BezierCurve : IFormattable
{
	private readonly Vec3F[] m_ctrlPoints;

	private readonly Vec3F[] m_coefficients;

	public Vec3F[] ControlPoints => m_ctrlPoints;

	public Vec3F[] Coefficients => m_coefficients;

	public BezierCurve(Vec3F[] controlPoints)
	{
		m_ctrlPoints = controlPoints;
		m_coefficients = new Vec3F[3];
		m_coefficients[2] = 3f * (m_ctrlPoints[1] - m_ctrlPoints[0]);
		m_coefficients[1] = 3f * (m_ctrlPoints[2] - m_ctrlPoints[1]) - m_coefficients[2];
		m_coefficients[0] = m_ctrlPoints[3] - m_ctrlPoints[0] - m_coefficients[2] - m_coefficients[1];
	}

	public Vec3F Evaluate(float t)
	{
		Vec3F vec3F = default(Vec3F);
		float num = t * t;
		float num2 = num * t;
		return m_coefficients[0] * num2 + m_coefficients[1] * num + m_coefficients[2] * t + m_ctrlPoints[0];
	}

	public override string ToString()
	{
		return ToString(null, null);
	}

	public string ToString(string format, IFormatProvider formatProvider)
	{
		string numberListSeparator = StringUtil.GetNumberListSeparator(formatProvider);
		if (format == null)
		{
			format = "R";
		}
		return string.Format("{1}{0} {2}{0} {3}{0} {4}{0} {5}{0} {6}{0} {7}{0} {8}{0} {9}{0} {10}{0} {11}{0} {12}", numberListSeparator, m_ctrlPoints[0].X.ToString(format, formatProvider), m_ctrlPoints[0].Y.ToString(format, formatProvider), m_ctrlPoints[0].Z.ToString(format, formatProvider), m_ctrlPoints[1].X.ToString(format, formatProvider), m_ctrlPoints[1].Y.ToString(format, formatProvider), m_ctrlPoints[1].Z.ToString(format, formatProvider), m_ctrlPoints[2].X.ToString(format, formatProvider), m_ctrlPoints[2].Y.ToString(format, formatProvider), m_ctrlPoints[2].Z.ToString(format, formatProvider), m_ctrlPoints[3].X.ToString(format, formatProvider), m_ctrlPoints[3].Y.ToString(format, formatProvider), m_ctrlPoints[3].Z.ToString(format, formatProvider));
	}
}
