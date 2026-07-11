using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows.Data;
using Sce.Atf.Wpf.Markup;

namespace Sce.Atf.Wpf.ValueConverters;

[ValueConversion(typeof(object), typeof(string))]
public class EnumDescriptionConverter : ConverterMarkupExtension<EnumDescriptionConverter>
{
	public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (value == null)
		{
			return Binding.DoNothing;
		}
		string result = value.ToString();
		FieldInfo fieldInfo = value.GetType().GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.GetField).FirstOrDefault((FieldInfo f) => f.GetValue(null).Equals(value));
		if (fieldInfo != null && fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), inherit: true).FirstOrDefault() is DescriptionAttribute descriptionAttribute)
		{
			result = descriptionAttribute.Description;
		}
		return result;
	}
}
