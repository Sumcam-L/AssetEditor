using System;
using System.ComponentModel;
using System.Globalization;

namespace Firaxis.MathEx.Converters;

public class Vec2Converter : TypeConverter
{
	public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
	{
		return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
	}

	public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
	{
		return destinationType == typeof(string) || base.CanConvertTo(context, destinationType);
	}

	public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (value is string)
		{
			string[] array = ((string)value).Split(new char[2] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
			return new Vec2(Convert.ToSingle(array[0]), Convert.ToSingle(array[1]));
		}
		return base.ConvertFrom(context, culture, value);
	}

	public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
	{
		if (destinationType == typeof(string))
		{
			Vec2 vec = (Vec2)value;
			return $"{vec.X}, {vec.Y}";
		}
		return base.ConvertTo(context, culture, value, destinationType);
	}
}
