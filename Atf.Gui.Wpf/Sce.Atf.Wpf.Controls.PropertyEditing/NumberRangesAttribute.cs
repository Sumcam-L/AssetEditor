using System;

namespace Sce.Atf.Wpf.Controls.PropertyEditing;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class NumberRangesAttribute : Attribute
{
	private readonly double m_maximum;

	private readonly double m_minimum;

	private readonly double m_hardMaximum;

	private readonly double m_hardMinimum;

	private readonly double m_center;

	public double Maximum => m_maximum;

	public double Minimum => m_minimum;

	public double HardMinimum => m_hardMinimum;

	public double HardMaximum => m_hardMaximum;

	public double Center => m_center;

	public NumberRangesAttribute()
	{
		m_minimum = double.MinValue;
		m_maximum = double.MaxValue;
		m_center = double.NaN;
		m_hardMinimum = double.NaN;
		m_hardMaximum = double.NaN;
	}

	public NumberRangesAttribute(double minimum, double maximum)
	{
		m_minimum = minimum;
		m_maximum = maximum;
		m_center = minimum + (maximum - minimum) / 2.0;
		m_hardMinimum = double.NaN;
		m_hardMaximum = double.NaN;
	}

	public NumberRangesAttribute(double minimum, double maximum, double center)
	{
		m_minimum = minimum;
		m_maximum = maximum;
		m_center = center;
		m_hardMinimum = double.NaN;
		m_hardMaximum = double.NaN;
	}

	public NumberRangesAttribute(double minimum, double maximum, double center, double hardMin, double hardMax)
	{
		m_minimum = minimum;
		m_maximum = maximum;
		m_center = center;
		m_hardMinimum = hardMin;
		m_hardMaximum = hardMax;
	}

	public override bool Equals(object obj)
	{
		if (obj == this)
		{
			return true;
		}
		if (obj is NumberRangesAttribute numberRangesAttribute && numberRangesAttribute.m_minimum == m_minimum && numberRangesAttribute.m_maximum == m_maximum && numberRangesAttribute.m_hardMinimum == m_hardMinimum && numberRangesAttribute.m_hardMaximum == m_hardMaximum && numberRangesAttribute.m_center == m_center)
		{
			return true;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return m_minimum.GetHashCode() ^ m_maximum.GetHashCode() ^ m_hardMinimum.GetHashCode() ^ m_hardMaximum.GetHashCode() ^ m_center.GetHashCode();
	}
}
