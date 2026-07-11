using System;
using System.Collections;
using System.Globalization;
using System.Text;
using Sce.Atf.Wpf.Markup;

namespace Sce.Atf.Wpf.ValueConverters;

public class ListToStringConverter : ConverterMarkupExtension<ListToStringConverter>
{
	public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (value is IEnumerable enumerable)
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (object item in enumerable)
			{
				stringBuilder.AppendLine(item.ToString());
			}
			return stringBuilder.ToString();
		}
		return null;
	}
}
