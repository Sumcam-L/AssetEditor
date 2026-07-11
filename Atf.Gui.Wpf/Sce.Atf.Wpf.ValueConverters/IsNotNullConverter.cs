using System;
using System.Globalization;
using Sce.Atf.Wpf.Markup;

namespace Sce.Atf.Wpf.ValueConverters;

public class IsNotNullConverter : ConverterMarkupExtension<IsNotNullConverter>
{
	public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		return value != null;
	}
}
