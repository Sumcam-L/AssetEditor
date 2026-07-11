using System;
using System.Globalization;
using System.Windows.Data;
using Sce.Atf.Wpf.Markup;

namespace Sce.Atf.Wpf.ValueConverters;

[ValueConversion(typeof(Enum), typeof(bool))]
public class EnumToBooleanConverter : ConverterMarkupExtension<EnumToBooleanConverter>
{
	public Type EnumType { get; set; }

	public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (value == null || parameter == null)
		{
			return Binding.DoNothing;
		}
		string text = value.ToString();
		string value2 = parameter.ToString();
		return text.Equals(value2, StringComparison.OrdinalIgnoreCase);
	}

	public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (value == null || parameter == null)
		{
			return Binding.DoNothing;
		}
		try
		{
			if (System.Convert.ToBoolean(value, culture))
			{
				return Enum.Parse(EnumType, parameter.ToString());
			}
		}
		catch (ArgumentException)
		{
		}
		catch (FormatException)
		{
		}
		return Binding.DoNothing;
	}
}
