using System;
using System.Globalization;
using System.Windows.Data;
using Sce.Atf.Wpf.Markup;

namespace Sce.Atf.Wpf.ValueConverters;

[ValueConversion(typeof(Enum), typeof(string[]))]
public class EnumValuesConverter : ConverterMarkupExtension<EnumValuesConverter>
{
	public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (value != null)
		{
			return Enum.GetValues(value.GetType());
		}
		if (targetType == typeof(Enum))
		{
			return Enum.GetValues(targetType);
		}
		return value;
	}
}
