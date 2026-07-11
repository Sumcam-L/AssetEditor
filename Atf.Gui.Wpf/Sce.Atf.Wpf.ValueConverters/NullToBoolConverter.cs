using System;
using System.Globalization;
using System.Windows.Data;
using Sce.Atf.Wpf.Markup;

namespace Sce.Atf.Wpf.ValueConverters;

[ValueConversion(typeof(bool), typeof(object))]
public class NullToBoolConverter : ConverterMarkupExtension<NullToBoolConverter>
{
	public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		return value == null;
	}

	public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		return null;
	}
}
