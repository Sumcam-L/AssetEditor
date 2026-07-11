using System;
using System.ComponentModel;
using System.Globalization;

namespace Sce.Atf.Applications.NetworkTargetServices.Psp2TmApilib;

internal class PowerStatusEnumConverter : TypeConverter
{
	public override bool CanConvertFrom(ITypeDescriptorContext context, Type srcType)
	{
		return false;
	}

	public override bool CanConvertTo(ITypeDescriptorContext context, Type destType)
	{
		return destType == typeof(string);
	}

	public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destType)
	{
		if (value is ePowerStatus && destType == typeof(string))
		{
			switch ((ePowerStatus)value)
			{
			case ePowerStatus.POWER_STATUS_NO_SUPPLY:
				return "No Supply";
			case ePowerStatus.POWER_STATUS_OFF:
				return "Off";
			case ePowerStatus.POWER_STATUS_ON:
				return "On";
			case ePowerStatus.POWER_STATUS_SUSPENDED:
				return "Suspended";
			}
		}
		return base.ConvertTo(context, culture, value, destType);
	}
}
