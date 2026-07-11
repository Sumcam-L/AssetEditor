using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Sce.Atf.Wpf.Markup;

namespace Sce.Atf.Wpf.ValueConverters;

[ValueConversion(typeof(GridLength), typeof(double))]
public class DoubleToGridLengthConverter : ConverterMarkupExtension<DoubleToGridLengthConverter>
{
	public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		return ConvertBack(value, targetType, parameter, culture);
	}

	public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (targetType == typeof(GridLength))
		{
			if (value == null)
			{
				return GridLength.Auto;
			}
			if (value is double)
			{
				return new GridLength((double)value);
			}
			return GridLength.Auto;
		}
		if (targetType == typeof(double))
		{
			if (value is GridLength gridLength)
			{
				return gridLength.Value;
			}
			return double.NaN;
		}
		return null;
	}
}
