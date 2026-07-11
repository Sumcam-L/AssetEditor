using System;
using System.ComponentModel;
using System.Globalization;

namespace Sce.Atf.Controls.PropertyEditing;

public class BoundedFloatConverter : SingleConverter, IAnnotatedParams
{
	private float? m_min;

	private float? m_max;

	public void Initialize(string[] parameters)
	{
		if (parameters.Length < 2)
		{
			throw new ArgumentException("Can't parse bounds");
		}
		try
		{
			if (parameters[0].Length > 0)
			{
				m_min = float.Parse(parameters[0]);
			}
			if (parameters[1].Length > 0)
			{
				m_max = float.Parse(parameters[1]);
			}
		}
		catch
		{
			throw new ArgumentException("Can't parse bounds");
		}
		if (m_min.HasValue && m_max.HasValue && m_min.Value >= m_max.Value)
		{
			throw new ArgumentException("Max must be > min");
		}
	}

	public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		float num = (float)base.ConvertFrom(context, culture, value);
		if (m_min.HasValue)
		{
			num = Math.Max(num, m_min.Value);
		}
		if (m_max.HasValue)
		{
			num = Math.Min(num, m_max.Value);
		}
		return num;
	}
}
