using System;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;

namespace Sce.Atf.Applications;

internal class StringColorConverter : TypeConverter
{
	private ColorConverter m_converter = new ColorConverter();

	public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
	{
		return sourceType == typeof(string) || sourceType == typeof(Color);
	}

	public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
	{
		return destinationType == typeof(string) || destinationType == typeof(Color);
	}

	public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (value is Color)
		{
			return $"#{((Color)value).ToArgb():X}";
		}
		return value;
	}

	public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
	{
		if (destinationType == typeof(string))
		{
			if (value is string)
			{
				return value;
			}
			return $"#{((Color)value).ToArgb():X}";
		}
		if (destinationType == typeof(Color))
		{
			if (value is Color)
			{
				return value;
			}
			try
			{
				return m_converter.ConvertFromString(value as string);
			}
			catch (Exception)
			{
				return Color.Black;
			}
		}
		return null;
	}
}
