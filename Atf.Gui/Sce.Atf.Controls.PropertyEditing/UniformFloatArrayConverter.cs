using System;
using System.ComponentModel;
using System.Globalization;

namespace Sce.Atf.Controls.PropertyEditing;

public class UniformFloatArrayConverter : TypeConverter
{
	private string m_format = "0.####";

	private int m_arrayLength = 3;

	public string Format
	{
		get
		{
			return m_format;
		}
		set
		{
			m_format = value;
		}
	}

	public int ArrayLength
	{
		get
		{
			return m_arrayLength;
		}
		set
		{
			m_arrayLength = value;
		}
	}

	public override bool CanConvertFrom(ITypeDescriptorContext context, Type t)
	{
		if (t == typeof(string))
		{
			return true;
		}
		return base.CanConvertFrom(context, t);
	}

	public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo info, object value)
	{
		if (value is string text)
		{
			if (!float.TryParse(text, NumberStyles.Float, CultureInfo.CurrentCulture, out var result))
			{
				throw new ArgumentException("Error converting " + text + " to uniform float[]");
			}
			float[] array = new float[m_arrayLength];
			for (int i = 0; i < m_arrayLength; i++)
			{
				array[i] = result;
			}
			return array;
		}
		return base.ConvertFrom(context, info, value);
	}

	public override bool CanConvertTo(ITypeDescriptorContext context, Type t)
	{
		if (t == typeof(string))
		{
			return true;
		}
		return base.CanConvertTo(context, t);
	}

	public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destType)
	{
		if (destType == typeof(string) && value is float[])
		{
			float[] array = (float[])value;
			if (array.Length != 0)
			{
				return array[0].ToString(m_format, CultureInfo.CurrentCulture);
			}
		}
		return base.ConvertTo(context, culture, value, destType);
	}
}
