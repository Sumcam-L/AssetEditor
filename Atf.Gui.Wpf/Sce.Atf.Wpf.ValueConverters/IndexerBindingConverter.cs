using System;
using System.Globalization;
using System.Reflection;
using System.Windows;
using Sce.Atf.Wpf.Markup;

namespace Sce.Atf.Wpf.ValueConverters;

public class IndexerBindingConverter : MultiConverterMarkupExtension<IndexerBindingConverter>
{
	public override object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
	{
		if (values.Length < 2)
		{
			return DependencyProperty.UnsetValue;
		}
		object obj = values[1];
		object obj2 = values[0];
		if (obj == null || obj2 == null)
		{
			return DependencyProperty.UnsetValue;
		}
		PropertyInfo[] properties = obj.GetType().GetProperties();
		foreach (PropertyInfo propertyInfo in properties)
		{
			if (propertyInfo.GetIndexParameters().Length != 0)
			{
				return propertyInfo.GetValue(obj, new object[1] { obj2 });
			}
		}
		return DependencyProperty.UnsetValue;
	}
}
