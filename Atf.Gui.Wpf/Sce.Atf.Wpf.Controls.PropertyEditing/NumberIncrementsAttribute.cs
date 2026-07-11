using System;

namespace Sce.Atf.Wpf.Controls.PropertyEditing;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class NumberIncrementsAttribute : Attribute
{
	private readonly double m_largeChange;

	private readonly double m_defaultChange;

	private readonly double m_smallChange;

	private readonly bool m_isLogarithmic;

	public double DefaultChange => m_defaultChange;

	public double LargeChange => m_largeChange;

	public double SmallChange => m_smallChange;

	public bool IsLogarithimc => m_isLogarithmic;

	public NumberIncrementsAttribute()
	{
		m_smallChange = 0.1;
		m_defaultChange = 1.0;
		m_largeChange = 10.0;
		m_isLogarithmic = false;
	}

	public NumberIncrementsAttribute(double smallChange, double defaultChange, double largeChange)
	{
		m_smallChange = smallChange;
		m_defaultChange = defaultChange;
		m_largeChange = largeChange;
		m_isLogarithmic = false;
	}

	public NumberIncrementsAttribute(double smallChange, double defaultChange, double largeChange, bool isLogarithmic)
	{
		m_smallChange = smallChange;
		m_defaultChange = defaultChange;
		m_largeChange = largeChange;
		m_isLogarithmic = isLogarithmic;
	}

	public override bool Equals(object obj)
	{
		if (obj == this)
		{
			return true;
		}
		if (obj is NumberIncrementsAttribute numberIncrementsAttribute)
		{
			return numberIncrementsAttribute.m_smallChange == m_smallChange && numberIncrementsAttribute.m_largeChange == m_largeChange && numberIncrementsAttribute.m_defaultChange == m_defaultChange && numberIncrementsAttribute.m_isLogarithmic == m_isLogarithmic;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return m_smallChange.GetHashCode() ^ m_largeChange.GetHashCode() ^ (m_defaultChange.GetHashCode() & m_isLogarithmic.GetHashCode());
	}
}
