using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Firaxis.Utility.Converters;

[ValueConversion(typeof(bool?), typeof(Visibility))]
public class InverseBoolToVis : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (value.GetType() != typeof(bool?) && value.GetType() != typeof(bool))
		{
			throw new InvalidOperationException("The source must be a Nullable<bool> or bool.");
		}
		if (targetType != typeof(Visibility))
		{
			throw new InvalidOperationException("The target must be a Visibility status.");
		}
		if (!(bool)value)
		{
			return Visibility.Visible;
		}
		return Visibility.Collapsed;
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}
