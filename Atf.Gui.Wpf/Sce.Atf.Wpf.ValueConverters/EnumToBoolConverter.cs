using System;
using System.Globalization;
using System.Windows.Data;
using Sce.Atf.Wpf.Markup;

namespace Sce.Atf.Wpf.ValueConverters;

public class EnumToBoolConverter : ConverterMarkupExtension<EnumToBoolConverter>
{
	public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		return value.Equals(parameter);
	}

	public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		return value.Equals(false) ? Binding.DoNothing : parameter;
	}
}
