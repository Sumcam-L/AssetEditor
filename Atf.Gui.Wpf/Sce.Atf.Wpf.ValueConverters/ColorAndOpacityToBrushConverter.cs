using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using Sce.Atf.Wpf.Markup;

namespace Sce.Atf.Wpf.ValueConverters;

public class ColorAndOpacityToBrushConverter : MultiConverterMarkupExtension<ColorAndOpacityToBrushConverter>
{
	public override object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
	{
		if (values[0] is Color && values[1] is double)
		{
			Color color = (Color)values[0];
			double num = (double)values[1];
			return new SolidColorBrush(Color.FromArgb((byte)(num * 255.0), color.R, color.G, color.B));
		}
		return DependencyProperty.UnsetValue;
	}
}
