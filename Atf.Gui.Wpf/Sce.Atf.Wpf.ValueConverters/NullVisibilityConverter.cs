using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Sce.Atf.Wpf.ValueConverters;

public class NullVisibilityConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		return (value == null) ? Visibility.Collapsed : Visibility.Visible;
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		return null;
	}
}
