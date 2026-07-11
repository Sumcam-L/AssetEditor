using System;
using System.Globalization;
using System.Windows;
using Sce.Atf.Wpf.Markup;

namespace Sce.Atf.Wpf.ValueConverters;

public class ItemNullToHiddenVisibilityConverter : ConverterMarkupExtension<ItemNullToHiddenVisibilityConverter>
{
	public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		return (value == null) ? Visibility.Hidden : Visibility.Visible;
	}
}
