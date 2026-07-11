using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using System.Windows;
using Sce.Atf.Wpf.Markup;

namespace Sce.Atf.Wpf.ValueConverters;

public class EnumerableNoneToVisibilityCollapsedConverter : MultiConverterMarkupExtension<EnumerableNoneToVisibilityCollapsedConverter>
{
	public override object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
	{
		return (values[0] is IEnumerable source && source.Cast<object>().Any()) ? Visibility.Collapsed : Visibility.Visible;
	}
}
