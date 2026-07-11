using System;
using System.Globalization;
using System.Windows;
using Sce.Atf.Wpf.Markup;

namespace Sce.Atf.Wpf.ValueConverters;

public class TwoWayVisibilityToBoolConverter : ConverterMarkupExtension<TwoWayVisibilityToBoolConverter>
{
	public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		return (Visibility)value == Visibility.Visible;
	}

	public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if ((string)parameter == "Hidden")
		{
			return (!(bool)value) ? Visibility.Hidden : Visibility.Visible;
		}
		return (!(bool)value) ? Visibility.Collapsed : Visibility.Visible;
	}
}
