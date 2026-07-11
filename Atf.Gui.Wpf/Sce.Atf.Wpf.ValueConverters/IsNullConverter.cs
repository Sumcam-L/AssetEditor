using System;
using System.Globalization;
using Sce.Atf.Wpf.Markup;

namespace Sce.Atf.Wpf.ValueConverters;

public class IsNullConverter : ConverterMarkupExtension<IsNullConverter>
{
	public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		return value == null;
	}
}
