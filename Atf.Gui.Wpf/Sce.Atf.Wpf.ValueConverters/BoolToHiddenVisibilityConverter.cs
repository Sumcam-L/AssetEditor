using System;
using System.Globalization;
using System.Windows;
using Sce.Atf.Wpf.Markup;

namespace Sce.Atf.Wpf.ValueConverters;

public class BoolToHiddenVisibilityConverter : ConverterMarkupExtension<BoolToHiddenVisibilityConverter>
{
	public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		return (!(bool)value) ? Visibility.Hidden : Visibility.Visible;
	}

	public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		return (Visibility)value == Visibility.Visible;
	}
}
