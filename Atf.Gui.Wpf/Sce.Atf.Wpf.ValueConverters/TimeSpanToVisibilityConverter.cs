using System;
using System.Globalization;
using System.Windows;
using Sce.Atf.Wpf.Markup;

namespace Sce.Atf.Wpf.ValueConverters;

public class TimeSpanToVisibilityConverter : ConverterMarkupExtension<TimeSpanToVisibilityConverter>
{
	public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (value is TimeSpan timeSpan)
		{
			return (timeSpan.Ticks == 0L) ? Visibility.Collapsed : Visibility.Visible;
		}
		if (value is double num)
		{
			return (Math.Abs(num - 0.0) < 1E-05) ? Visibility.Collapsed : Visibility.Visible;
		}
		return DependencyProperty.UnsetValue;
	}
}
