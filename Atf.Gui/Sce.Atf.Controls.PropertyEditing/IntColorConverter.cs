using System;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;

namespace Sce.Atf.Controls.PropertyEditing;

public class IntColorConverter : TypeConverter
{
	public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
	{
		return sourceType == typeof(string) || sourceType == typeof(Color);
	}

	public override bool CanConvertTo(ITypeDescriptorContext context, Type destType)
	{
		return destType == typeof(string) || destType == typeof(Color);
	}

	public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (value == null)
		{
			return null;
		}
		if (value is string)
		{
			TypeConverter converter = TypeDescriptor.GetConverter(typeof(Color));
			return ((Color)converter.ConvertFrom(context, culture, value)).ToArgb();
		}
		if (value is Color color)
		{
			return color.ToArgb();
		}
		return base.ConvertFrom(context, culture, value);
	}

	public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destType)
	{
		if (value == null)
		{
			return null;
		}
		if (value is Color color)
		{
			value = color.ToArgb();
		}
		if (value is int && destType == typeof(string))
		{
			Color color2 = Color.FromArgb((int)value);
			TypeConverter converter = TypeDescriptor.GetConverter(typeof(Color));
			return converter.ConvertTo(context, culture, color2, destType);
		}
		if (value is int && destType == typeof(Color))
		{
			return Color.FromArgb((int)value);
		}
		return base.ConvertTo(context, culture, value, destType);
	}
}
