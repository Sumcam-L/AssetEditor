using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Sce.Atf.Collections;

namespace Sce.Atf.Wpf.ValueConverters;

public class AdaptingCollectionValueConverter<T, U> : IValueConverter where T : class where U : class
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (value is IObservableCollection<T> collection)
		{
			return new AdaptableObservableCollection<T, U>(collection);
		}
		return DependencyProperty.UnsetValue;
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		throw new NotSupportedException();
	}
}
