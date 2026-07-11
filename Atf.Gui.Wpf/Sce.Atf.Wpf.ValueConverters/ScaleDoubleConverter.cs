using System;
using System.Globalization;
using Sce.Atf.Wpf.Markup;

namespace Sce.Atf.Wpf.ValueConverters;

public class ScaleDoubleConverter : ConverterMarkupExtension<ScaleDoubleConverter>
{
	public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		double num = (double)value;
		double num2 = 0.75;
		if (parameter is string)
		{
			num2 = double.Parse((string)parameter);
		}
		else if (parameter != null)
		{
			num2 = (double)parameter;
		}
		return num * num2;
	}
}
