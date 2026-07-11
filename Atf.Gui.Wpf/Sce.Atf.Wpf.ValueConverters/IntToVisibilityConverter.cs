using System;
using System.Globalization;
using System.Windows;
using Sce.Atf.Wpf.Markup;

namespace Sce.Atf.Wpf.ValueConverters;

public class IntToVisibilityConverter : ConverterMarkupExtension<IntToVisibilityConverter>
{
	public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		return ((int)value <= 0) ? Visibility.Collapsed : Visibility.Visible;
	}
}
