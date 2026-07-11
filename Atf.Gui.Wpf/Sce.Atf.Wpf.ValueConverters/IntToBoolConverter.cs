using System;
using System.Globalization;
using Sce.Atf.Wpf.Markup;

namespace Sce.Atf.Wpf.ValueConverters;

public class IntToBoolConverter : ConverterMarkupExtension<IntToBoolConverter>
{
	public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		return (int)value > 0;
	}
}
