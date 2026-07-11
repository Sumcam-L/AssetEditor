using System;
using System.Globalization;
using Sce.Atf.Wpf.Markup;

namespace Sce.Atf.Wpf.ValueConverters;

public class IsAssignableFromConverter : ConverterMarkupExtension<IsAssignableFromConverter>
{
	public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		Type type = parameter as Type;
		if (value != null && type != null)
		{
			return type.IsAssignableFrom(value.GetType());
		}
		return false;
	}
}
