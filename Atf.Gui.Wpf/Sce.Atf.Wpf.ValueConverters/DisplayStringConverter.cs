using System;
using System.Globalization;
using Sce.Atf.Wpf.Markup;

namespace Sce.Atf.Wpf.ValueConverters;

public class DisplayStringConverter : ConverterMarkupExtension<DisplayStringConverter>
{
	public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (value != null)
		{
			Type type = value.GetType();
			if (type.IsEnum)
			{
				return EnumDisplayUtil.GetDisplayString(type, value);
			}
			return value.ToString();
		}
		return value;
	}
}
