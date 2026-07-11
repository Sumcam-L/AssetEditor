namespace Sce.Atf.Dom;

public class NumericMinRule : AttributeRule
{
	private readonly double m_minimum;

	private readonly bool m_inclusive;

	public NumericMinRule(double minimum, bool inclusive)
	{
		m_minimum = minimum;
		m_inclusive = inclusive;
	}

	public override bool Validate(object value, AttributeInfo info)
	{
		if (value is byte b)
		{
			return (double)(int)b > m_minimum || (m_inclusive && (double)(int)b == m_minimum);
		}
		if (value is sbyte b2)
		{
			return (double)b2 > m_minimum || (m_inclusive && (double)b2 == m_minimum);
		}
		if (value is ushort num)
		{
			return (double)(int)num > m_minimum || (m_inclusive && (double)(int)num == m_minimum);
		}
		if (value is short num2)
		{
			return (double)num2 > m_minimum || (m_inclusive && (double)num2 == m_minimum);
		}
		if (value is uint num3)
		{
			return (double)num3 > m_minimum || (m_inclusive && (double)num3 == m_minimum);
		}
		if (value is int num4)
		{
			return (double)num4 > m_minimum || (m_inclusive && (double)num4 == m_minimum);
		}
		if (value is ulong num5)
		{
			return (double)num5 > m_minimum || (m_inclusive && (double)num5 == m_minimum);
		}
		if (value is long num6)
		{
			return (double)num6 > m_minimum || (m_inclusive && (double)num6 == m_minimum);
		}
		if (value is float num7)
		{
			return (double)num7 > m_minimum || (m_inclusive && (double)num7 == m_minimum);
		}
		if (value is double num8)
		{
			return num8 > m_minimum || (m_inclusive && num8 == m_minimum);
		}
		return false;
	}
}
