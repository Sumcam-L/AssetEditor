using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using Sce.Atf.Wpf.Markup;

namespace Sce.Atf.Wpf.ValueConverters;

public class ColorToBrushConverter : ConverterMarkupExtension<ColorToBrushConverter>
{
	public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (value != null)
		{
			Type type = value.GetType();
			if (type == typeof(Color))
			{
				return new SolidColorBrush((Color)value);
			}
			if (type == typeof(string))
			{
				return new SolidColorBrush(ColorUtil.ConvertFromString((string)value));
			}
		}
		return DependencyProperty.UnsetValue;
	}
}
