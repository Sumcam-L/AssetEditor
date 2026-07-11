using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Sce.Atf.Wpf.ValueConverters;

public class BoolVisibilityConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (value != null)
		{
			return (!(bool)value) ? Visibility.Collapsed : Visibility.Visible;
		}
		return Visibility.Collapsed;
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (value != null)
		{
			return (Visibility)value == Visibility.Visible;
		}
		return false;
	}
}
