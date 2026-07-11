using System;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;

namespace Firaxis.Utility.Converters;

public class PointFConverter : TypeConverter
{
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
		if (value is string)
		{
			string[] array = ((string)value).Split(',');
			if (array.Length >= 2)
			{
				return new PointF(float.Parse(array[0]), float.Parse(array[1]));
			}
		}
		return base.ConvertFrom(context, culture, value);
	}

	public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
	{
		if (destinationType == typeof(string))
		{
			PointF pointF = (PointF)value;
			return pointF.X.ToString("0.000") + "," + pointF.Y.ToString("0.000");
		}
		return base.ConvertTo(context, culture, value, destinationType);
	}
}
