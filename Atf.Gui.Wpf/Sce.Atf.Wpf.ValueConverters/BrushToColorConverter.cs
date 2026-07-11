using System;
using System.Globalization;
using System.Windows.Media;
using Sce.Atf.Wpf.Markup;

namespace Sce.Atf.Wpf.ValueConverters;

public class BrushToColorConverter : ConverterMarkupExtension<BrushToColorConverter>
{
	public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (!(value is SolidColorBrush solidColorBrush))
		{
			return Color.FromArgb(0, 0, 0, 0);
		}
		return solidColorBrush.Color;
	}
}
