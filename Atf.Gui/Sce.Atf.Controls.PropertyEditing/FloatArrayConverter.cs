using System;
using System.ComponentModel;
using System.Globalization;
using System.Text;

namespace Sce.Atf.Controls.PropertyEditing;

public class FloatArrayConverter : TypeConverter
{
	private string m_format = "0.####";

	private float m_scaleFactor = 1f;

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

	public double ScaleFactor
	{
		get
		{
			return m_scaleFactor;
		}
		set
		{
			if (value == 0.0)
			{
				throw new ArgumentException("value must be non-zero");
			}
			m_scaleFactor = (float)value;
		}
	}

	public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
	{
		if (sourceType == typeof(string))
		{
			return true;
		}
		return base.CanConvertFrom(context, sourceType);
	}

	public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (value is string text)
		{
			string[] array = text.Split(',');
			float[] array2 = new float[array.Length];
			for (int i = 0; i < array.Length; i++)
			{
				if (!float.TryParse(array[i], NumberStyles.Float, CultureInfo.CurrentCulture, out var result))
				{
					throw new ArgumentException("Error converting " + text + " to float[]");
				}
				array2[i] = result / m_scaleFactor;
			}
			return array2;
		}
		return base.ConvertFrom(context, culture, value);
	}

	public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
	{
		if (culture == null)
		{
			culture = CultureInfo.CurrentCulture;
		}
		string listSeparator = culture.TextInfo.ListSeparator;
		if (value is float[] array)
		{
			StringBuilder stringBuilder = new StringBuilder();
			float[] array2 = array;
			foreach (float num in array2)
			{
				stringBuilder.Append((num * m_scaleFactor).ToString(m_format, culture));
				stringBuilder.Append(listSeparator);
			}
			if (stringBuilder.Length > 0)
			{
				stringBuilder.Length -= listSeparator.Length;
			}
			return stringBuilder.ToString();
		}
		return base.ConvertTo(context, culture, value, destinationType);
	}
}
