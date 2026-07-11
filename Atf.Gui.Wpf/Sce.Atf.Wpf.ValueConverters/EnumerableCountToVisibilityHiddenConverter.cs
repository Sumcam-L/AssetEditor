using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using System.Windows;
using Sce.Atf.Wpf.Markup;

namespace Sce.Atf.Wpf.ValueConverters;

public class EnumerableCountToVisibilityHiddenConverter : ConverterMarkupExtension<EnumerableCountToVisibilityHiddenConverter>
{
	public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		return (!(value is IEnumerable source) || !source.Cast<object>().Any()) ? Visibility.Hidden : Visibility.Visible;
	}
}
