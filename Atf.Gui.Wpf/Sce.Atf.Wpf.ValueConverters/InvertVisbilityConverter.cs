using System;
using System.Globalization;
using System.Windows;
using Sce.Atf.Wpf.Markup;

namespace Sce.Atf.Wpf.ValueConverters;

public class InvertVisbilityConverter : ConverterMarkupExtension<InvertVisbilityConverter>
{
	public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (value is Visibility && (Visibility)value == Visibility.Visible)
		{
			return Visibility.Collapsed;
		}
		return Visibility.Visible;
	}
}
