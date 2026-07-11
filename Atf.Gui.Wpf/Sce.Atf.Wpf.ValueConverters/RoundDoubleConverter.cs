using System;
using System.Globalization;
using System.Linq;
using Sce.Atf.Wpf.Markup;

namespace Sce.Atf.Wpf.ValueConverters;

public class RoundDoubleConverter : ConverterMarkupExtension<RoundDoubleConverter>
{
	public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		double value2 = 0.0;
		int num = 1;
		if (value is string)
		{
			value2 = double.Parse((string)value);
		}
		else if (value is double)
		{
			value2 = (double)value;
		}
		else if (value is float)
		{
			value2 = (float)value;
		}
		if (parameter is string)
		{
			num = int.Parse((string)parameter);
		}
		else if (parameter != null)
		{
			num = (int)parameter;
		}
		Requires.Require<ArgumentException>(num >= 0, "parameter");
		if (targetType == typeof(string))
		{
			string text = new string(Enumerable.Repeat('0', num).ToArray());
			return value2.ToString("0." + text);
		}
		value2 = Math.Round(value2, num);
		return value2;
	}

	public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		return Convert(value, targetType, parameter, culture);
	}
}
