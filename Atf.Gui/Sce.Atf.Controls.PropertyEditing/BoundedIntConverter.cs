using System;
using System.ComponentModel;
using System.Globalization;

namespace Sce.Atf.Controls.PropertyEditing;

public class BoundedIntConverter : Int32Converter, IAnnotatedParams
{
	private int? m_min;

	private int? m_max;

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
				m_min = int.Parse(parameters[0]);
			}
			if (parameters[1].Length > 0)
			{
				m_max = int.Parse(parameters[1]);
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
		int num = (int)base.ConvertFrom(context, culture, value);
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
