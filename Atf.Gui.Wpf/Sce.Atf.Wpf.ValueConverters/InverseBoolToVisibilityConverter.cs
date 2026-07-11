using System;
using System.Globalization;
using System.Windows;
using Sce.Atf.Wpf.Markup;

namespace Sce.Atf.Wpf.ValueConverters;

public class InverseBoolToVisibilityConverter : ConverterMarkupExtension<InverseBoolToVisibilityConverter>
{
	public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		return ((bool)value) ? Visibility.Collapsed : Visibility.Visible;
	}
}
