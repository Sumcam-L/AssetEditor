using System;
using System.Globalization;
using System.Windows.Data;
using Sce.Atf.Adaptation;

namespace Sce.Atf.Wpf.ValueConverters;

public class AdaptingValueConverter<T, U> : IValueConverter where T : class where U : class
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		return value.As<U>();
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		return value.As<T>();
	}
}
