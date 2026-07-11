using System;
using System.Globalization;
using System.Windows.Data;
using Sce.Atf.Wpf.Markup;

namespace Sce.Atf.Wpf.ValueConverters;

[ValueConversion(typeof(object), typeof(double))]
public class ToDoubleConverter : ConverterMarkupExtension<ToDoubleConverter>
{
	public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (value == null)
		{
			return 0;
		}
		Type type = value.GetType();
		if (type == typeof(int))
		{
			return (int)value;
		}
		if (type == typeof(long))
		{
			return (long)value;
		}
		if (type == typeof(byte))
		{
			return (byte)value;
		}
		if (type == typeof(double))
		{
			return (double)value;
		}
		if (type == typeof(float))
		{
			return (float)value;
		}
		return Binding.DoNothing;
	}

	public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		return value;
	}
}
