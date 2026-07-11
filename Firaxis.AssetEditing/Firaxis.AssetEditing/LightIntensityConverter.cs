using System;
using System.ComponentModel;
using System.Globalization;

namespace Firaxis.AssetEditing;

public class LightIntensityConverter : TypeConverter
{
	private readonly float m_multiplier;

	public LightIntensityConverter(float multiplier)
	{
		m_multiplier = multiplier;
	}

	public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
	{
		return sourceType == typeof(float);
	}

	public override bool CanConvertTo(ITypeDescriptorContext context, Type destType)
	{
		return destType == typeof(float);
	}

	public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (value == null)
		{
			return null;
		}
		if (value is float)
		{
			return (float)value / m_multiplier;
		}
		return base.ConvertFrom(context, culture, value);
	}

	public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destType)
	{
		if (value == null)
		{
			return null;
		}
		if (value is float)
		{
			return (float)value * m_multiplier;
		}
		return base.ConvertTo(context, culture, value, destType);
	}
}
