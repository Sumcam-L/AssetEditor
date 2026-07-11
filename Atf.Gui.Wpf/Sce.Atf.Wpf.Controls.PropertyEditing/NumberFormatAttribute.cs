using System;

namespace Sce.Atf.Wpf.Controls.PropertyEditing;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class NumberFormatAttribute : Attribute
{
	private readonly string m_formatString;

	private readonly int? m_maxPrecision;

	private readonly double? m_scale;

	public string FormatString => m_formatString;

	public int? MaxPrecision => m_maxPrecision;

	public double? Scale => m_scale;

	public NumberFormatAttribute()
	{
		m_formatString = null;
		m_maxPrecision = null;
		m_scale = null;
	}

	public NumberFormatAttribute(string formatString)
	{
		m_formatString = formatString;
		m_maxPrecision = null;
		m_scale = null;
	}

	public NumberFormatAttribute(string formatString, int maxPrecision, double scale)
	{
		m_formatString = formatString;
		m_maxPrecision = maxPrecision;
		m_scale = scale;
	}

	public override bool Equals(object obj)
	{
		if (obj == this)
		{
			return true;
		}
		if (obj is NumberFormatAttribute numberFormatAttribute && numberFormatAttribute.m_formatString == m_formatString && numberFormatAttribute.m_maxPrecision == m_maxPrecision)
		{
			return numberFormatAttribute.m_scale == m_scale;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return ((m_formatString != null) ? m_formatString.GetHashCode() : 0) ^ m_maxPrecision.GetHashCode() ^ m_scale.GetHashCode();
	}
}
