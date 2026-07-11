using System;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;

namespace Sce.Atf.Applications;

internal class DefaultTypeConverter : TypeConverter
{
	public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
	{
		if (destinationType == typeof(double) || destinationType == typeof(float) || destinationType.IsEnum || destinationType == typeof(int) || destinationType == typeof(byte) || destinationType == typeof(bool) || destinationType == typeof(Color) || destinationType == typeof(string))
		{
			return true;
		}
		return false;
	}

	public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
	{
		if (value.GetType() != typeof(string))
		{
			throw new InvalidOperationException("Can only convert from strings to other representations.");
		}
		if (destinationType == typeof(double))
		{
			return new DoubleConverter().ConvertTo(null, culture, value, destinationType);
		}
		if (destinationType == typeof(float))
		{
			return new SingleConverter().ConvertTo(null, culture, value, destinationType);
		}
		if (destinationType.IsEnum)
		{
			return Enum.Parse(destinationType, (string)value);
		}
		if (destinationType == typeof(int))
		{
			return int.Parse((string)value, NumberStyles.Integer, culture);
		}
		if (destinationType == typeof(byte))
		{
			return new ByteConverter().ConvertTo(null, culture, value, destinationType);
		}
		if (destinationType == typeof(bool))
		{
			return bool.Parse((string)value);
		}
		if (destinationType == typeof(Color))
		{
			if (string.Compare((string)value, "Empty", ignoreCase: false) == 0)
			{
				return Color.Empty;
			}
			return new ColorConverter().ConvertFromString(null, culture, (string)value);
		}
		if (destinationType == typeof(string))
		{
			return value;
		}
		return null;
	}
}
