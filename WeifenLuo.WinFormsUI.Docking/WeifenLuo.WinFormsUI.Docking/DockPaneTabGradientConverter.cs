using System;
using System.ComponentModel;
using System.Globalization;

namespace WeifenLuo.WinFormsUI.Docking;

public class DockPaneTabGradientConverter : ExpandableObjectConverter
{
	public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
	{
		if (destinationType == typeof(TabGradient))
		{
			return true;
		}
		return base.CanConvertTo(context, destinationType);
	}

	public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
	{
		TabGradient tabGradient = value as TabGradient;
		if (destinationType == typeof(string) && tabGradient != null)
		{
			return "DockPaneTabGradient";
		}
		return base.ConvertTo(context, culture, value, destinationType);
	}
}
