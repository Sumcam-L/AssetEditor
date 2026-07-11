using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace Sce.Atf.Wpf.ValueConverters;

[ValueConversion(typeof(object), typeof(object))]
public class TypeConverter : FrameworkElement, IValueConverter
{
	public static readonly DependencyProperty SourceTypeProperty = DependencyProperty.Register("SourceType", typeof(Type), typeof(TypeConverter));

	public static readonly DependencyProperty TargetTypeProperty = DependencyProperty.Register("TargetType", typeof(Type), typeof(TypeConverter));

	[ConstructorArgument("sourceType")]
	public Type SourceType
	{
		get
		{
			return GetValue(SourceTypeProperty) as Type;
		}
		set
		{
			SetValue(SourceTypeProperty, value);
		}
	}

	[ConstructorArgument("targetType")]
	public Type TargetType
	{
		get
		{
			return GetValue(TargetTypeProperty) as Type;
		}
		set
		{
			SetValue(TargetTypeProperty, value);
		}
	}

	public TypeConverter()
	{
	}

	public TypeConverter(Type sourceType, Type targetType)
	{
		SourceType = sourceType;
		TargetType = targetType;
	}

	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		Requires.NotNull(TargetType, "NoTargetType");
		return DoConversion(value, TargetType, culture);
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		Requires.NotNull(SourceType, "NoSourceType");
		return DoConversion(value, SourceType, culture);
	}

	private static object DoConversion(object value, Type toType, CultureInfo culture)
	{
		if (value is IConvertible || value == null)
		{
			try
			{
				return System.Convert.ChangeType(value, toType, culture);
			}
			catch (Exception)
			{
				return DependencyProperty.UnsetValue;
			}
		}
		System.ComponentModel.TypeConverter converter = TypeDescriptor.GetConverter(value);
		if (converter.CanConvertTo(toType))
		{
			return converter.ConvertTo(null, culture, value, toType);
		}
		return DependencyProperty.UnsetValue;
	}
}
