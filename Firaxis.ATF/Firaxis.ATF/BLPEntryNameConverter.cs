using System;
using System.ComponentModel;
using System.Globalization;

namespace Firaxis.ATF;

public class BLPEntryNameConverter : TypeConverter
{
	public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
	{
		if (sourceType == typeof(BLPData))
		{
			return true;
		}
		return base.CanConvertFrom(context, sourceType);
	}

	public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (value is BLPData)
		{
			return ((BLPData)value).Name;
		}
		return base.ConvertFrom(context, culture, value);
	}

	public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
	{
		if (destinationType == typeof(string) && value.GetType() == typeof(BLPData))
		{
			return ((BLPData)value).Name;
		}
		return base.ConvertTo(context, culture, value, destinationType);
	}
}
